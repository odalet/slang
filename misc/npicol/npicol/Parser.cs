using System;

namespace NPicol
{
    internal sealed class Parser
    {
        public Parser(string text)
        {
            Text = text;
            P = 0;
            Len = text.Length;
            Start = 0;
            End = 0;
            InsideQuote = false;
            Type = TokenType.Eol;
        }

        public string Text { get; }
        public int P { get; private set; } // Current position
        public int Len { get; private set; } // Remaining Length
        public int Start { get; private set; } // Token Start
        public int End { get; private set; } // Token End
        public TokenType Type { get; private set; }
        public bool InsideQuote { get; private set; }

        public string Token => End + 1 - Start < 0 ? "" : Text[Start..(End + 1)];

        public Status ConsumeNextToken()
        {
            while (true)
            {
                if (Len == 0)
                {
                    Type = Type is not TokenType.Eol and not TokenType.Eof
                        ? TokenType.Eol
                        : TokenType.Eof;

                    return Status.OK;
                }

                switch (Text[P])
                {
                    case ' ' or '\t' or '\r':
                        return InsideQuote ? ParseString() : ParseSeparator();
                    case ';' or '\n':
                        return InsideQuote ? ParseString() : ParseEol();
                    case '[':
                        return ParseCommand();
                    case '$':
                        return ParseVariable();
                    case '#':
                        if (Type == TokenType.Eol)
                        {
                            _ = ParseComment();
                            continue;
                        }
                        else return ParseString();
                    case '\0':
                        Type = TokenType.Eof;
                        return Status.OK;
                    default:
                        return ParseString();
                }
            }
        }

        private Status ParseString()
        {
            var newWord = Type is TokenType.Sep or TokenType.Eol or TokenType.Str;
            if (newWord && Text[P] == '{') return ParseBrace();
            if (newWord && Text[P] == '"')
            {
                InsideQuote = true;
                Advance();
            }

            Start = P;
            while (true)
            {
                if (Len == 0)
                {
                    End = P - 1;
                    Type = TokenType.Esc;
                    return Status.OK;
                }

                switch (Text[P])
                {
                    case '\\':
                        if (Len >= 2)
                            Advance();
                        break;
                    case '$' or '[':
                        End = P - 1;
                        Type = TokenType.Esc;
                        return Status.OK;
                    case ' ' or ';' or '\t' or '\r' or '\n':
                        if (!InsideQuote)
                        {
                            End = P - 1;
                            Type = TokenType.Esc;
                            return Status.OK;
                        }

                        break;
                    case '"':
                        if (InsideQuote)
                        {
                            End = P - 1;
                            Type = TokenType.Esc;
                            Advance();
                            InsideQuote = false;
                            return Status.OK;
                        }

                        break;
                }

                Advance();
            }
        }

        private Status ParseBrace()
        {
            var level = 1;
            Advance();
            Start = P;
            while (true)
            {
                if (Len >= 2 && Text[P] == '\\')
                    Advance();
                else if (Len == 0 || Text[P] == '}')
                {
                    level--;
                    if (Len == 0 || level == 0)
                    {
                        End = P - 1;
                        if (Len != 0)
                            Advance(); // skip final }
                        Type = TokenType.Str;
                        return Status.OK;
                    }
                }
                else if (Text[P] == '{')
                    level++;
                Advance();
            }
        }

        private Status ParseVariable()
        {
            Advance(); // Skip the $
            Start = P;
            while (true)
            {
                if (Text[P] is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_')
                {
                    Advance();
                    continue;
                }

                break;
            }

            if (Start == P) // This is the single-character string "$"
            {
                Start = End = P - 1;
                Type = TokenType.Str;
            }
            else
            {
                End = P - 1;
                Type = TokenType.Var;
            }

            return Status.OK;
        }

        private Status ParseCommand()
        {
            var level = 1;
            var blevel = 0;

            Advance(); // Skip the [
            Start = P;
            while (true)
            {
                if (Len == 0) break;
                else if (Text[P] == '[' && blevel == 0)
                    level++;
                else if (Text[P] == ']' && blevel == 0)
                {
                    level--;
                    if (level == 0) break;
                }
                else if (Text[P] == '\\')
                    Advance();
                else if (Text[P] == '{')
                    blevel++;
                else if (Text[P] == '}')
                {
                    if (blevel != 0) blevel--;
                }

                Advance();
            }

            End = P - 1;
            Type = TokenType.Cmd;
            if (Text[P] == ']')
                Advance();

            return Status.OK;
        }

        private Status ParseSeparator()
        {
            Start = P;
            while (Text[P] is ' ' or '\t' or '\r' /*or '\n'*/)
                Advance();

            End = P - 1;
            Type = TokenType.Sep;
            return Status.OK;
        }

        private Status ParseEol()
        {
            Start = P;
            while (Text[P] is ' ' or '\t' or '\r' or '\n' or ';')
                Advance();

            End = P - 1;
            Type = TokenType.Eol;
            return Status.OK;
        }

        private Status ParseComment()
        {
            while (Len > 0 && Text[P] != '\n')
                Advance();
            return Status.OK;
        }

        private void Advance() { P++; Len--; }

        public void Dump()
        {
            static string f(string token) => token
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t")
                ;

            Console.WriteLine($"{Type}: '{f(Token)}' - P={P}, L={Len}");
        }
    }
}
