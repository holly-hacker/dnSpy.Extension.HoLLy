using System;
using System.Collections.Generic;
using System.Linq;
using dnSpy.Contracts.Text;

namespace HoLLy.dnSpyExtension.Common.Logging
{
    internal class CachedTextColorWriter : ITextColorWriter
    {
        private ITextColorWriter? writer;
        private readonly Queue<(object color, string? text, bool usesObject)> queue = new();

        public ITextColorWriter Writer
        {
            set
            {
                writer = writer is null
                    ? value ?? throw new ArgumentNullException(nameof(value))
                    : throw new InvalidOperationException($"Tried to initialize {nameof(CachedTextColorWriter)} twice");

                while (queue.Any())
                {
                    (var color, string? text, bool usesObject) = queue.Dequeue();
                    if (usesObject)
                        writer.Write(color, text);
                    else
                        writer.Write((TextColor)color, text);
                }
            }
        }

        public void Write(object color, string? text)
        {
            if (writer is null)
                queue.Enqueue((color, text, true));
            else
                writer.Write(color, text);
        }

        public void Write(TextColor color, string? text)
        {
            if (writer is null)
                queue.Enqueue((color, text, false));
            else
                writer.Write(color, text);
        }
    }
}