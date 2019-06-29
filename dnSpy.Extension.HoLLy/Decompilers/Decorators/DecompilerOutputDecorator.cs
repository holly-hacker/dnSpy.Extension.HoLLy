using System.Linq;
using dnlib.DotNet;
using dnSpy.Contracts.Decompiler;

namespace HoLLy.dnSpy.Extension.Decompilers.Decorators
{
    public class DecompilerOutputDecorator : IDecompilerOutput
    {
        private IDecompilerOutput implementation;

        public DecompilerOutputDecorator(IDecompilerOutput implementation) => this.implementation = implementation;

        public void Write(string text, object reference, DecompilerReferenceFlags flags, object color) => implementation.Write(Modify(text, reference), reference, flags, color);
        public void Write(string text, int index, int length, object reference, DecompilerReferenceFlags flags, object color) => implementation.Write(Modify(text, reference), index, length, reference, flags, color);

        private static string Modify(string text, object reference)
        {
            switch (reference) {
                case MethodDef _:
                    return new string(text.Reverse().ToArray());
                case NamespaceReference _:
                    return $"<{text}>";
                default:
                    return text;
            }
        }

        #region standard implementation
        public int Length => implementation.Length;
        public int NextPosition => implementation.NextPosition;

        public void IncreaseIndent() => implementation.IncreaseIndent();
        public void DecreaseIndent() => implementation.DecreaseIndent();
        public void WriteLine() => implementation.WriteLine();
        public void Write(string text, object color) => implementation.Write(text, color);
        public void Write(string text, int index, int length, object color) => implementation.Write(text, index, length, color);

        public void AddCustomData<TData>(string id, TData data) => implementation.AddCustomData(id, data);
        public bool UsesCustomData => implementation.UsesCustomData;
        #endregion
    }
}
