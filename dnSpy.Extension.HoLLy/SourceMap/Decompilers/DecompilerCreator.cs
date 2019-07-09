using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Decompiler;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.SourceMap;
using HoLLy.dnSpyExtension.SourceMap.Decompilers.Decorators;

namespace HoLLy.dnSpyExtension.SourceMap.Decompilers
{
    [Export(typeof(IDecompilerCreator))]
    internal class DecompilerCreator : IDecompilerCreator
    {
        private const string DLLName = "dnSpy.Decompiler.ILSpy.Core.dll";
        private const string TypeNameFormat = "dnSpy.Decompiler.ILSpy.Core.{0}.DecompilerProvider";
        private static readonly string[] LanguageNames = { "CSharp", "IL", "ILAst", "VisualBasic" };

        private readonly ISourceMapStorage sourceMapStorage;

        [ImportingConstructor]
        public DecompilerCreator(ISourceMapStorage sourceMapStorage)
        {
            this.sourceMapStorage = sourceMapStorage;
        }

        public IEnumerable<IDecompiler> Create()
        {
            foreach (string languageName in LanguageNames) {
                var provider = TryCreateDecompilerProvider(languageName);

                if (provider is null) {
                    if (Utils.IsDebugBuild)
                        MsgBox.Instance.Show($"Couldn't load decompiler provider for '{languageName}', was null");

                    yield break;
                }

                foreach (IDecompiler dec in provider.Create())
                    yield return new DecompilerDecorator(dec, sourceMapStorage);
            }
        }

        private static IDecompilerProvider? TryCreateDecompilerProvider(string languageName)
        {
            var dnSpyDir = Path.GetDirectoryName(typeof(IDecompilerCreator).Assembly.Location);
            if (dnSpyDir is null) return null;

            string dllPath = Path.Combine(dnSpyDir, DLLName);

            var asm = Assembly.LoadFile(dllPath);
            var type = asm.GetTypes().SingleOrDefault(x => x.FullName == String.Format(TypeNameFormat, languageName));
            if (type is null) return null;

            return (IDecompilerProvider)Activator.CreateInstance(type);
        }
    }
}
