using System;
using System.Diagnostics;
using System.Text.Json;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Images;
using dnSpy.Contracts.Text;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.DetectItEasy.Models;

namespace HoLLy.dnSpyExtension.DetectItEasy
{
    public class DieTreeNode : DocumentTreeNodeData, IDecompileSelf
    {
        private readonly string _filePath;
        private readonly string _diePath;

        public override Guid Guid => Constants.DetectItEasyNodeGuid;
        public override NodePathName NodePathName => new NodePathName(Guid);

        public DieTreeNode(string filePath, string diePath)
        {
            _filePath = filePath;
            _diePath = diePath;
        }

        protected override ImageReference GetIcon(IDotNetImageService dnImgMgr) => DsImages.Binary;

        protected override void WriteCore(ITextColorWriter output, IDecompiler decompiler,
            DocumentNodeWriteOptions options) => output.Write(TextColor.Text, "Detect-It-Easy");

        public bool Decompile(IDecompileNodeContext context)
        {
            var startInfo = new ProcessStartInfo(_diePath, $"-d -j \"{_filePath}\"")
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            var proc = Process.Start(startInfo);
            var output = proc?.StandardOutput.ReadToEnd();

            if (output is null)
            {
                context.Output.WriteLine("Something went wrong, couldn't start Detect-It-Easy", TextColor.Error);
                return true;
            }

            var parsed = JsonSerializer.Deserialize<DieOutput>(output, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
            if (parsed is null)
            {
                context.Output.WriteLine("Failed to read Detect-It-Easy output", TextColor.Error);
                return true;
            }

            context.Output.Write("FileType: ", TextColor.Keyword);
            context.Output.WriteLine(parsed.Filetype, TextColor.Text);
            context.Output.WriteLine();

            foreach (var parsedDetect in parsed.Detects)
            {
                context.Output.Write(parsedDetect.Type, TextColor.Keyword);
                context.Output.Write(": ", TextColor.Text);
                context.Output.Write(parsedDetect.Name, TextColor.Text);
                context.Output.Write("(", TextColor.Text);
                context.Output.Write(parsedDetect.Version, TextColor.Number);
                context.Output.Write(")[", TextColor.Text);
                context.Output.Write(parsedDetect.Options, TextColor.Number);
                context.Output.Write("]", TextColor.Text);
                context.Output.WriteLine();
            }

            return true;
        }
    }
}