using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Decompiler;
using HoLLy.dnSpy.Extension.Decompilers.Decorators;

namespace HoLLy.dnSpy.Extension.Decompilers
{
    [Export(typeof(IDecompilerCreator))]
    public class DecompilerCreator : IDecompilerCreator
    {
        private const string dllName = "dnSpy.Decompiler.ILSpy.Core.dll";
        private const string typeName = "dnSpy.Decompiler.ILSpy.Core.CSharp.DecompilerProvider";

        public IEnumerable<IDecompiler> Create()
        {
            var provider = TryCreateDecompilerProvider();

            if (provider is null && Utils.IsDebugBuild) {
                MsgBox.Instance.Show("Couldn't load decompiler provider, was null");
            }

            return provider?.Create().Select(dec => new DecompilerDecorator(dec));
        }

        internal static IDecompilerProvider TryCreateDecompilerProvider()
        {
            var dnSpyDir = Path.GetDirectoryName(typeof(IDecompilerCreator).Assembly.Location);
            if (dnSpyDir is null) return null;

            string dllPath = Path.Combine(dnSpyDir, dllName);

            var asm = Assembly.LoadFile(dllPath);
            var type = asm.GetTypes().SingleOrDefault(x => x.FullName == typeName);
            if (type is null) return null;

            return (IDecompilerProvider)Activator.CreateInstance(type);
        }
    }
}
