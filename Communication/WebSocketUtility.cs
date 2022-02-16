using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace WhirlpoolCore.Communication
{
    static class WebSocketUtility
    {
        public static ReadOnlySequence<byte> UnmaskSequence(ReadOnlySequence<byte> MaskedSequence, out int FrameHeaderByteCount)
        {
            int PayloadLength;
            int MaskingKeyOffset;

            byte[] UnmaskedBytes;
            byte[] MaskingKeyBytes = new byte[4];

            byte[] FrameBytes = MaskedSequence.ToArray<byte>();

            if ((FrameBytes[1] ^ 128) == 126)
            {
                byte[] PayloadLengthBytes = { FrameBytes[2], FrameBytes[3] };
                MaskingKeyOffset = 4;

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(PayloadLengthBytes);
                }

                PayloadLength = BitConverter.ToInt16(PayloadLengthBytes, 0);
            }

            else
            {
                PayloadLength = FrameBytes[1] - 128;
                MaskingKeyOffset = 2;
            }

            FrameHeaderByteCount = MaskingKeyOffset + 4;
            UnmaskedBytes = new byte[FrameBytes.Length];

            Array.Copy(FrameBytes, MaskingKeyOffset, MaskingKeyBytes, 0, 4);

            for (int n = 0; n < PayloadLength; n++)
            {
                UnmaskedBytes[FrameHeaderByteCount + n] = (byte)(MaskingKeyBytes[n % 4] ^ FrameBytes[n + MaskingKeyOffset + 4]);
            }

            return new ReadOnlySequence<byte>(UnmaskedBytes);
        }

        public static byte[] WriteFrameHeader(byte[] InputBytes)
        {
            int HeaderSize = (InputBytes.Length > 125) ? 4 : 2;

            byte[] OutputBytes = new byte[InputBytes.Length + HeaderSize];
            OutputBytes[0] = 0x82;

            if (HeaderSize == 4)
            {
                OutputBytes[1] = 0x7e;
                byte[] SizeBytes = BitConverter.GetBytes(InputBytes.Length);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(SizeBytes);
                }

                Array.Copy(SizeBytes, 0, OutputBytes, 2, 2);
            }

            else
            {
                OutputBytes[1] = Convert.ToByte(InputBytes.Length);
            }

            Array.Copy(InputBytes, 0, OutputBytes, HeaderSize, InputBytes.Length);

            return OutputBytes;
        }
    }
}
