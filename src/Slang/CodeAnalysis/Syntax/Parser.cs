using System;
using System.Collections.Generic;
using System.Linq;
using Slang.CodeAnalysis.Text;

namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;

    public sealed class Parser
    {
        private readonly Token[] source;
        private readonly IDiagnosticSink diagnostics;
        private int position;

        public Parser(IEnumerable<Token> tokens, IDiagnosticSink diagnosticSink)
        {
            source = tokens
                .Where(t => t.Category != TokenCategory.Trivia)
                .ToArray();

            diagnostics = diagnosticSink;
        }

        private Token Current => Peek(0);

        public ParseTree Parse()
        {
            try
            {
                return ParseTokens();
            }
            catch (Exception ex)
            {
                diagnostics.ReportParserException(ex);
                return new(new InvalidNode(
                    new Token(InvalidToken, TokenCategory.Terminal, new TextSpan(0, 1), new LinePosition(0, 0), "")));
            }
        }

        private ParseTree ParseTokens()
        {
            var statements = new List<StatementNode>();
            while (!IsAtEnd())
                statements.Add(ParseDeclarationOrStatement());

            var compilationUnit = new CompilationUnitNode(statements.ToArray());
            return new ParseTree(compilationUnit);
        }

        private StatementNode ParseDeclarationOrStatement()
        {
            // NB: we separate declarations from other statements
            // Because we don't want to allow a variable declaration inside an if unless it is in a block
            try
            {
                if (Current.Kind is VarToken or ValToken)
                    return ParseDeclaration();

                // Otherwise, let's parse other statements
                return ParseStatement();
            }
            catch (ParserException pex)
            {
                return Sync(pex);
            }
        }

        private StatementNode ParseStatement()
        {
            try
            {
                if (Current.Kind == SemicolonToken) // Empty Statement
                    return ParseEmptyStatement();

                if (Current.Kind == IfToken)
                    return ParseIf();

                if (Current.Kind == LeftBraceToken)
                    return ParseBlock();

                if (Current.Kind is PrintToken or PrintlnToken)
                    return ParsePrintStatement();

                // Otherwise, let's go to expressions
                var expression = ParseExpression();
                _ = ConsumeIfMatches(SemicolonToken);
                return expression;
            }
            catch (ParserException pex)
            {
                return Sync(pex);
            }
        }

        private StatementNode ParseEmptyStatement()
        {
            _ = Consume();
            return new EmptyNode();
        }

        private StatementNode ParseIf()
        {
            _ = Consume();
            _ = ConsumeIfMatches(LeftParenToken);
            var condition = ParseExpression();
            _ = ConsumeIfMatches(RightParenToken);

            var then = ParseStatement();
            StatementNode? @else = null;
            if (Current.Kind == ElseToken)
            {
                _ = Consume();
                @else = ParseStatement();
            }

            return new IfNode(condition, then, @else);
        }

        private StatementNode ParseBlock()
        {
            _ = Consume();

            var statements = new List<StatementNode>();
            while (Current.Kind != RightBraceToken && !IsAtEnd())
                statements.Add(ParseDeclarationOrStatement());

            _ = ConsumeIfMatches(RightBraceToken);

            return new BlockNode(statements.ToArray());
        }

        private StatementNode ParseDeclaration()
        {
            var token = Consume(); // var or val
            var name = ConsumeIfMatches(IdentifierToken);

            var isReadOnly = token.Kind == ValToken;

            ExpressionNode? initializer = null;
            if (Current.Kind == EqualToken)
            {
                _ = Consume(); // =
                initializer = ParseExpression();
            }

            if (isReadOnly && initializer == null)
                throw ParserException.MakeMissingVariableInitialization(Current);

            _ = ConsumeIfMatches(SemicolonToken);
            return new VariableDeclarationNode(name, isReadOnly, initializer);
        }

        private StatementNode ParsePrintStatement()
        {
            // Because we want this to look like a function call, we expect '(' expr ')' and a final ';'
            var token = Consume(); // Consumes 'print'
            _ = ConsumeIfMatches(LeftParenToken);
            var argument = ParseExpression();
            _ = ConsumeIfMatches(RightParenToken);
            _ = ConsumeIfMatches(SemicolonToken);

            return new PrintNode(argument, token.Kind == PrintlnToken);
        }

        private ExpressionNode ParseExpression(int parentPrecedence = 0)
        {
            ExpressionNode lhs;

            var uprecedence = Current.GetUnaryOperatorPrecedence();

            // For now, all unary ops have the same precedence; hence this test
            // is meant to distinguish unary ops from other tokens
            if (uprecedence >= parentPrecedence)
            {
                var op = Consume();
                var operand = ParseExpression(uprecedence);
                lhs = new UnaryNode(op, operand);
            }
            else lhs = ParsePrimaryExpression();

            while (true)
            {
                var bprecedence = Current.GetBinaryOperatorPrecedence();

                // If we use <, the resulting expression tree leans on the right
                // If we use <=, it leans on the left
                // For example, with the 1+2+3 expression, in the first case, we obtain:
                //
                //     +
                //   1   +
                //      2 3 
                //
                // Or: 1+2+3 <=> 1+(2+3)
                //
                // And in the second case:
                //
                //     +
                //   +    3
                //  1 2 
                //
                // Or: 1+2+3 <=> (1+2)+3
                //
                // In order to evaluate first what's leftmost, we prefer the second case.
                //
                // NB: assignment is a special case, we want to be able to chain them in a right-associative way:
                //
                // a=b=42 <=> a=(b=42) 

                var op = Current;

                if (op.Kind == EqualToken)
                {
                    // Assignment is right-associative!
                    if (bprecedence < parentPrecedence)
                        break;
                }
                else
                {
                    // Other binary operators are left-associative
                    if (bprecedence <= parentPrecedence)
                        break;
                }

                _ = Consume(); // Consume the operator
                var rhs = ParseExpression(bprecedence);

                // Special case: assignment
                if (op.Kind == EqualToken)
                {
                    // lhs must be an lvalue!
                    if (lhs is not VariableNode variableNode)
                        throw ParserException.MakeNotAnLValue(Current);

                    lhs = new AssignmentNode(variableNode.Name, rhs);
                }
                else lhs = new BinaryNode(lhs, op, rhs);
            }

            return lhs;
        }

        private ExpressionNode ParsePrimaryExpression() => Current.Kind switch
        {
            LeftParenToken => ParseGroupingExpression(),
            TrueToken or FalseToken => ParseBooleanLiteral(),
            IntegerLiteralToken => ParseIntegerLiteral(),
            FloatLiteralToken => ParseFloatLiteral(),
            StringLiteralToken => ParseStringLiteral(),
            IdentifierToken => ParseIdentifier(),
            _ => throw ParserException.MakeUnexpectedToken(Current)
        };

        private GroupingNode ParseGroupingExpression()
        {
            _ = ConsumeIfMatches(LeftParenToken); // Discard the opening paren
            var expression = new GroupingNode(ParseExpression());
            _ = ConsumeIfMatches(RightParenToken); // Discard the closing paren
            return expression;
        }

        private LiteralNode ParseBooleanLiteral() => new(ConsumeIfMatchesAny(TrueToken, FalseToken));
        private LiteralNode ParseIntegerLiteral() => new(ConsumeIfMatches(IntegerLiteralToken));
        private LiteralNode ParseFloatLiteral() => new(ConsumeIfMatches(FloatLiteralToken));
        private LiteralNode ParseStringLiteral() => new(ConsumeIfMatches(StringLiteralToken));
        private VariableNode ParseIdentifier() => new(ConsumeIfMatches(IdentifierToken));

        // Helpers --------------------------------------

        private Token ConsumeIfMatches(SyntaxKind kind) =>
            ConsumeIf(t => t.Kind == kind, $"{kind} token");

        private Token ConsumeIfMatchesAny(params SyntaxKind[] kinds) =>
            ConsumeIf(t => kinds.Contains(t.Kind), $"One of {string.Join(", ", kinds)} tokens");

        private Token ConsumeIf(Func<Token, bool> condition, string expectation)
        {
            var token = Current;
            return condition(token) ? Consume() : throw ParserException.MakeUnexpectedToken(token, expectation);
        }

        private Token Consume()
        {
            if (IsAtEnd()) throw new InvalidOperationException("End of stream was reached");

            var current = Current;
            position++;
            return current;
        }

        private Token Peek(int offset)
        {
            var index = position + offset;
            return index >= source.Length ? source[^1] : source[index];
        }

        private bool IsAtEnd() => Current.Kind == EofToken;

        // Advance the parser up to the next ';' so that we can continue parsing in a 
        // more 'stable' state
        private StatementNode Sync(ParserException cause)
        {
            diagnostics.ReportParserException(cause);

            while (!IsAtEnd())
            {
                var token = Consume();
                if (token.Kind == SemicolonToken)
                    break;
            }

            return new InvalidNode(cause.Token);
        }
    }
}
