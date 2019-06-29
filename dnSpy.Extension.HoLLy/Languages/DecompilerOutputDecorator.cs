using dnSpy.Contracts.Decompiler;

namespace HoLLy.dnSpy.Extension.Languages
{
    public class DecompilerOutputDecorator : IDecompilerOutput
    {
        private IDecompilerOutput implementation;

        public DecompilerOutputDecorator(IDecompilerOutput implementation) => this.implementation = implementation;

        public void Write(string text, object reference, DecompilerReferenceFlags flags, object color)
        {
            implementation.Write(text, reference, flags, color);
        }

        public void Write(string text, int index, int length, object reference, DecompilerReferenceFlags flags, object color)
        {
            implementation.Write(text, index, length, reference, flags, color);
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
