using System.Collections.Generic;
using System.Collections.Immutable;
using Delta.Slang.Utils;

namespace Delta.Slang.Syntax
{
    partial class Parser
    {
        private ExpressionNode ParseExpression() =>
            Peek(0).Kind == TokenKind.Identifier && Peek(1).Kind == TokenKind.Equal ?
            ParseAssignmentExpression() :
            ParseUnaryOrBinaryOrPrimaryExpression();

        private AssignmentExpressionNode ParseAssignmentExpression()
        {
            var identifier = MatchToken(TokenKind.Identifier);
            var equal = MatchToken(TokenKind.Equal);
            var rhs = ParseExpression();

            return new AssignmentExpressionNode(identifier, equal, rhs);
        }

        private ExpressionNode ParseUnaryOrBinaryOrPrimaryExpression(int parentPrecedence = 0)
        {
            ExpressionNode lhs;
            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var op = Next();
                var operand = ParseUnaryOrBinaryOrPrimaryExpression(unaryOperatorPrecedence);
                lhs = new UnaryExpressionNode(op, operand);
            }
            else lhs = ParsePrimaryExpression();

            while (true)
            {
                var precedence = Current.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;

                var op = Next();
                var rhs = ParseUnaryOrBinaryOrPrimaryExpression(precedence);
                lhs = new BinaryExpressionNode(lhs, op, rhs);
            }

            return lhs;
        }

        private ExpressionNode ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case TokenKind.OpenParenthesis: return ParseParenthesizedExpression();
                case TokenKind.FalseKeyword:
                case TokenKind.TrueKeyword: return ParseBooleanLiteral();
                case TokenKind.NumberLiteral: return ParseNumberLiteral();
                case TokenKind.DoubleQuote: return ParseStringLiteral();
                case TokenKind.Identifier:
                default:
                    return ParseNameOrInvokeExpression();
            }
        }

        private ParenthesizedExpressionNode ParseParenthesizedExpression()
        {
            var openParen = MatchToken(TokenKind.OpenParenthesis);
            var expression = ParseExpression();
            _ = MatchToken(TokenKind.CloseParenthesis);
            return new ParenthesizedExpressionNode(openParen, expression);
        }

        private LiteralExpressionNode ParseBooleanLiteral()
        {
            var isTrue = Current.Kind == TokenKind.TrueKeyword;
            var keywordToken = isTrue ? MatchToken(TokenKind.TrueKeyword) : MatchToken(TokenKind.FalseKeyword);
            return LiteralExpressionNode.MakeBooleanLiteral(keywordToken, isTrue);
        }

        private LiteralExpressionNode ParseNumberLiteral()
        {
            var token = MatchToken(TokenKind.NumberLiteral);
            return LiteralExpressionNode.MakeIntegerLiteral(token);
        }

        private LiteralExpressionNode ParseStringLiteral()
        {
            _ = MatchToken(TokenKind.DoubleQuote);
            var token = MatchToken(TokenKind.StringLiteral);
            _ = MatchToken(TokenKind.DoubleQuote);
            return LiteralExpressionNode.MakeStringLiteral(token);
        }

        private ExpressionNode ParseNameOrInvokeExpression() =>
            Peek(0).Kind == TokenKind.Identifier && Peek(1).Kind == TokenKind.OpenParenthesis ?
            ParseInvokeExpression() :
            ParseNameExpression();

        private ExpressionNode ParseInvokeExpression()
        {
            var identifier = MatchToken(TokenKind.Identifier);
            var openParen = MatchToken(TokenKind.OpenParenthesis);
            var arguments = ParseArguments();
            var closeParen = MatchToken(TokenKind.CloseParenthesis);

            return new InvokeExpressionNode(identifier, openParen, arguments, closeParen);
        }

        private IEnumerable<ExpressionNode> ParseArguments()
        {
            var parameters = ImmutableArray.CreateBuilder<ExpressionNode>();

            var shouldParseNextParameter = true;
            while (shouldParseNextParameter && Current.Kind != TokenKind.CloseParenthesis && Current.Kind != TokenKind.Eof)
            {
                var expression = ParseExpression();
                parameters.Add(expression);

                if (Current.Kind == TokenKind.Comma)
                    _ = MatchToken(TokenKind.Comma);
                else shouldParseNextParameter = false;
            }

            return parameters.ToImmutable();
        }

        private NameExpressionNode ParseNameExpression()
        {
            var identifier = MatchToken(TokenKind.Identifier);
            return new NameExpressionNode(identifier);
        }
    }
}
