using System;
using System.Collections.Generic;
using Delta.Slang.Symbols;

namespace Delta.Slang.Syntax;

public sealed class AssignmentExpressionNode : ExpressionNode
{
    internal AssignmentExpressionNode(Token identifier, Token equal, ExpressionNode expression)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        Equal = equal ?? throw new ArgumentNullException(nameof(equal));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
    public override Token MainToken => Identifier;
    public override IEnumerable<SyntaxNode> Children { get { yield return Expression; } }

    public Token Identifier { get; }
    public Token Equal { get; }
    public ExpressionNode Expression { get; }
}

public sealed class UnaryExpressionNode : ExpressionNode
{
    internal UnaryExpressionNode(Token op, ExpressionNode operand)
    {
        Operator = op ?? throw new ArgumentNullException(nameof(op));
        Operand = operand ?? throw new ArgumentNullException(nameof(operand));
    }

    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
    public override Token MainToken => Operator;
    public override IEnumerable<SyntaxNode> Children { get { yield return Operand; } }

    public Token Operator { get; }
    public ExpressionNode Operand { get; }
}

public sealed class BinaryExpressionNode : ExpressionNode
{
    internal BinaryExpressionNode(ExpressionNode lhs, Token op, ExpressionNode rhs)
    {
        Left = lhs ?? throw new ArgumentNullException(nameof(lhs));
        Operator = op ?? throw new ArgumentNullException(nameof(op));
        Right = rhs ?? throw new ArgumentNullException(nameof(rhs));
    }

    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
    public override Token MainToken => Operator;
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Left;
            yield return Right;
        }
    }

    public ExpressionNode Left { get; }
    public Token Operator { get; }
    public ExpressionNode Right { get; }
}

public sealed class ParenthesizedExpressionNode : ExpressionNode
{
    internal ParenthesizedExpressionNode(Token openParenthesis, ExpressionNode expression)
    {
        MainToken = openParenthesis;
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
    public override Token MainToken { get; }
    public override IEnumerable<SyntaxNode> Children { get { yield return Expression; } }

    public ExpressionNode Expression { get; }
}

public sealed class LiteralExpressionNode : ExpressionNode
{
    private LiteralExpressionNode(Token literal, object value, TypeSymbol type)
    {
        Literal = literal ?? throw new ArgumentNullException(nameof(literal));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public static LiteralExpressionNode MakeBoolLiteral(Token literal, bool value) => new(literal, value, BuiltinTypes.Bool);
    public static LiteralExpressionNode MakeIntLiteral(Token literal) => new(literal, literal.Value, BuiltinTypes.Int);
    public static LiteralExpressionNode MakeDoubleLiteral(Token literal) => new(literal, literal.Value, BuiltinTypes.Double);
    public static LiteralExpressionNode MakeStringLiteral(Token literal) => new(literal, literal.Value, BuiltinTypes.String);

    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    public override Token MainToken => Literal;
    public override IEnumerable<SyntaxNode> Children { get { yield break; } }

    public TypeSymbol Type { get; }
    public Token Literal { get; }
    public object Value { get; }
}

public sealed class InvokeExpressionNode : ExpressionNode
{
    internal InvokeExpressionNode(Token functionName, Token openParenthesis, IEnumerable<ExpressionNode> arguments, Token closeParenthesis)
    {
        FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
        OpenParenthesis = openParenthesis ?? throw new ArgumentNullException(nameof(openParenthesis));
        Arguments = arguments ?? Array.Empty<ExpressionNode>();
        CloseParenthesis = closeParenthesis ?? throw new ArgumentNullException(nameof(closeParenthesis));
    }

    public override SyntaxKind Kind => SyntaxKind.InvokeExpression;
    public override Token MainToken => FunctionName;
    public override IEnumerable<SyntaxNode> Children => Arguments;

    public Token FunctionName { get; }
    public Token OpenParenthesis { get; }
    public Token CloseParenthesis { get; }
    public IEnumerable<ExpressionNode> Arguments { get; }
}

public sealed class NameExpressionNode : ExpressionNode
{
    internal NameExpressionNode(Token identifier) =>
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));

    public override SyntaxKind Kind => SyntaxKind.NameExpression;
    public override Token MainToken => Identifier;
    public override IEnumerable<SyntaxNode> Children { get { yield break; } }

    public Token Identifier { get; }
}