namespace Delta.Slang.Semantics;

public enum BoundTreeNodeKind
{
    Root,
    InvalidStatement,
    InvalidExpression,

    // Statements
    BlockStatement,
    VariableDeclaration,
    //IfStatement,
    //WhileStatement,
    //DoWhileStatement,
    //ForStatement,
    //LabelStatement,
    //ConditionalGotoStatement,
    ReturnStatement,
    ExpressionStatement,
    IfStatement,
    GotoStatement,
    LabelStatement,

    // Expressions
    ErrorExpression,
    LiteralExpression,
    VariableExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    InvokeExpression,
    ConversionExpression,
    FunctionDefinition       
}
