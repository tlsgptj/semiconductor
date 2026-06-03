using System.Buffers.Binary;
using System.Text;

namespace semi.Core.Secs;

public static class SecsEncoder
{
    public static byte[] Encode(SecsItem item)
    {
        return item.Format switch
        {
            SecsFormat.List => EncodeList(item),
            SecsFormat.Ascii => EncodeAscii(item),
            SecsFormat.U4 => EncodeU4(item),
            _ => throw new NotSupportedException($"Format {item.Format} is not supported yet.")
        };
    }

    private static byte[] EncodeList(SecsItem item)
    {
        if (item.Items == null)
        {
            throw new InvalidOperationException("List item must have child items.");
        }

        var body = new List<byte>();

        foreach (var child in item.Items)
        {
            body.AddRange(Encode(child));
        }

        // List의 Length는 byte 길이가 아니라 item 개수
        byte[] header = CreateHeader(SecsFormat.List, item.Items.Count);

        return header.Concat(body).ToArray();
    }

    private static byte[] EncodeAscii(SecsItem item)
    {
        if (item.AsciiValue == null)
        {
            throw new InvalidOperationException("ASCII item must have value.");
        }

        byte[] data = Encoding.ASCII.GetBytes(item.AsciiValue);
        byte[] header = CreateHeader(SecsFormat.Ascii, data.Length);

        return header.Concat(data).ToArray();
    }

    private static byte[] EncodeU4(SecsItem item)
    {
        if (item.U4Values == null)
        {
            throw new InvalidOperationException("U4 item must have value.");
        }

        byte[] data = new byte[item.U4Values.Length * 4];

        for (int i = 0; i < item.U4Values.Length; i++)
        {
            BinaryPrimitives.WriteUInt32BigEndian(
                data.AsSpan(i * 4, 4),
                item.U4Values[i]);
        }

        byte[] header = CreateHeader(SecsFormat.U4, data.Length);

        return header.Concat(data).ToArray();
    }

    private static byte[] CreateHeader(SecsFormat format, int length)
    {
        if (length <= 0xFF)
        {
            return new[]
            {
                (byte)((byte)format | 0x01),
                (byte)length
            };
        }

        if (length <= 0xFFFF)
        {
            return new[]
            {
                (byte)((byte)format | 0x02),
                (byte)((length >> 8) & 0xFF),
                (byte)(length & 0xFF)
            };
        }

        if (length <= 0xFFFFFF)
        {
            return new[]
            {
                (byte)((byte)format | 0x03),
                (byte)((length >> 16) & 0xFF),
                (byte)((length >> 8) & 0xFF),
                (byte)(length & 0xFF)
            };
        }

        throw new InvalidOperationException("SECS-II item length is too large.");
    }
}