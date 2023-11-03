namespace Slang.Utils;

public readonly record struct TextLocation(int Start, int Length)
{
    public int End { get; } = Start + Length;
    public TextLocation WithEnd(int endPosition) => WithLength(endPosition - Start);
    public TextLocation WithLength(int length) => new(Start, length);
}
