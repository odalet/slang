namespace Delta.Slang.Semantic
{
    public enum BoundTreeNodeKind
    {
        // Structure

        // Statements
        BlockStatement,
        VariableDeclaration,
        //IfStatement,
        //WhileStatement,
        //DoWhileStatement,
        //ForStatement,
        //LabelStatement,
        //GotoStatement,
        //ConditionalGotoStatement,
        ReturnStatement,
        ExpressionStatement,

        // Expressions
        ErrorExpression,
        LiteralExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        BinaryExpression,
        InvokeExpression,
        ConversionExpression,
        InvalidStatement,
        InvalidExpression,
        FunctionDefinition,
    }
}
