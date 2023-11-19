using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

namespace Slang.Syntax;

using static ParseErrorOrAstNodeIndex;

public ref struct Parser
{
    private AstBuilder astBuilder;

    public Parser(ReadOnlySpan<SyntaxToken> source)
    {
        var state = new ParserState(source);
        astBuilder = new(state);
    }

    public Ast Parse() => astBuilder.ParseRoot();
}

internal ref struct ParserState
{
    private readonly ReadOnlySpan<SyntaxToken> tokens;
    private readonly int length;
    private readonly List<AstNode> nodes = new();

    public ParserState(ReadOnlySpan<SyntaxToken> source)
    {
        tokens = source;
        length = tokens.Length;

        // Make root node
        var rootData = new AstNodeData(null, null);
        nodes.Add(new(AstNodeKind.Root, 0, -1, new(null, null)));
    }

    public int TokenIndex;

    public SyntaxToken Token => tokens[TokenIndex];
    public SyntaxKind Kind => Token.Kind;

    public Ast ToAst() => new(nodes[0], nodes.ToArray().AsSpan());
    
    public void SetRootNodeData(AstNodeData data) => nodes[0] = nodes[0] with { Data = data };
    
    public int AddNode(AstNodeKind kind, int? lhs, int? rhs)
    {
        var mainTokenIndex = Next();
        var data = new AstNodeData(lhs, rhs);
        var index = nodes.Count;
        var node = new AstNode(kind, index, mainTokenIndex, data);
        nodes.Add(node);
        return index;
    }
    
    private int Next()
    {
        var result = TokenIndex;
        TokenIndex++;
        return result;
    }
}

public ref struct Ast
{
    private readonly ReadOnlySpan<AstNode> nodes;

    public Ast(AstNode root, ReadOnlySpan<AstNode> astNodes)
    {
        Root = root;
        nodes = astNodes;
    }
    
    public AstNode Root { get; }
}

public readonly record struct AstNode(AstNodeKind Kind, int Index, int MainTokenIndex, AstNodeData Data)
{
    public const int InvalidIndex = -1;
    public static bool IsValidIndex(int index) => index >= 0;
}

public enum AstNodeKind
{
    Root,
    BooleanNot,
    UnaryAdd,
    UnarySub,
    Add,
    Sub,
    Mul,
    Div
}

public readonly record struct ParseError(ParseErrorKind Kind, int TokenIndex)
{
    public static ParseError None { get; } = new(ParseErrorKind.None, -1);
}

public enum ParseErrorKind
{
    None,
    ExpectedPrefixExpr
}

public readonly record struct AstNodeData(int? Lhs, int? Rhs);

public readonly struct ParseErrorOrAstNodeIndex
{
    private ParseErrorOrAstNodeIndex(bool isError, ParseError error, int index)
    {
        IsOk = !isError;
        Error = error;
        Index = index;
    }

    public bool IsOk { get; }
    public bool IsKo => !IsOk;
    public ParseError Error { get; }
    public int Index { get; }

    public static ParseErrorOrAstNodeIndex Ok(int index) => new(false, ParseError.None, index);
    public static ParseErrorOrAstNodeIndex Ko(ParseError error) => new(true, error, AstNode.InvalidIndex);
}

internal ref struct AstBuilder
{
    private ParserState state;

    public AstBuilder(ParserState parserState) => state = parserState;

    // Root <- skip container_doc_comment? ContainerMembers eof
    public Ast ParseRoot()
    {
        var l = 1;
        var r = 42;
        state.SetRootNodeData(new(l, r));
        return state.ToAst();
    }

    public int ParseExpr() => ParseExprPrecedence(0);

    public int ParseExprPrecedence(int precedence)
    {
        return 0;
    }


    // PrefixExpr <- PrefixOp* PrimaryExpr
    // PrefixOp <- Bang | Plus | Minus
    private ParseErrorOrAstNodeIndex ParsePrefixExpr()
    {
        var nodeKind = state.Kind switch
        {
            SyntaxKind.BangToken => AstNodeKind.BooleanNot,
            SyntaxKind.PlusToken => AstNodeKind.UnaryAdd,
            SyntaxKind.MinusToken => AstNodeKind.UnarySub,
            _ => (AstNodeKind?)null
        };

        if (!nodeKind.HasValue) return ParsePrimaryExpr();

        var lhs = ExpectPrefixExpr();
        return lhs.IsOk
            ? Ok(state.AddNode(nodeKind.Value, lhs.Index, null))
            : lhs;
    }

    private ParseErrorOrAstNodeIndex ExpectPrefixExpr()
    {
        var result = ParsePrefixExpr();
        return result.IsOk ? result : Ko(Fail(ParseErrorKind.ExpectedPrefixExpr));
    }

    // PrimaryExpr <- IfExpr | Block
    private ParseErrorOrAstNodeIndex ParsePrimaryExpr()
    {
        // if (state.Kind == SyntaxKind.IfToken)
        //     return ParseIfExpr();
        //
        // var nodeKind = state.Kind switch
        // {
        //     SyntaxKind.IfToken => AstNodeKind.,
        //     SyntaxKind.PlusToken => AstNodeKind.UnaryAdd,
        //     SyntaxKind.MinusToken => AstNodeKind.UnarySub,
        //     _ => (AstNodeKind?)null
        // };

        return Ok(0);
    }

    private ParseError Fail(ParseErrorKind kind) => new(kind, state.TokenIndex);
}