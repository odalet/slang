using System.Runtime.InteropServices;

namespace Slang;

// Similar to C#'s System.Void, but can be used in code to represent an 'empty' type
[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Unit 
{
    public static Unit Value { get; } = new Unit();
}
