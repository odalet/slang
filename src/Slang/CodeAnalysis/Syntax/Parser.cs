using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slang.CodeAnalysis.Syntax;
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
                return new(new InvalidExpressionNode(new Token(InvalidToken, TokenCategory.Terminal, new TextSpan(0, 1), new LinePosition(0, 0), "")));
            }
        }

        private ParseTree ParseTokens()
        {
            var expression = ParseExpression();
            return new ParseTree(expression);
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
                lhs = new UnaryExpressionNode(op, operand);
            }
            else lhs = ParsePrimaryExpression();

            while (true)
            {
                var bprecedence = Current.GetBinaryOperatorPrecedence();

                // If we use <, the resulting expression tree leans on the left
                // If we use <=, it leans on the right
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

                if (bprecedence < parentPrecedence)
                    break;

                var op = Consume();
                var rhs = ParseExpression(bprecedence);
                lhs = new BinaryExpressionNode(lhs, op, rhs);
            }

            return lhs;
        }

        private ExpressionNode ParsePrimaryExpression() => Current.Kind switch
        {
            TrueToken or FalseToken => ParseBooleanLiteral(),
            IntegerLiteralToken => ParseIntegerLiteral(),
            FloatLiteralToken => ParseFloatLiteral(),
            StringLiteralToken => ParseStringLiteral(),
            _ => ParseInvalidExpression()
        };

        private LiteralExpressionNode ParseBooleanLiteral() => new(ConsumeIfMatchesAny(TrueToken, FalseToken));
        private LiteralExpressionNode ParseIntegerLiteral() => new(ConsumeIfMatches(IntegerLiteralToken));
        private LiteralExpressionNode ParseFloatLiteral() => new(ConsumeIfMatches(FloatLiteralToken));
        private LiteralExpressionNode ParseStringLiteral() => new(ConsumeIfMatches(StringLiteralToken));
        private InvalidExpressionNode ParseInvalidExpression() => MakeInvalidExpression(Consume());

        // Helpers --------------------------------------

        private InvalidExpressionNode MakeInvalidExpression(Token token)
        {
            diagnostics.ReportUnexpectedToken(Current.Position, Current.Span, token);
            return new(token);
        }

        private Token ConsumeIfMatches(SyntaxKind kind) =>
            ConsumeIf(t => t.Kind == kind, $"{kind} token");

        private Token ConsumeIfMatchesAny(params SyntaxKind[] kinds) =>
            ConsumeIf(t => kinds.Contains(t.Kind), $"One of {string.Join(", ", kinds)} tokens");

        private Token ConsumeIf(Func<Token, bool> condition, string expectation)
        {
            var token = Current;
            if (condition(token))
                return Consume();

            diagnostics.ReportUnexpectedToken(Current.Position, Current.Span, token, expectation);
            return Consume(); // consume it anyway...
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
    }
}
