using System.Collections.Generic;
using Delta.Slang.Symbols;

namespace Delta.Slang.Syntax;

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

    private ExpressionNode ParsePrimaryExpression() => Current.Kind switch
    {
        TokenKind.OpenParenthesis => ParseParenthesizedExpression(),
        TokenKind.FalseKeyword or TokenKind.TrueKeyword => ParseBoolLiteral(),
        TokenKind.IntLiteral => ParseIntLiteral(),
        TokenKind.DoubleLiteral => ParseDoubleLiteral(),
        TokenKind.DoubleQuote => ParseStringLiteral(),
        _ => ParseNameOrInvokeExpression(),
    };

    private ParenthesizedExpressionNode ParseParenthesizedExpression()
    {
        var openParen = MatchToken(TokenKind.OpenParenthesis);
        var expression = ParseExpression();
        _ = MatchToken(TokenKind.CloseParenthesis);
        return new ParenthesizedExpressionNode(openParen, expression);
    }

    private LiteralExpressionNode ParseBoolLiteral()
    {
        var isTrue = Current.Kind == TokenKind.TrueKeyword;
        var keywordToken = isTrue ? MatchToken(TokenKind.TrueKeyword) : MatchToken(TokenKind.FalseKeyword);
        return LiteralExpressionNode.MakeBoolLiteral(keywordToken, isTrue);
    }

    private LiteralExpressionNode ParseIntLiteral()
    {
        var token = MatchToken(TokenKind.IntLiteral);
        return LiteralExpressionNode.MakeIntLiteral(token);
    }

    private LiteralExpressionNode ParseDoubleLiteral()
    {
        var token = MatchToken(TokenKind.DoubleLiteral);
        return LiteralExpressionNode.MakeDoubleLiteral(token);
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
        var parameters = new List<ExpressionNode>();

        var shouldParseNextParameter = true;
        while (shouldParseNextParameter && Current.Kind != TokenKind.CloseParenthesis && Current.Kind != TokenKind.Eof)
        {
            var expression = ParseExpression();
            parameters.Add(expression);

            if (Current.Kind == TokenKind.Comma)
                _ = MatchToken(TokenKind.Comma);
            else shouldParseNextParameter = false;
        }

        return parameters.ToArray();
    }

    private NameExpressionNode ParseNameExpression()
    {
        var identifier = MatchToken(TokenKind.Identifier);
        return new NameExpressionNode(identifier);
    }
}
