namespace Slang.Utils;

// NB: both Line and Column are 0-based
public readonly record struct LinePosition(int Line, int Column);
