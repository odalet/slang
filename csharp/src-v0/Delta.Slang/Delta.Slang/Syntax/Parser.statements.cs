namespace Delta.Slang.Syntax;

partial class Parser
{
    private StatementNode ParseStatement() => Current.Kind switch
    {
        TokenKind.OpenBrace => ParseBlock(),
        TokenKind.VarKeyword => ParseVariableDeclaration(),
        TokenKind.IfKeyword => ParseIfStatement(),
        TokenKind.GotoKeyword => ParseGotoStatement(),
        //case TokenKind.WhileKeyword: return ParseWhileStatement();
        //case TokenKind.DoKeyword: return ParseDoWhileStatement();
        //case TokenKind.ForKeyword: return ParseForStatement();
        //case TokenKind.BreakKeyword: return ParseBreakStatement();
        //case TokenKind.ContinueKeyword: return ParseContinueStatement();
        TokenKind.ReturnKeyword => ParseReturnStatement(),
        TokenKind.Identifier => ParseIdentifierStatement(),
        _ => ParseExpressionStatement(),
    };

    private VariableDeclarationNode ParseVariableDeclaration()
    {
        _ = MatchToken(TokenKind.VarKeyword);
        var identifier = MatchToken(TokenKind.Identifier);

        var type = Current.Kind == TokenKind.Colon ? ParseTypeClause() : null;

        ExpressionNode initializer = null;

        if (Current.Kind == TokenKind.Equal)
        {
            Advance();
            initializer = ParseExpression();
        }

        _ = MatchToken(TokenKind.Semicolon);

        if (type == null && initializer == null) diagnostics.ReportInvalidVariableDeclaration(
            identifier.Position, identifier.Span, identifier,
            "Either the variable type or its initializer can be missing, but not both");

        return new VariableDeclarationNode(identifier, type, initializer);
    }

    private IfStatementNode ParseIfStatement()
    {
        _ = MatchToken(TokenKind.IfKeyword);
        _ = MatchToken(TokenKind.OpenParenthesis);
        var condition = ParseExpression();
        _ = MatchToken(TokenKind.CloseParenthesis);
        var statement = ParseStatement();
        var elseClause = ParseElseClause();
        return new IfStatementNode(condition, statement, elseClause);
    }

    private GotoStatementNode ParseGotoStatement()
    {
        var gotoKeyword = MatchToken(TokenKind.GotoKeyword);
        var label = ParseNameExpression();
        _ = MatchToken(TokenKind.Semicolon);
        return new GotoStatementNode(gotoKeyword, label);
    }

    private StatementNode ParseIdentifierStatement() => 
        Peek(1).Kind == TokenKind.Colon ? 
        ParseLabelStatement() : 
        (StatementNode)ParseExpressionStatement();

    private LabelStatementNode ParseLabelStatement()
    {
        var label = ParseNameExpression();
        _ = MatchToken(TokenKind.Colon);
        return new LabelStatementNode(label);
    }

    private ElseClauseNode ParseElseClause()
    {
        if (Current.Kind != TokenKind.ElseKeyword)
            return null;

        _ = MatchToken(TokenKind.ElseKeyword);
        var statement = ParseStatement();
        return new ElseClauseNode(statement);
    }

    private ReturnStatementNode ParseReturnStatement()
    {
        var returnToken = MatchToken(TokenKind.ReturnKeyword);
        var expression = Current.Kind == TokenKind.Semicolon ? null : ParseExpression();
        _ = MatchToken(TokenKind.Semicolon);

        return new ReturnStatementNode(returnToken, expression);
    }
    
    private ExpressionStatementNode ParseExpressionStatement()
    {
        var expression = ParseExpression();
        _ = MatchToken(TokenKind.Semicolon);
        return new ExpressionStatementNode(expression);
    }
}
