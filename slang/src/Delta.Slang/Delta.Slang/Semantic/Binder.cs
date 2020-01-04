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
                var body = bodyBinder.BindStatement(functionDeclarationNode.Body);

                // TODO: lower + check the function returns on all its paths

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
                functions,
                binder.Scope.GetDeclaredVariables(),
                statements,
                binder.Diagnostics);
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

            diagnostics.ReportSymbolAlreadyDeclared(node.FunctionName, function.Name);
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
                case IfStatementNode ifs: return BindIfStatement(ifs);
            }

            return new InvalidStatement();
        }

        private Statement BindExpressionStatement(ExpressionStatementNode node)
        {
            var expression = BindExpression(node.Expression, allowVoid: true);
            return new ExpressionStatement(expression);
        }

        private Block BindBlock(BlockNode node)
        {
            var statements = new List<Statement>();
            Scope = new Scope(Scope);
            try
            {
                foreach (var statementNode in node.Statements)
                {
                    var statement = BindStatement(statementNode);
                    statements.Add(statement);
                }
            }
            finally
            {
                Scope = Scope.Parent;
            }

            return new Block(statements);
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
            return new VariableDeclaration(variable, realInitializer);
        }

        private IfStatement BindIfStatement(IfStatementNode syntax)
        {
            var condition = BindExpression(syntax.Condition, BuiltinTypes.Boolean);
            var thenStatement = BindStatement(syntax.Statement);
            var elseStatement = syntax.Else == null ? null : BindStatement(syntax.Else.Statement);
            return new IfStatement(condition, thenStatement, elseStatement);
        }

        private Expression BindExpression(ExpressionNode node, TypeSymbol expectedType) => BindConversion(node, expectedType);

        private Expression BindExpression(ExpressionNode node, bool allowVoid = false)
        {
            var result = DoBindExpression(node);
            if (!allowVoid && result.Type == BuiltinTypes.Void)
            {
                diagnostics.ReportExpressionMustHaveValue(node.MainToken);
                return new InvalidExpression();
            }

            return result;
        }

        private Expression DoBindExpression(ExpressionNode node)
        {
            switch(node)
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
            if (node.Identifier.IsForged) // This means the token was inserted by the parser.
                return new InvalidExpression();

            var name = node.Identifier.Text;
            if (!Scope.TryLookupVariable(name, out var variable))
            {
                diagnostics.ReportUndefinedVariable(node.Identifier, name);
                return new InvalidExpression();
            }

            return new VariableExpression(variable);
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
                return new InvalidExpression();

            var boundOperator = UnaryOperatorBinder.Bind(node.Operator.Kind, boundOperand.Type);
            if (boundOperator == null)
            {
                diagnostics.ReportUndefinedUnaryOperator(node.Operator, node.Operator.Text, boundOperand.Type);
                return new InvalidExpression();
            }

            return new UnaryExpression(boundOperator, boundOperand);
        }

        private Expression BindBinaryExpression(BinaryExpressionNode node)
        {
            var boundLhs = BindExpression(node.Left);
            var boundRhs = BindExpression(node.Right);

            if (boundLhs.Type == BuiltinTypes.Invalid || boundRhs.Type == BuiltinTypes.Invalid)
                return new InvalidExpression();

            var boundOperator = BinaryOperatorBinder.Bind(node.Operator.Kind, boundLhs.Type, boundRhs.Type);
            if (boundOperator == null)
            {
                diagnostics.ReportUndefinedBinaryOperator(node.Operator, node.Operator.Text, boundLhs.Type, boundRhs.Type);
                return new InvalidExpression();
            }

            return new BinaryExpression(boundLhs, boundOperator, boundRhs);
        }

        private Expression BindInvokeExpression(InvokeExpressionNode syntax)
        {
            // This takes care of explicit conversions
            var arguments = syntax.Arguments.ToArray();
            if (arguments.Length == 1 && Scope.TryLookupType(syntax.FunctionName.Text, out var type))
                return BindConversion(arguments[0], type, allowExplicit: true);

            var boundArguments = new List<Expression>();
            foreach (var argument in syntax.Arguments)
            {
                var boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }

            if (!Scope.TryLookupFunction(syntax.FunctionName.Text, out var function))
            {
                diagnostics.ReportUndefinedFunction(syntax.FunctionName, syntax.FunctionName.Text);
                return new InvalidExpression();
            }

            var parameters = function.Parameters.ToArray();
            if (arguments.Length != parameters.Length)
            {
                // OK, let's try to report the error at the best location possible
                Token where;
                if (arguments.Length > parameters.Length)
                    where = syntax.CloseParenthesis;
                else
                    where = parameters.Length == 0 ?
                        arguments[0].MainToken :
                        arguments[parameters.Length - 1].MainToken;

                diagnostics.ReportWrongArgumentCount(where, function.Name, parameters.Length, arguments.Length);
                return new InvalidExpression();
            }

            var hasErrors = false;
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = boundArguments[i];
                var parameter = parameters[i];

                if (argument.Type != parameter.Type)
                {
                    if (argument.Type != BuiltinTypes.Invalid)
                        diagnostics.ReportWrongArgumentType(arguments[i].MainToken, parameter.Name, function.Name, parameter.Type, argument.Type);
                    hasErrors = true;
                }
            }

            return hasErrors ? (Expression)new InvalidExpression() : new InvokeExpression(function, boundArguments);
        }

        private Expression BindConversion(ExpressionNode node, TypeSymbol type, bool allowExplicit = false)
        {
            var expression = BindExpression(node);
            return BindConversion(node, expression, type, allowExplicit);
        }

        private Expression BindConversion(SyntaxNode sourceNode, Expression expression, TypeSymbol type, bool allowExplicit = false)
        {
            var conversion = Conversions.GetConversionKind(expression.Type, type);

            if (!conversion.Exists)
            {
                if (expression.Type != BuiltinTypes.Invalid && type != BuiltinTypes.Invalid)
                    diagnostics.ReportImpossibleConversion(sourceNode.MainToken, expression.Type, type);

                return new InvalidExpression();
            }

            if (!allowExplicit && conversion.IsExplicit)
                diagnostics.ReportImpossibleImplicitConversion(sourceNode.MainToken, expression.Type, type);

            return conversion.IsIdentity ? expression : new ConversionExpression(type, expression);
        }

        private Expression BindDefaultInitializer(TypeSymbol type) => new LiteralExpression(type, type.DefaultValue);

        private VariableSymbol BindVariable(Token identifier, bool isReadOnly, TypeSymbol type)
        {
            var name = identifier.Text;
            var variable = Function == null ?
                (VariableSymbol)new GlobalVariableSymbol(name, isReadOnly, type) :
                new LocalVariableSymbol(name, isReadOnly, type);

            if (!Scope.TryDeclareVariable(variable))
                diagnostics.ReportSymbolAlreadyDeclared(identifier, name);

            return variable;
        }

        private static Scope CreateRootScope()
        {
            var result = new Scope(null);

            foreach (var f in BuiltinFunctions.All)
                _ = result.TryDeclareFunction(f);

            return result;
        }
    }
}
