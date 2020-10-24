using System;
using System.Collections.Generic;
using System.Linq;
using dnSpy.Contracts.Text;

namespace HoLLy.dnSpyExtension.Common.Logging
{
    internal class CachedTextColorWriter : ITextColorWriter
    {
        private ITextColorWriter? _writer;
        private readonly Queue<(object color, string? text, bool usesObject)> _queue = new Queue<(object color, string? text, bool usesObject)>();

        public ITextColorWriter Writer
        {
            set
            {
                _writer = _writer is null
                    ? value ?? throw new ArgumentNullException(nameof(value))
                    : throw new InvalidOperationException($"Tried to initialize {nameof(CachedTextColorWriter)} twice");

                while (_queue.Any())
                {
                    (var color, string? text, bool usesObject) = _queue.Dequeue();
                    if (usesObject)
                        _writer.Write(color, text);
                    else
                        _writer.Write((TextColor)color, text);
                }
            }
        }

        public void Write(object color, string? text)
        {
            if (_writer is null)
                _queue.Enqueue((color, text, true));
            else
                _writer.Write(color, text);
        }

        public void Write(TextColor color, string? text)
        {
            if (_writer is null)
                _queue.Enqueue((color, text, false));
            else
                _writer.Write(color, text);
        }
    }
}