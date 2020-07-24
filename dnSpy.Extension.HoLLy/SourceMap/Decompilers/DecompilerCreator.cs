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

namespace HoLLy.dnSpyExtension.SourceMap.Decompilers
{
    [Export(typeof(IDecompilerCreator))]
    internal class DecompilerCreator : IDecompilerCreator
    {
        private const string DllName = "dnSpy.Decompiler.ILSpy.Core.dll";
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
            // I would import a IDecompilerService instance in the constructor, but it seems it wouldn't be created yet?
            // Instead, we have this hacky thing.

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

        /// <summary>
        /// Creates a <see cref="IDecompilerProvider"/> for the given <paramref name="languageName"/> by using
        /// reflection to load it from the dnSpy binary.
        /// </summary>
        private static IDecompilerProvider? TryCreateDecompilerProvider(string languageName)
        {
            var dnSpyDir = Path.GetDirectoryName(typeof(IDecompilerCreator).Assembly.Location);
            if (dnSpyDir is null) return null;

            string dllPath = Path.Combine(dnSpyDir, DllName);

            var asm = Assembly.LoadFile(dllPath);
            var type = asm.GetTypes().SingleOrDefault(x => x.FullName == String.Format(TypeNameFormat, languageName));
            if (type is null) return null;

            return (IDecompilerProvider)Activator.CreateInstance(type);
        }
    }
}
