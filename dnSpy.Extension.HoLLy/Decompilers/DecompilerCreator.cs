using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Decompiler;
using HoLLy.dnSpy.Extension.Decompilers.Decorators;
using HoLLy.dnSpy.Extension.SourceMap;

namespace HoLLy.dnSpy.Extension.Decompilers
{
    [Export(typeof(IDecompilerCreator))]
    internal class DecompilerCreator : IDecompilerCreator
    {
        private readonly ISourceMapStorage sourceMapStorage;
        private const string DLLName = "dnSpy.Decompiler.ILSpy.Core.dll";
        private const string TypeName = "dnSpy.Decompiler.ILSpy.Core.CSharp.DecompilerProvider";

        [ImportingConstructor]
        public DecompilerCreator(ISourceMapStorage sourceMapStorage)
        {
            this.sourceMapStorage = sourceMapStorage;
        }

        public IEnumerable<IDecompiler> Create()
        {
            var provider = TryCreateDecompilerProvider();

            if (provider is null) {
                if (Utils.IsDebugBuild)
                    MsgBox.Instance.Show("Couldn't load decompiler provider, was null");
                yield break;
            }

            foreach (IDecompiler dec in provider.Create()) {
                yield return new DecompilerDecorator(dec, sourceMapStorage);
            }
        }
        internal static IDecompilerProvider TryCreateDecompilerProvider()
        {
            var dnSpyDir = Path.GetDirectoryName(typeof(IDecompilerCreator).Assembly.Location);
            if (dnSpyDir is null) return null;

            string dllPath = Path.Combine(dnSpyDir, DLLName);

            var asm = Assembly.LoadFile(dllPath);
            var type = asm.GetTypes().SingleOrDefault(x => x.FullName == TypeName);
            if (type is null) return null;

            return (IDecompilerProvider)Activator.CreateInstance(type);
        }
    }
}
