﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Slang.Utilities;

// Simplified from https://raw.githubusercontent.com/dotnet/efcore/main/src/EFCore/Infrastructure/IndentedStringBuilder.cs
public sealed class IndentedStringBuilder
{
    private const byte indentSize = 4;
    private bool indentPending = true;

    private readonly StringBuilder builder = new();

    public byte CurrentIndent { get; private set; }
    public int Length => builder.Length;

    public IndentedStringBuilder Indent()
    {
        CurrentIndent++;
        return this;
    }

    public IndentedStringBuilder Dedent()
    {
        if (CurrentIndent > 0) CurrentIndent--;
        return this;
    }

    public IndentedStringBuilder Append(string value)
    {
        DoIndent();
        _ = builder.Append(value);
        return this;
    }

    public IndentedStringBuilder AppendLine() => AppendLine(string.Empty);
    public IndentedStringBuilder AppendLine(string value)
    {
        if (value.Length != 0)
            DoIndent();

        _ = builder.AppendLine(value);
        indentPending = true;
        return this;
    }

    public override string ToString() => builder.ToString();

    private void DoIndent()
    {
        if (indentPending && CurrentIndent > 0)
            _ = builder.Append(' ', CurrentIndent * indentSize);
        indentPending = false;
    }
}