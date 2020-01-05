using System;
using System.IO;
using Delta.Slang.Symbols;
using Delta.Slang.Syntax;

namespace Delta.Slang.Utils
{
    public sealed class Unparser
    {
        private int indentLevel = 0;

        public Unparser(ParseTree tree) => Tree = tree ?? throw new ArgumentNullException(nameof(tree));

        private ParseTree Tree { get; }

        private string Tabs => indentLevel > 0 ? new string(' ', 3 * indentLevel) : "";

        public void Unparse(TextWriter writer) => Unparse(writer, Tree.Root);

        private void Unparse(TextWriter writer, SyntaxNode node)
        {
            const string lb = "\r\n";

            void tabs() => writer.Write(Tabs);
            void ltabs() => writer.Write(lb + Tabs);
            void wl(string text = null) => writer.WriteLine(text ?? "");
            void w(string text) => writer.Write(text ?? "");
            void unparse(SyntaxNode n) => Unparse(writer, n);
            void walk()
            {
                foreach (var child in node.Children)
                    unparse(child);
            }

            switch (node)
            {
                case CompilationUnitNode _:
                    tabs();
                    w("// Compilation Unit");
                    walk();
                    break;
                case GlobalStatementNode _:
                    walk();
                    break;
                case FunctionDeclarationNode fd:
                    ltabs();
                    w($"fun {fd.FunctionName.Text}(");
                    unparse(fd.ParametersDeclaration);
                    w("): ");
                    unparse(fd.ReturnType);
                    unparse(fd.Body);
                    break;
                case ParametersDeclarationNode psd:
                    var isFirst = true;
                    foreach (var param in psd.Parameters)
                    {
                        if (isFirst) isFirst = false; else w(", ");
                        unparse(param);
                    }
                    break;
                case ParameterDeclarationNode pd:
                    w($"{pd.ParameterName.Text}: ");
                    unparse(pd.Type);
                    break;
                case TypeClauseNode tc:
                    w(tc.TypeName.Text);
                    break;
                case BlockNode _:
                    w(" {");
                    Indent();
                    walk();
                    Dedent();
                    wl();
                    tabs();
                    wl("}");
                    break;
                // Statements
                case VariableDeclarationNode vd:
                    ltabs();
                    w($"var {vd.VariableName.Text}");
                    if (vd.Type != null)
                    {
                        w(": ");
                        unparse(vd.Type);
                    }
                    if (vd.Initializer != null)
                    {
                        w(" = ");
                        unparse(vd.Initializer);
                    }
                    w(";");
                    break;
                case ExpressionStatementNode es:
                    ltabs();
                    unparse(es.Expression);
                    w(";");
                    break;
                case GotoStatementNode gs:
                    ltabs();
                    w("goto ");
                    unparse(gs.Label);
                    w(";");
                    break;
                case LabelStatementNode ls:
                    wl();
                    unparse(ls.Label);
                    w(":");
                    break;
                case IfStatementNode ifs:
                {
                    ltabs();
                    w("if (");
                    unparse(ifs.Condition);
                    w(")");

                    var isBlock = ifs.Statement is BlockNode;
                    if (!isBlock)
                    {
                        Indent();
                        unparse(ifs.Statement);
                        Dedent();
                    }
                    else unparse(ifs.Statement);

                    if (ifs.Else != null)
                    {
                        if (!isBlock)
                            ltabs();
                        unparse(ifs.Else);
                    }
                }
                break;
                case ElseClauseNode ec:
                {
                    w("else");
                    var isBlock = ec.Statement is BlockNode;
                    if (!isBlock)
                    {
                        Indent();
                        unparse(ec.Statement);
                        Dedent();
                    }
                    else unparse(ec.Statement);
                }
                break;
                case ReturnStatementNode rs:
                    ltabs();
                    w("return");
                    if (rs.Expression != null)
                    {
                        w(" ");
                        unparse(rs.Expression);
                    }
                    w(";");
                    break;
                // Expressions
                case InvokeExpressionNode ie:
                    w($"{ie.FunctionName.Text}(");
                    var isFirstParam = true;
                    foreach (var param in ie.Arguments)
                    {
                        if (isFirstParam) isFirstParam = false; else w(", ");
                        unparse(param);
                    }
                    w(")");
                    break;
                case UnaryExpressionNode ue:
                    w(ue.Operator.Text);
                    unparse(ue.Operand);
                    break;
                case BinaryExpressionNode be:
                    unparse(be.Left);
                    w($" {be.Operator.Text} ");
                    unparse(be.Right);
                    break;
                case NameExpressionNode ne:
                    w(ne.Identifier.Text);
                    break;
                case ParenthesizedExpressionNode pe:
                    w("(");
                    unparse(pe.Expression);
                    w(")");
                    break;
                case AssignmentExpressionNode ae:
                    w($"{ae.Identifier.Text} = ");
                    unparse(ae.Expression);
                    break;
                case LiteralExpressionNode le:
                    if (le.Type == BuiltinTypes.String)
                        w("\"");
                    if (le.Literal.Value != null)
                        w(le.Literal.Value.ToString());
                    else
                        w(le.Literal.Text);
                    if (le.Type == BuiltinTypes.String)
                        w("\"");
                    break;
                default:
                    w("<?>");
                    walk();
                    break;
            }
        }

        private void Indent() => indentLevel++;
        private void Dedent() => indentLevel--;
    }
}
