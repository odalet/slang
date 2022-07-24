# Design Notes

## Inspiration

* [Crafting Interpreters](https://craftinginterpreters.com/) by *Robert Nystrom*
* *[Immo Landwerth](https://github.com/terrajobst) / [@terrajobst](https://twitter.com/terrajobst)*'s [Minsk Language](https://github.com/terrajobst/minsk) (and [here](http://minsk-compiler.net/) is the series of live-coding videos)

### Lox

* Implementations:
  * Official: <https://github.com/munificent/craftinginterpreters>
* Syntax highlighting:
  * <https://marketplace.visualstudio.com/items?itemName=dberezin.lox-language>

## Grammar

[Here](slang.ebnf)

### Literals

```ebnf
LITERAL = NUMBER | STRING | BOOLEAN | NULL;
NULL = "null";
BOOLEAN = "true" | "false";

STRING = '"', STRING_CONTENT, '"';
STRING_CONTENT = { (CHARACTER - '"' | '\"') };

(* NB: We allow numbers to start with an unlimited number of 0; eg. 1 == 01 == 001... *)
NUMBER = INTEGER | FLOAT;
FLOAT = INTEGER, ".", DIGIT, { DIGIT };
INTEGER = DIGIT, { DIGIT };
DIGIT =  "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9";

CHARACTER = ? all valid unicode characters ?;
```

### Expression Grammar

In *Crafting interpreters*, the expression grammar is disambiguated and crafted so as to embed precedence:

```ebnf
expression = equality;
equality = comparison, [ ("==" | "!="), comparison ];
comparison = term, [ ("<" | "<=" | ">" | ">="), term ];
term = factor, [ ("+" | "-"), factor ];
factor = unary, [ ("*" | "/"), unary ];
unary = ("!" | "+" | "-"), unary | primary;
primary = grouping | LITERAL;
grouping = "(", expression, ")";
```

However, in **slang**, we go with the *precedence-metadata* principle and the actual grammar looks more like this:

```ebnf
expression = primary | unary | binary;
unary = ("!" | "+" | "-"), primary;
binary = primary, ("==" | "!=" | "<" | "<=" | ">" | ">=" | "+" | "-" | "*" | "/"), primary;
primary = grouping | LITERAL;
grouping = "(", expression, ")";
```

## Operator Precedences

Ordered by ascending precedence: operators with *higher* precedence bind tighter.

| Name           | Operators | Associativity | Precedence |
| -------------- | --------- | ------------- | ---------- |
| Equality       | == !=     | Left          | 10         |
| Comparison     | < > <= >= | Left          | 20         |
| Addition       | + -       | Left          | 30         |
| Multiplication | * /       | Left          | 40         |
| Unary          | + - !     | **Right**     | 50         |

For comparison: [C Operators](https://en.cppreference.com/w/c/language/operator_precedence)

See also [Evaluation order in C](https://en.cppreference.com/w/c/language/eval_order) (mostly unspecified!)