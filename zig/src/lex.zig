const std = @import("std");

// Helpers
fn isIdentifierFirstCharacter(c: u8) bool {
    return c == '_' or isLetter(c);
}

fn isIdentifierCharacter(c: u8) bool {
    return isIdentifierFirstCharacter(c) or isDigit(c);
}

fn isLetter(c: u8) bool {
    return (c >= 'a' and c <= 'z') or (c >= 'A' and c <= 'Z');
}

fn isDigit(c: u8) bool {
    return c >= '0' and c <= '9';
}

fn isWhitespaceOrLineBreak(c: u8) bool {
    return switch (c) {
        ' ', '\t', '\r', '\n' => true,
        else => false,
    };
}

pub const Token = struct {
    tag: Tag,
    loc: Loc,
    is_valid: bool,

    pub const Loc = struct {
        start: usize,
        end: usize,
    };

    pub const Tag = enum {
        invalid,
        eof,
        plus, // +
        minus, // -
        star, // *
        slash, // /
        left_paren, // (
        right_paren, // )
        left_brace, // {
        right_brace, // }
        period, // .
        comma, // ,
        colon, // :
        semicolon, // ;
        comment, // // ..., or /* ... */
        ampersand_ampersand, // &&
        pipe_pipe, // ||
        equal, // =
        equal_equal, // ==
        lower, // <
        lower_equal, // <=
        greater, // >
        greater_equal, // >=
        bang, // !
        bang_equal, // !=
        int_literal,
        float_literal,
        identifier,
    };
};

pub const Lexer = struct {
    buffer: [:0]const u8,
    //index: usize,
    previous: usize,
    current: usize,

    pending_invalid_token: ?Token,

    pub fn init(buffer: [:0]const u8) Lexer {
        // Original code retains the real start of the slice
        //
        // // Skip the UTF-8 BOM if present
        // const src_start: usize = if (std.mem.startsWith(u8, buffer, "\xEF\xBB\xBF")) 3 else 0;
        // return Lexer{
        //     .buffer = buffer,
        //     .index = src_start,
        //     .pending_invalid_token = null,
        // };

        // Here, we skip it entirely and redefine the slice
        // See https://en.wikipedia.org/wiki/Byte_order_mark#UTF-8
        const has_bom = std.mem.startsWith(u8, buffer, "\xEF\xBB\xBF");
        const buf = buffer[if (has_bom) 3 else 0.. :0];
        return Lexer{
            .buffer = buf,
            // .index = 0,
            .previous = 0,
            .current = 0,
            .pending_invalid_token = null,
        };
    }

    const State = enum {
        start,
    };

    pub fn next(self: *Lexer) Token {
        var tok = Token{
            .tag = .eof,
            .is_valid = true,
            .loc = .{
                // .start = self.index,
                .start = self.current,
                .end = undefined,
            },
        };

        self.startNextLexeme();

        const c = self.lookAhead();
        switch (c) {
            0 => self.setAndConsume(&tok, .eof),
            '+' => self.setAndConsume(&tok, .plus),
            '-' => self.setAndConsume(&tok, .minus),
            '*' => self.lexPotentialEndOfComment(&tok),
            '/' => self.lexPotentialComment(&tok),
            '(' => self.setAndConsume(&tok, .left_paren),
            ')' => self.setAndConsume(&tok, .right_paren),
            '{' => self.setAndConsume(&tok, .left_brace),
            '}' => self.setAndConsume(&tok, .right_brace),
            '.' => self.setAndConsume(&tok, .period),
            ',' => self.setAndConsume(&tok, .comma),
            ':' => self.setAndConsume(&tok, .colon),
            ';' => self.setAndConsume(&tok, .semicolon),
            '&', '|' => self.lexLogicalOperator(c, &tok),
            '<', '>', '=', '!' => self.lexOperatorEndingWithOptionalEqual(c, &tok),
            else => {
                if (isDigit(c)) {
                    self.lexNumberLiteral(&tok);
                } else if (isIdentifierFirstCharacter(c)) {
                    self.lexIdentifierOrReservedWord(&tok);
                } else {
                    self.setAndConsume(&tok, .invalid);
                }
            },
        }

        return tok;
    }

    fn lexIdentifierOrReservedWord(self: *Lexer, tok: *Token) void {
        while (isIdentifierCharacter(self.lookAhead()))
            self.consume();
        self.set(tok, .identifier);
    }

    fn lexNumberLiteral(self: *Lexer, tok: *Token) void {
        while (isDigit(self.lookAhead()))
            self.consume();

        // A digit after a period means we are looking at a decimal separator
        if (self.lookAhead() == '.' and isDigit(self.lookAheadN(1))) {
            self.consume(); // Consume the period

            // Decimal part
            while (isDigit(self.lookAhead()))
                self.consume();

            // Make a floating point literal
            self.set(tok, .float_literal);
            return;
        }

        // Otherwise, don't consume the dot (it will be consumed by the
        // general lexing loop) and build an integer
        self.set(tok, .int_literal);
    }

    fn lexOperatorEndingWithOptionalEqual(self: *Lexer, first_char: u8, tok: *Token) void {
        self.consume();
        var has_additional_equal = false;
        if (self.lookAhead() == '=') {
            self.consume();
            has_additional_equal = true;
        }

        const tag: Token.Tag = switch (first_char) {
            '<' => if (has_additional_equal) .lower_equal else .lower,
            '>' => if (has_additional_equal) .greater_equal else .greater,
            '=' => if (has_additional_equal) .equal_equal else .equal,
            '!' => if (has_additional_equal) .bang_equal else .bang,
            else => unreachable,
        };

        self.set(tok, tag);
    }

    fn lexLogicalOperator(self: *Lexer, first_char: u8, tok: *Token) void {
        self.consume();
        var is_duplicated = false;
        if (self.lookAhead() == first_char) {
            self.consume();
            is_duplicated = true;
        }

        const tag: Token.Tag = switch (first_char) {
            '&' => .ampersand_ampersand,
            '|' => .pipe_pipe,
            else => unreachable,
        };

        self.set(tok, tag);
        tok.is_valid = is_duplicated; // Later on, non duplicated operators will represent bit manipulation operators
    }

    fn lexPotentialEndOfComment(self: *Lexer, tok: *Token) void {
        self.consume(); // Consume the initial *
        var c = self.lookAhead();
        if (c == '/') {
            // Unexpected end of C comment!
            self.setAndConsume(tok, .comment);
            tok.is_valid = false;
            return;
        }

        // Otherwise, star token
        self.set(tok, .star);
    }

    fn lexPotentialComment(self: *Lexer, tok: *Token) void {
        self.consume(); // Consume the initial /
        const c = self.lookAhead();
        if (c == '/') {
            self.lexCppComment(tok);
            return;
        }

        if (c == '*') {
            self.lexCComment(tok);
            return;
        }

        // Otherwise, slash token
        self.set(tok, .slash);
    }

    fn lexCppComment(self: *Lexer, tok: *Token) void {
        self.consume(); // Consume the second /
        while (true) {
            const c = self.lookAhead();
            switch (c) {
                '\r', '\n', 0 => break,
                else => self.consume(),
            }
        }

        self.set(tok, .comment);
    }

    fn lexCComment(self: *Lexer, tok: *Token) void {
        self.consume(); // Consume the * after the initial /
        while (true) {
            const c = self.lookAhead();
            if (c == 0) {
                // Unterminated comment!
                tok.is_valid = false;
                break;
            }

            if (self.consumeLineBreakIfAny(c))
                continue;

            // No support (yet) for nested comments: we stop at the first */
            if (c == '*') {
                self.consume();
                const c2 = self.lookAhead();
                if (c2 == '/') {
                    self.consume();
                    break;
                } else if (c2 == 0) {
                    // Unterminated comment!
                    tok.is_valid = false;
                    break;
                }
            }

            // All other cases, keep looping
            self.consume();
        }

        self.set(tok, .comment);
    }

    fn consumeLineBreakIfAny(self: *Lexer, c: u8) bool {
        if (c == '\r') {
            self.consume();
            var c2 = self.lookAhead();
            if (c2 == '\n')
                self.consume();
            return true;
        }

        if (c == '\n') {
            self.consume();
            return true;
        }

        return false;
    }

    fn startNextLexeme(self: *Lexer) void {
        self.previous = self.current;
    }

    fn set(self: *Lexer, tok: *Token, tag: Token.Tag) void {
        tok.tag = tag;
        tok.loc.end = self.current;
    }

    fn setAndConsume(self: *Lexer, tok: *Token, tag: Token.Tag) void {
        tok.tag = tag;
        tok.loc.end = self.current;
        self.consume();
    }

    fn consume(self: *Lexer) void {
        self.current += 1;
    }

    fn lookAhead(self: *Lexer) u8 {
        return self.buffer[self.current];
    }

    fn lookAheadN(self: *Lexer, n: usize) u8 {
        return self.buffer[self.current + n];
    }
};
