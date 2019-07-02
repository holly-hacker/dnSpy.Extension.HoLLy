using dnlib.DotNet;
using dnSpy.Contracts.Decompiler;
using HoLLy.dnSpy.Extension.SourceMap;

namespace HoLLy.dnSpy.Extension.Decompilers.Decorators
{
    internal class DecompilerOutputDecorator : IDecompilerOutput
    {
        private readonly IDecompilerOutput implementation;
        private readonly ISourceMapStorage sourceMap;

        public DecompilerOutputDecorator(IDecompilerOutput implementation, ISourceMapStorage sourceMap)
        {
            this.implementation = implementation;
            this.sourceMap = sourceMap;
        }

        public void Write(string text, object reference, DecompilerReferenceFlags flags, object color) => implementation.Write(Modify(text, reference), reference, flags, color);
        public void Write(string text, int index, int length, object reference, DecompilerReferenceFlags flags, object color) => implementation.Write(Modify(text, reference), index, length, reference, flags, color);

        private string Modify(string text, object reference)
        {
            switch (reference) {
                case IMemberDef memberDef:
                    return sourceMap.GetName(memberDef) ?? text;
                default:
                    return text;
            }
        }

        #region default implementation
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
