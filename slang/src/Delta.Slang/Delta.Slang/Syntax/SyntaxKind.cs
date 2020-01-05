namespace Delta.Slang.Syntax
{
    public enum SyntaxKind
    {
        Invalid,
        CompilationUnit,

        FunctionDeclaration,
        ParametersDeclaration,
        ParameterDeclaration,
        TypeClause,

        Block,
        GlobalStatement,
        IfStatement,
        ElseClause,
        ReturnStatement,
        ExpressionStatement,

        AssignmentExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        LiteralExpression,
        InvokeExpression,
        NameExpression,
        GotoStatement,
        LabelStatement,
        VariableDeclaration
    }
}
