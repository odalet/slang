////using System;
////using System.IO;
////using System.Linq;
////using Delta.Slang.Symbols;
////using Delta.Slang.Syntax;

////namespace Delta.Slang.Utils
////{
////    internal static class SymbolPrinter
////    {
////        public static void WriteTo(this Symbol symbol, TextWriter writer)
////        {
////            switch (symbol)
////            {
////                case FunctionSymbol f:
////                    WriteFunctionTo(f, writer);
////                    break;
////                case ParameterSymbol p:
////                    WriteParameterTo(p, writer);
////                    break;
////                case GlobalVariableSymbol gv:
////                    WriteVariableTo(gv, writer);
////                    break;
////                case LocalVariableSymbol lv:
////                    WriteVariableTo(lv, writer);
////                    break;
////                case TypeSymbol t:
////                    WriteTypeTo(t, writer);
////                    break;
////                default:
////                    throw new ArgumentException($"Unexpected symbol: {symbol.Kind}", nameof(symbol));
////            }
////        }

////        private static void WriteFunctionTo(FunctionSymbol symbol, TextWriter writer)
////        {
////            writer.WriteKeyword(TokenKind.FunKeyword);
////            writer.WriteSpace();
////            writer.WriteIdentifier(symbol.Name);
////            writer.WritePunctuation(TokenKind.OpenParenthesis);

////            var first = true;
////            foreach (var parameter in symbol.Parameters)
////            {
////                if (first) first = false;
////                else
////                {
////                    writer.WritePunctuation(TokenKind.Comma);
////                    writer.WriteSpace();
////                }

////                parameter.WriteTo(writer);
////            }

////            writer.WritePunctuation(TokenKind.CloseParenthesis);

////            writer.WritePunctuation(TokenKind.Colon);
////            writer.WriteSpace();
////            symbol.Type.WriteTo(writer);
////        }
        
////        private static void WriteVariableTo(VariableSymbol symbol, TextWriter writer)
////        {
////            writer.WriteKeyword(TokenKind.VarKeyword);
////            writer.WriteSpace();
////            writer.WriteIdentifier(symbol.Name);
////            writer.WritePunctuation(TokenKind.Colon);
////            writer.WriteSpace();
////            symbol.Type.WriteTo(writer);
////        }

////        private static void WriteParameterTo(ParameterSymbol symbol, TextWriter writer)
////        {
////            writer.WriteIdentifier(symbol.Name);
////            writer.WritePunctuation(TokenKind.Colon);
////            writer.WriteSpace();
////            symbol.Type.WriteTo(writer);
////        }

////        private static void WriteTypeTo(TypeSymbol symbol, TextWriter writer) => writer.WriteIdentifier(symbol.Name);
////    }
////}
