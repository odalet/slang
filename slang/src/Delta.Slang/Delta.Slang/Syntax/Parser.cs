using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Delta.Slang.Symbols;
using Delta.Slang.Text;

namespace Delta.Slang.Syntax
{
    internal sealed partial class Parser
    {
        private readonly DiagnosticCollection diagnostics;
        private readonly Token[] tokens;
        private int position;

        public Parser(IEnumerable<Token> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            diagnostics = new DiagnosticCollection();
            tokens = source
                .Where(t => t.Kind != TokenKind.Whitespace && t.Kind != TokenKind.Invalid && t.Kind != TokenKind.Comment)
                .ToArray();
        }

        public IEnumerable<IDiagnostic> Diagnostics => diagnostics;

        public ParseTree Parse()
        {
            var compilationUnit = ParseCompilationUnit();
            return new ParseTree(compilationUnit);
        }

        private CompilationUnitNode ParseCompilationUnit()
        {
            var members = ParseCompilationUnitContent();
            _ = MatchToken(TokenKind.Eof);
            return new CompilationUnitNode(members);
        }

        private IEnumerable<MemberNode> ParseCompilationUnitContent()
        {
            var members = ImmutableArray.CreateBuilder<MemberNode>();
            while (Current.Kind != TokenKind.Eof)
            {
                var startToken = Current;

                var member = ParseMember();
                members.Add(member);

                // If ParseMember() did not consume any tokens,
                // we need to skip the current token and continue
                // in order to avoid an infinite loop.
                //
                // We don't need to report an error, because we've
                // already tried to parse an expression statement
                // and reported one.
                if (Current == startToken)
                    Advance();
            }

            return members.ToImmutable();
        }

        private MemberNode ParseMember() => Current.Kind == TokenKind.FunKeyword ? (MemberNode)ParseFunctionDeclaration() : ParseGlobalStatement();

        private FunctionDeclarationNode ParseFunctionDeclaration()
        {
            _ = MatchToken(TokenKind.FunKeyword);
            var identifier = MatchToken(TokenKind.Identifier);
            _ = MatchToken(TokenKind.OpenParenthesis);
            var parameters = ParseParametersDeclaration();
            _ = MatchToken(TokenKind.CloseParenthesis);
            var returnType = ParseOptionalTypeClause();
            var body = ParseBlock();
            return new FunctionDeclarationNode(identifier, parameters, returnType, body);
        }

        private GlobalStatementNode ParseGlobalStatement()
        {
            var statement = ParseStatement();
            return new GlobalStatementNode(statement);
        }

        private ParametersDeclarationNode ParseParametersDeclaration()
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterDeclarationNode>();

            var shouldParseNextParameter = true;
            while (shouldParseNextParameter && Current.Kind != TokenKind.CloseParenthesis && Current.Kind != TokenKind.Eof)
            {
                var parameter = ParseParameterDeclaration();
                parameters.Add(parameter);

                if (Current.Kind == TokenKind.Comma)
                    _ = MatchToken(TokenKind.Comma);
                else shouldParseNextParameter = false;
            }

            return new ParametersDeclarationNode(parameters.ToImmutable());
        }

        private ParameterDeclarationNode ParseParameterDeclaration()
        {
            var identifier = MatchToken(TokenKind.Identifier);
            var type = ParseTypeClause();
            return new ParameterDeclarationNode(identifier, type);
        }

        private TypeClauseNode ParseTypeClause()
        {
            _ = MatchToken(TokenKind.Colon);
            var identifier = MatchToken(TokenKind.Identifier);
            return new TypeClauseNode(identifier);
        }

        private TypeClauseNode ParseOptionalTypeClause()
        {
            if (Current.Kind == TokenKind.Colon)
                return ParseTypeClause();

            // If no type declared: this is void, and void is a type ;)
            var voidToken = new Token(
                TokenKind.Identifier,
                new TextSpan(Current.Span.Start, 0),
                Current.Position,
                BuiltinTypes.Void.Name);
            return new TypeClauseNode(voidToken);
        }

        private BlockNode ParseBlock()
        {
            var statements = ImmutableArray.CreateBuilder<StatementNode>();

            _ = MatchToken(TokenKind.OpenBrace);
            while (Current.Kind != TokenKind.CloseBrace && Current.Kind != TokenKind.Eof)
            {
                var startToken = Current;

                var statement = ParseStatement();
                statements.Add(statement);

                // If we did not consume any tokens, we need to skip the current token and continue
                // in order to avoid an infinite loop. We don't need to report an error, because we'll
                // already tried to parse an expression statement and reported one.
                if (Current == startToken)
                    Advance();
            }

            _ = MatchToken(TokenKind.CloseBrace);
            return new BlockNode(statements.ToImmutable());
        }

        private Token MatchToken(TokenKind expected)
        {
            var actual = Current.Kind;
            if (actual == expected) return Next();

            diagnostics.ReportUnexpectedToken(Current.Position, Current.Span, expected, actual);
            return new Token(expected, Current.Span, Current.Position, "", null, true);
        }

        private Token Current => Peek(0);

        private Token Next()
        {
            var current = Peek(0);
            Advance();
            return current;
        }

        private void Advance() => position++;

        private Token Peek(int offset)
        {
            var index = position + offset;
            return index >= tokens.Length ?
                tokens[tokens.Length - 1] :
                tokens[index];
        }
    }
}
