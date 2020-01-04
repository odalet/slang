namespace Delta.Slang.Syntax
{
    partial class Parser
    {
        private StatementNode ParseStatement()
        {
            switch (Current.Kind)
            {
                case TokenKind.OpenBrace: return ParseBlock();
                case TokenKind.VarKeyword: return ParseVariableDeclaration();
                //case TokenKind.IfKeyword: return ParseIfStatement();
                //case TokenKind.WhileKeyword: return ParseWhileStatement();
                //case TokenKind.DoKeyword: return ParseDoWhileStatement();
                //case TokenKind.ForKeyword: return ParseForStatement();
                //case TokenKind.BreakKeyword: return ParseBreakStatement();
                //case TokenKind.ContinueKeyword: return ParseContinueStatement();
                case TokenKind.ReturnKeyword: return ParseReturnStatement();
                default: return ParseExpressionStatement();
            }
        }
        
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

        private ReturnStatementNode ParseReturnStatement()
        {
            _ = MatchToken(TokenKind.ReturnKeyword);
            var expression = Current.Kind == TokenKind.Semicolon ? null : ParseExpression();
            _ = MatchToken(TokenKind.Semicolon);

            return new ReturnStatementNode(expression);
        }

        private ExpressionStatementNode ParseExpressionStatement()
        {
            var expression = ParseExpression();
            _ = MatchToken(TokenKind.Semicolon);
            return new ExpressionStatementNode(expression);
        }
    }
}
