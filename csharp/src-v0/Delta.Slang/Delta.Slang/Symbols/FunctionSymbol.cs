using System;
using System.Collections.Generic;
using System.Linq;
using Delta.Slang.Syntax;

namespace Delta.Slang.Symbols;

public enum FunctionSymbolKind
{
    NotAnOperator = 0,
    PrefixUnary = 1,
    PostfixUnary = 2,
    InfixBinary = 3,
}

public class FunctionSymbol : Symbol
{
    public FunctionSymbol(string name, IEnumerable<ParameterSymbol> parameters, TypeSymbol type, FunctionDeclarationNode declaration = null) : base(name)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Declaration = declaration;

        Key = SymbolKey.FromFunction(name, parameters.Select(p => p.Type));
    }

    public override SymbolKind Kind => SymbolKind.Function;
    public virtual FunctionSymbolKind FunctionKind => FunctionSymbolKind.NotAnOperator;

    public override SymbolKey Key { get; }
    public FunctionDeclarationNode Declaration { get; }
    public IEnumerable<ParameterSymbol> Parameters { get; }
    public TypeSymbol Type { get; }        
}

public interface IOperatorFunctionSymbol
{
    FunctionSymbolKind FunctionKind { get; }
    OperatorDescriptor GetOperatorDescriptor();
}

// A special function associated with a prefix unary operator
public class UnaryOperatorFunctionSymbol : FunctionSymbol, IOperatorFunctionSymbol
{
    public UnaryOperatorFunctionSymbol(UnaryOperatorDescriptor op, TypeSymbol type) : this(op, type, type) { }

    public UnaryOperatorFunctionSymbol(UnaryOperatorDescriptor op, TypeSymbol operandType, TypeSymbol resultType) :
        base(MakeName(op), new[] { MakeParameter(operandType) }, resultType)
    {
        OperatorDescriptor = op;
        OperandType = operandType;
    }
    
    public override FunctionSymbolKind FunctionKind => FunctionSymbolKind.PrefixUnary;
    public UnaryOperatorDescriptor OperatorDescriptor { get; }
    public TypeSymbol OperandType { get; }

    OperatorDescriptor IOperatorFunctionSymbol.GetOperatorDescriptor() => OperatorDescriptor;

    private static string MakeName(UnaryOperatorDescriptor op) => op.FunctionName;
    private static ParameterSymbol MakeParameter(TypeSymbol type) => new("operand", type);
}

// A special function associated with a infix binary operator
public class BinaryOperatorFunctionSymbol : FunctionSymbol, IOperatorFunctionSymbol
{
    public BinaryOperatorFunctionSymbol(BinaryOperatorDescriptor op, TypeSymbol type) : this(op, type, type) { }
    public BinaryOperatorFunctionSymbol(BinaryOperatorDescriptor op, TypeSymbol type, TypeSymbol resultType) : this(op, type, type, resultType) { }
    public BinaryOperatorFunctionSymbol(BinaryOperatorDescriptor op, TypeSymbol lhsType, TypeSymbol rhsType, TypeSymbol resultType) :
        base(MakeName(op), new[] { MakeLhs(lhsType), MakeRhs(rhsType) }, resultType)
    {
        OperatorDescriptor = op;
        LeftType = lhsType;
        RightType = rhsType;
    }
    
    public override FunctionSymbolKind FunctionKind => FunctionSymbolKind.PrefixUnary;
    public BinaryOperatorDescriptor OperatorDescriptor { get; }
    public TypeSymbol LeftType { get; }
    public TypeSymbol RightType { get; }

    OperatorDescriptor IOperatorFunctionSymbol.GetOperatorDescriptor() => OperatorDescriptor;

    private static string MakeName(BinaryOperatorDescriptor op) => op.FunctionName;
    private static ParameterSymbol MakeLhs(TypeSymbol type) => new("lhs", type);
    private static ParameterSymbol MakeRhs(TypeSymbol type) => new("rhs", type);
}
