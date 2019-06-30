using System;
using dnlib.DotNet;
using dnSpy.Contracts.Decompiler;

namespace HoLLy.dnSpy.Extension.Decompilers.Decorators
{
    public class DecompilerOutputDecorator : IDecompilerOutput
    {
        private IDecompilerOutput implementation;
        private readonly Func<IMemberDef, string> mappingFunc;

        public DecompilerOutputDecorator(IDecompilerOutput implementation, Func<IMemberDef, string> mappingFunc)
        {
            this.implementation = implementation;
            this.mappingFunc = mappingFunc;
        }

        public void Write(string text, object reference, DecompilerReferenceFlags flags, object color) => implementation.Write(Modify(text, reference), reference, flags, color);
        public void Write(string text, int index, int length, object reference, DecompilerReferenceFlags flags, object color) => implementation.Write(Modify(text, reference), index, length, reference, flags, color);

        private string Modify(string text, object reference)
        {
            switch (reference) {
                case IMemberDef memberDef:
                    return mappingFunc(memberDef);
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
