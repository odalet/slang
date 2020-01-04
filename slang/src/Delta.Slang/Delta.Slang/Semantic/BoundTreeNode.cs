using System.IO;
using Delta.Slang.Utils;

namespace Delta.Slang.Semantic
{
    internal abstract class BoundTreeNode
    {
        public abstract BoundTreeNodeKind Kind { get; }

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                this.WriteTo(writer);
                return writer.ToString();
            }
        }
    }
}
