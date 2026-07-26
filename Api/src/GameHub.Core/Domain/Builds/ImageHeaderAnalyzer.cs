using System;
using System.Buffers.Binary;
using System.IO;

namespace GameHub.Builds
{
    /// <summary>
    /// Reads image dimensions from PNG, JPEG, GIF and WebP (VP8) headers without external dependencies.
    /// </summary>
    public static class ImageHeaderAnalyzer
    {
        public static (int Width, int Height)? TryGetDimensions(Stream stream)
        {
            if (stream == null || !stream.CanRead)
            {
                return null;
            }

            var header = new byte[32];
            var originalPosition = stream.CanSeek ? stream.Position : 0L;
            var read = stream.Read(header, 0, header.Length);
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }

            if (read < 8)
            {
                return null;
            }

            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
            {
                return ReadPng(header, stream);
            }

            if (header[0] == 0xFF && header[1] == 0xD8)
            {
                return ReadJpeg(stream);
            }

            if (header[0] == 'G' && header[1] == 'I' && header[2] == 'F')
            {
                return ReadGif(header);
            }

            if (header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F'
                && header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P')
            {
                return ReadWebP(header, stream);
            }

            return null;
        }

        private static (int, int)? ReadPng(byte[] header, Stream stream)
        {
            // PNG: IHDR starts at offset 16, width at 16, height at 20
            if (header.Length >= 24)
            {
                var width = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(16, 4));
                var height = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(20, 4));
                return (width, height);
            }

            return ReadBytes(stream, 16, 8);
        }

        private static (int, int)? ReadJpeg(Stream stream)
        {
            var position = stream.CanSeek ? stream.Position : 0;
            try
            {
                stream.Position = 2;
                while (true)
                {
                    if (stream.ReadByte() != 0xFF)
                    {
                        continue;
                    }

                    var marker = stream.ReadByte();
                    if (marker == -1 || marker == 0xD9 || marker == 0xD8)
                    {
                        return null;
                    }

                    if (marker == 0xC0 || marker == 0xC2)
                    {
                        var buffer = new byte[7];
                        if (stream.Read(buffer, 0, buffer.Length) < buffer.Length)
                        {
                            return null;
                        }

                        var height = (buffer[3] << 8) | buffer[4];
                        var width = (buffer[5] << 8) | buffer[6];
                        return (width, height);
                    }

                    var lengthBuffer = new byte[2];
                    if (stream.Read(lengthBuffer, 0, 2) < 2)
                    {
                        return null;
                    }

                    var length = (lengthBuffer[0] << 8) | lengthBuffer[1];
                    if (length < 2)
                    {
                        return null;
                    }

                    stream.Seek(length - 2, SeekOrigin.Current);
                }
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = position;
                }
            }
        }

        private static (int, int)? ReadGif(byte[] header)
        {
            if (header.Length < 10)
            {
                return null;
            }

            var width = header[6] | (header[7] << 8);
            var height = header[8] | (header[9] << 8);
            return (width, height);
        }

        private static (int, int)? ReadWebP(byte[] header, Stream stream)
        {
            if (header.Length < 28)
            {
                return null;
            }

            var chunkType = System.Text.Encoding.ASCII.GetString(header, 12, 4);
            if (chunkType != "VP8 ")
            {
                // VP8L parsing is not implemented; return null to skip dimension validation.
                return null;
            }

            var offset = 20;
            if (stream.CanSeek)
            {
                stream.Position = offset;
            }

            var vp8 = new byte[7];
            if (stream.Read(vp8, 0, vp8.Length) < vp8.Length)
            {
                return null;
            }

            // Skip 3-byte sync code 0x9d 0x01 0x2a
            var width = vp8[4] | ((vp8[5] & 0x3F) << 8);
            var height = ((vp8[5] >> 6) | (vp8[6] << 2)) & 0x3FFF;
            return (width, height);
        }

        private static (int, int)? ReadBytes(Stream stream, int offset, int count)
        {
            if (stream.CanSeek)
            {
                stream.Position = offset;
            }

            var buffer = new byte[count];
            if (stream.Read(buffer, 0, count) < count)
            {
                return null;
            }

            var width = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(0, 4));
            var height = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(4, 4));
            return (width, height);
        }
    }
}
