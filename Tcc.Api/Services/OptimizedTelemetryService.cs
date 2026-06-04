using System.Buffers;
using System.Buffers.Text;
using System.IO.Pipelines;
using System.Text;
using Tcc.Api.Interfaces;
using Tcc.Api.Models;

namespace Tcc.Api.Services
{
    public class OptimizedTelemetryService : ITelemetryProcessor
    {
        private struct TelemetryState
        {
            public int TotalItems;
            public int CheckWarnings;
            public double CheckTemperature;
            public long CheckDateTime;
        }

        public async Task<ProcessingResult> Process(Stream sensorData)
        {
            var reader = PipeReader.Create(sensorData);

            TelemetryState state = await ReadPipeAsync(reader);

            return new ProcessingResult
            {
                CheckTemp = state.CheckTemperature,
                CheckDateTime = state.CheckDateTime,
                CheckStatusWarning = state.CheckWarnings,
                TotalItems = state.TotalItems
            };
        }

        private async Task<TelemetryState> ReadPipeAsync(PipeReader reader)
        {
            var state = new TelemetryState();

            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
                {
                    if (line.IsSingleSegment)
                    {
                        ProcessLine(line.First.Span, ref state);
                    }
                    else
                    {
                        Span<byte> continuousLine = stackalloc byte[(int)line.Length];
                        line.CopyTo(continuousLine);
                        ProcessLine(continuousLine, ref state);
                    }
                }

                reader.AdvanceTo(buffer.Start, buffer.End);
                if (result.IsCompleted) break;
            }

            await reader.CompleteAsync();
            return state;
        }

        private void ProcessLine(ReadOnlySpan<byte> lineSpan, ref TelemetryState state)
        {
            if (lineSpan.Length < 10) return;

            state.TotalItems++;

            ReadOnlySpan<byte> keyTimestamp = "\"Timestamp\":"u8;
            ReadOnlySpan<byte> keyValue = "\"Value\":"u8;
            ReadOnlySpan<byte> keyStatus = "\"Status\":"u8;

            int index = lineSpan.IndexOf(keyTimestamp);
            if (index != -1)
            {
                var slice = lineSpan.Slice(index + keyTimestamp.Length);

                int end = slice.IndexOf((byte)',');
                if (end == -1) end = slice.Length;

                var valueSpan = slice.Slice(0, end).Trim(" "u8).Trim((byte)'"');

                Span<char> charBuffer = stackalloc char[32];
                int charsWritten = Encoding.UTF8.GetChars(valueSpan, charBuffer);

                if (DateTime.TryParse(charBuffer.Slice(0, charsWritten), out DateTime dt))
                    state.CheckDateTime += dt.Ticks;
            }

            index = lineSpan.IndexOf(keyValue);
            if (index != -1)
            {
                var slice = lineSpan.Slice(index + keyValue.Length);

                int end = slice.IndexOf((byte)',');
                if (end == -1) end = slice.Length;

                var valueSpan = slice.Slice(0, end).Trim(" "u8);

                if (Utf8Parser.TryParse(valueSpan, out double temp, out _))
                    state.CheckTemperature += temp;
            }


            index = lineSpan.IndexOf(keyStatus);
            if (index != -1)
            {
                var slice = lineSpan.Slice(index + keyStatus.Length);

                int end = slice.IndexOf((byte)',');
                if (end == -1) end = slice.Length;

                var valueSpan = slice.Slice(0, end).Trim(" "u8).Trim((byte)'"');

                if (valueSpan.SequenceEqual("Warning"u8))
                    state.CheckWarnings++;
            }
        }

        private bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            SequencePosition? position = buffer.PositionOf((byte)'}');

            if (position == null)
            {
                line = default;
                return false;
            }

            line = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            return true;
        }
    }
}