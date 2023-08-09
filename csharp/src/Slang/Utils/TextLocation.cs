namespace Slang.Utils;

public readonly record struct TextLocation(int Start, int Length)
{
    public int End { get; } = Start + Length;
}
