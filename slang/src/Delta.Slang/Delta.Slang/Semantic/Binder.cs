using System;
using System.Collections.Generic;
using System.Linq;
using Delta.Slang.Symbols;
using Delta.Slang.Syntax;

namespace Delta.Slang.Semantic
{
    internal sealed class Binder
    {
        private readonly DiagnosticCollection diagnostics;

        public Binder(Scope parent, FunctionSymbol function)
        {
            Scope = parent ?? throw new ArgumentNullException(nameof(parent));
            Function = function;
            diagnostics = new DiagnosticCollection();
        }

        private Scope Scope { get; set; }
        private FunctionSymbol Function { get; }

        public IEnumerable<IDiagnostic> Diagnostics => diagnostics;

        public static BoundTree BindCompilationUnit(CompilationUnitNode compilationUnit)
        {
            var rootScope = CreateRootScope();
            var binder = new Binder(rootScope, function: null);

            // First let's add the functions
            var functions = new List<FunctionDefinition>();
            foreach (var functionDeclarationNode in compilationUnit.Members.OfType<FunctionDeclarationNode>())
            {
                var declaration = binder.BindFunctionDeclaration(functionDeclarationNode);
                if (declaration == null) // Most probably function re-definition
                    continue;

                var bodyBinder = new Binder(rootScope, declaration);
                var body = bodyBinder.BindBlock(functionDeclarationNode.Body, isFunctionTopmostBlock: true);
                bodyBinder.FixGotoStatements(body);

                // TODO: lower + check the function returns on all its paths

                // Retrieve the diagnostics
                _ = binder.diagnostics.AddRange(bodyBinder.Diagnostics);

                var function = new FunctionDefinition(declaration, body);
                functions.Add(function);
            }

            // The the free-floating code (global statements, including global variables)
            var statements = new List<Statement>();
            foreach (var globalStatement in compilationUnit.Members.OfType<GlobalStatementNode>())
            {
                var statement = binder.BindStatement(globalStatement.Statement);
                statements.Add(statement);
            }

            return new BoundTree(
                rootScope,
                functions,
                binder.Scope.GetDeclaredVariables(),
                statements,
                binder.Diagnostics);
        }

        private void FixGotoStatements(Statement statement)
        {
            if (statement is GotoStatement g && !g.IsValid)
            {
                var labelName = g.GotoStatementNode.Label.Identifier.Text;
                if (statement.Scope.TryLookupLabel(labelName, out var label))
                    g.Fix(label);
                else
                    diagnostics.ReportUndefinedLabel(g.GotoStatementNode.MainToken, labelName);
                return;
            }

            if (statement is IHasChildStatements withChildren)
            {
                foreach (var child in withChildren.Statements)
                    FixGotoStatements(child);
            }
        }

        private FunctionSymbol BindFunctionDeclaration(FunctionDeclarationNode node)
        {
            var seenParameterNames = new HashSet<string>();

            var parameters = new List<ParameterSymbol>();
            foreach (var parameterNode in node.ParametersDeclaration.Parameters)
            {
                var parameterName = parameterNode.MainToken.Text;
                var parameterType = BindTypeClause(parameterNode.Type);
                if (seenParameterNames.Add(parameterName))
                {
                    var parameter = new ParameterSymbol(parameterName, parameterType);
                    parameters.Add(parameter);
                }
                else diagnostics.ReportParameterAlreadyDeclared(parameterNode.MainToken, parameterName);
            }

            var type = BindTypeClause(node.ReturnType) ?? BuiltinTypes.Void;

            var function = new FunctionSymbol(node.FunctionName.Text, parameters, type, node);
            if (Scope.TryDeclareFunction(function))
                return function;

            diagnostics.ReportSymbolAlreadyDeclared(node.FunctionName, function.Name, "Function");
            return null;
        }

        private TypeSymbol BindTypeClause(TypeClauseNode node)
        {
            if (node == null)
                return null;

            if (Scope.TryLookupType(node.TypeName.Text, out var type))
                return type;

            diagnostics.ReportUndefinedType(node.TypeName, node.TypeName.Text);
            return null;
        }

        private Statement BindStatement(StatementNode node)
        {
            switch (node)
            {
                case BlockNode b: return BindBlock(b);
                case VariableDeclarationNode vd: return BindVariableDeclaration(vd);
                case ExpressionStatementNode es: return BindExpressionStatement(es);
                case GotoStatementNode gs: return BindGotoStatement(gs);
                case LabelStatementNode ls: return BindLabelStatement(ls);
                case IfStatementNode ifs: return BindIfStatement(ifs);
                case ReturnStatementNode rs: return BindReturnStatement(rs);
            }

            return new InvalidStatement();
        }

        private Statement BindReturnStatement(ReturnStatementNode syntax)
        {
            if (Function == null)
            {
                diagnostics.ReportInvalidReturn(syntax.ReturnToken);
                return new InvalidStatement();
            }

            var expression = syntax.Expression == null ? null : BindExpression(syntax.Expression);
            var expressionType = expression?.Type ?? BuiltinTypes.Void;
            if (expressionType != Function.Type)
            {
                if (expression == null)
                    diagnostics.ReportInconsistentReturnType(syntax.ReturnToken, Function, Function.Type, expressionType);
                else
                {
                    expression = BindConversion(syntax.Expression, expression, Function.Type, noDiagnostics: true);
                    if (expression is InvalidExpression)
                        diagnostics.ReportInconsistentReturnType(syntax.ReturnToken, Function, Function.Type, expressionType);
                }
            }

            return new ReturnStatement(Scope, expression);
        }

        private Statement BindExpressionStatement(ExpressionStatementNode node)
        {
            var expression = BindExpression(node.Expression, allowVoid: true);
            return new ExpressionStatement(Scope, expression);
        }

        private Block BindBlock(BlockNode node, bool isFunctionTopmostBlock = false)
        {
            var statements = new List<Statement>();
            Scope = new Scope(isFunctionTopmostBlock ? ScopeKind.Function : ScopeKind.Block, Scope);
            try
            {
                foreach (var statementNode in node.Statements)
                {
                    var statement = BindStatement(statementNode);
                    statements.Add(statement);
                }

                return new Block(Scope, statements);
            }
            finally
            {
                Scope = Scope.Parent;
            }
        }

        private VariableDeclaration BindVariableDeclaration(VariableDeclarationNode node)
        {
            const bool isReadOnly = false;
            var variableType = BindTypeClause(node.Type);
            var initializer = node.Initializer == null ? null : BindExpression(node.Initializer);
            if (variableType == null && initializer != null) // infer the variable type from its initializer
                variableType = initializer.Type;

            var variable = BindVariable(node.VariableName, isReadOnly, variableType);

            var realInitializer = initializer == null ? BindDefaultInitializer(variableType) : BindConversion(node.Initializer, initializer, variableType);
            return new VariableDeclaration(Scope, variable, realInitializer);
        }

        private Statement BindLabelStatement(LabelStatementNode node)
        {
            var name = node.Label.Identifier.Text;

            if (Function == null)
            {
                diagnostics.ReportInvalidLabelDeclaration(node.MainToken, name);
                return new InvalidStatement();
            }

            var label = new LabelSymbol(name);
            if (!Scope.TryDeclareLabel(label))
                diagnostics.ReportSymbolAlreadyDeclared(node.MainToken, name, "Label");

            return new LabelStatement(Scope, label);
        }

        private Statement BindGotoStatement(GotoStatementNode node)
        {
            var targetLabelName = node.Label.Identifier.Text;

            // Well, maybe the label is defined further in the code.
            // We'll try to fix this once we are completely finished binding the function
            _ = Scope.TryLookupLabel(targetLabelName, out var label);
            return new GotoStatement(Scope, node, label);
        }

        private IfStatement BindIfStatement(IfStatementNode node)
        {
            var condition = BindExpression(node.Condition, BuiltinTypes.Bool);
            var thenStatement = BindStatement(node.Statement);
            var elseStatement = node.Else == null ? null : BindStatement(node.Else.Statement);
            return new IfStatement(Scope, condition, thenStatement, elseStatement);
        }

        private Expression BindExpression(ExpressionNode node, TypeSymbol expectedType) => BindConversion(node, expectedType);

        private Expression BindExpression(ExpressionNode node, bool allowVoid = false)
        {
            var result = DoBindExpression(node);
            if (!allowVoid && result.Type == BuiltinTypes.Void)
            {
                diagnostics.ReportExpressionMustHaveValue(node.MainToken);
                return new InvalidExpression(result);
            }

            return result;
        }

        private Expression DoBindExpression(ExpressionNode node)
        {
            switch (node)
            {
                case ParenthesizedExpressionNode pe:
                    return BindParenthesizedExpression(pe);
                case LiteralExpressionNode le:
                    return BindLiteralExpression(le);
                case NameExpressionNode ne:
                    return BindNameExpression(ne);
                case AssignmentExpressionNode ae:
                    return BindAssignmentExpression(ae);
                case UnaryExpressionNode ue:
                    return BindUnaryExpression(ue);
                case BinaryExpressionNode be:
                    return BindBinaryExpression(be);
                case InvokeExpressionNode ie:
                    return BindInvokeExpression(ie);
                default:
                    throw new ArgumentException($"Unexpected Expression Node: '{node.Kind}'", nameof(node));
            }
        }

        private Expression BindParenthesizedExpression(ParenthesizedExpressionNode node) => DoBindExpression(node);

        private Expression BindLiteralExpression(LiteralExpressionNode node) => new LiteralExpression(node.Type, node.Value);

        private Expression BindNameExpression(NameExpressionNode node)
        {
            VariableExpression variableExpression = null;
            var name = node.Identifier.Text;
            if (Scope.TryLookupVariable(name, out var variable))
                variableExpression = new VariableExpression(variable);

            if (variableExpression == null || node.Identifier.IsForged)
            {
                diagnostics.ReportUndefinedVariable(node.Identifier, name);
                return new InvalidExpression(variableExpression);
            }

            return variableExpression;
        }

        private Expression BindAssignmentExpression(AssignmentExpressionNode node)
        {
            var name = node.Identifier.Text;
            var boundExpression = BindExpression(node.Expression);

            if (!Scope.TryLookupVariable(name, out var variable))
            {
                diagnostics.ReportUndefinedVariable(node.Identifier, name);
                return boundExpression;
            }

            if (variable.IsReadOnly)
                diagnostics.ReportInvalidAssignmentToReadOnlyVariable(node.Equal, name);

            var convertedExpression = BindConversion(node.Expression, boundExpression, variable.Type);
            return new AssignmentExpression(variable, convertedExpression);
        }

        private Expression BindUnaryExpression(UnaryExpressionNode node)
        {
            var boundOperand = BindExpression(node.Operand);

            if (boundOperand.Type == BuiltinTypes.Invalid)
                return new InvalidExpression(null);

            var boundOperator = UnaryOperatorBinder.Bind(node.Operator.Kind, boundOperand.Type);
            if (boundOperator == null)
            {
                diagnostics.ReportUndefinedUnaryOperator(node.Operator, node.Operator.Text, boundOperand.Type);
                return new InvalidExpression(null);
            }

            return new UnaryExpression(boundOperator, boundOperand);
        }

        private Expression BindBinaryExpression(BinaryExpressionNode node)
        {
            var boundLhs = BindExpression(node.Left);
            var boundRhs = BindExpression(node.Right);

            if (boundLhs.Type == BuiltinTypes.Invalid || boundRhs.Type == BuiltinTypes.Invalid)
                return new InvalidExpression(null);

            var boundOperator = BinaryOperatorBinder.Bind(node.Operator.Kind, boundLhs.Type, boundRhs.Type);
            if (boundOperator == null)
            {
                diagnostics.ReportUndefinedBinaryOperator(node.Operator, node.Operator.Text, boundLhs.Type, boundRhs.Type);
                return new InvalidExpression(null);
            }

            return new BinaryExpression(boundLhs, boundOperator, boundRhs);
        }

        private Expression BindInvokeExpression(InvokeExpressionNode node)
        {
            // This takes care of explicit conversions
            var argumentNodes = node.Arguments.ToArray();
            if (argumentNodes.Length == 1 && Scope.TryLookupType(node.FunctionName.Text, out var type))
                return BindConversion(argumentNodes[0], type, allowExplicit: true);

            var candidates = Scope.LookupFunctions(node.FunctionName.Text).ToArray();
            if (candidates.Length == 0)
            {
                diagnostics.ReportUndefinedFunction(node.FunctionName, node.FunctionName.Text);
                return new InvalidExpression(null);
            }

            var boundArguments = new List<Expression>();
            foreach (var argumentNode in argumentNodes)
            {
                var boundArgument = BindExpression(argumentNode);
                boundArguments.Add(boundArgument);
            }

            var arguments = boundArguments.ToArray();

            // Now let's first look for a definition with the exact same parameters
            foreach (var candidate in candidates)
            {
                var found = MatchStrictOverload(candidate, arguments);
                if (found != null)
                    return new InvokeExpression(candidate, arguments);
            }

            // No exact parameter match, let's see if we can find functions that would be compatible with implicit conversions            
            InvokeExpression invokeExpression = null;
            var findCount = 0;
            foreach (var candidate in candidates)
            {
                var found = MatchCompatibleOverload(node, candidate, arguments, out var convertedArguments);
                if (found != null)
                {
                    invokeExpression = new InvokeExpression(found, convertedArguments);
                    findCount++;
                    if (findCount > 1)
                    {
                        diagnostics.ReportAmbiguousFunction(
                            node.FunctionName, node.FunctionName.Text, arguments.Select(a => a.Type.Name).ToArray());
                        return new InvalidExpression(invokeExpression);
                    }
                }
            }

            // No compatible signature was found
            if (invokeExpression == null)
            {
                diagnostics.ReportUndefinedFunctionWithArguments(
                    node.FunctionName, node.FunctionName.Text, arguments.Select(a => a.Type.Name).ToArray());
                return new InvalidExpression(new InvokeExpression(candidates.First(), arguments));
            }

            // OK, we found one and only one matching overload
            return invokeExpression;
        }

        private FunctionSymbol MatchStrictOverload(FunctionSymbol candidate, Expression[] arguments)
        {
            var parameters = candidate.Parameters.ToArray();
            if (arguments.Length != parameters.Length) return null;

            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                var parameter = parameters[i];
                if (argument.Type != parameter.Type)
                    return null;
            }

            return candidate;
        }

        private FunctionSymbol MatchCompatibleOverload(ExpressionNode node, FunctionSymbol candidate, Expression[] arguments, out Expression[] convertedArguments)
        {
            var parameters = candidate.Parameters.ToArray();
            if (arguments.Length != parameters.Length)
            {
                convertedArguments = null;
                return null;
            }

            convertedArguments = new Expression[arguments.Length];

            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                var parameter = parameters[i];

                var conversionExpression = BindConversion(node, argument, parameter.Type, allowExplicit: false, noDiagnostics: true);
                if (conversionExpression is InvalidExpression)
                    return null;

                convertedArguments[i] = conversionExpression;
            }

            return candidate;
        }

        private Expression BindConversion(ExpressionNode node, TypeSymbol type, bool allowExplicit = false)
        {
            var expression = BindExpression(node);
            return BindConversion(node, expression, type, allowExplicit);
        }

        private Expression BindConversion(SyntaxNode node, Expression fromExpression, TypeSymbol toType, bool allowExplicit = false, bool noDiagnostics = false)
        {
            // We can always build the expression; however, it is not always valid!
            var conversionExpression = new ConversionExpression(toType, fromExpression);

            var conversion = Conversions.GetConversionKind(fromExpression.Type, toType);
            if (!conversion.Exists)
            {
                if (!noDiagnostics && fromExpression.Type != BuiltinTypes.Invalid && toType != BuiltinTypes.Invalid)
                    diagnostics.ReportImpossibleConversion(node.MainToken, fromExpression.Type, toType);

                return new InvalidExpression(conversionExpression);
            }

            if (!allowExplicit && conversion.IsExplicit)
            {
                if (!noDiagnostics)
                    diagnostics.ReportImpossibleImplicitConversion(node.MainToken, fromExpression.Type, toType);
                return new InvalidExpression(conversionExpression);
            }

            return conversion.IsIdentity ? fromExpression : conversionExpression;
        }

        private Expression BindDefaultInitializer(TypeSymbol type) => new LiteralExpression(type, type.DefaultValue);

        private VariableSymbol BindVariable(Token identifier, bool isReadOnly, TypeSymbol type)
        {
            var name = identifier.Text;
            var variable = Function == null ?
                (VariableSymbol)new GlobalVariableSymbol(name, isReadOnly, type) :
                new LocalVariableSymbol(name, isReadOnly, type);

            if (!Scope.TryDeclareVariable(variable))
                diagnostics.ReportSymbolAlreadyDeclared(identifier, name, "Variable");

            return variable;
        }

        private static Scope CreateRootScope()
        {
            var result = new Scope(ScopeKind.Global);

            foreach (var f in BuiltinFunctions.All)
                _ = result.TryDeclareFunction(f);

            return result;
        }
    }
}
