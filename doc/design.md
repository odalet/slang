# Design Notes

## Inspiration

* [Crafting Interpreters](https://craftinginterpreters.com/) by *Robert Nystrom*
* *[Immo Landwerth](https://github.com/terrajobst) / [@terrajobst](https://twitter.com/terrajobst)*'s [Minsk Language](https://github.com/terrajobst/minsk) (and [here](http://minsk-compiler.net/) is the series of live-coding videos)

### Lox

* Implementations:
  * Official: <https://github.com/munificent/craftinginterpreters>
* Syntax highlighting:
  * <https://marketplace.visualstudio.com/items?itemName=dberezin.lox-language>

### CLR Internals

* [Managed Objects Internals series](https://devblogs.microsoft.com/premier-developer/managed-object-internals-part-4-fields-layout/)
* <https://stackoverflow.com/questions/8951828/clr-class-memory-layout/8951857?noredirect=1#comment129133194_8951857>
  * [CLR 1.1](https://web.archive.org/web/20080919091745/http://msdn.microsoft.com:80/en-us/magazine/cc163791.aspx)
  * [Update for CLR 4](https://web.archive.org/web/20200108021433/http://blogs.microsoft.co.il/sasha/2012/03/15/virtual-method-dispatch-and-object-layout-changes-in-clr-40/)

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
| Assignment     | =         | Left          | 10         |
| Equality       | == !=     | Left          | 20         |
| Comparison     | < > <= >= | Left          | 30         |
| Addition       | + -       | Left          | 40         |
| Multiplication | * /       | Left          | 50         |
| Unary          | + - !     | **Right**     | 60         |

For comparison: [C Operators](https://en.cppreference.com/w/c/language/operator_precedence)

See also [Evaluation order in C](https://en.cppreference.com/w/c/language/eval_order) (mostly unspecified!)