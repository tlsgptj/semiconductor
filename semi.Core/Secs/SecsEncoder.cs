using System.Text;

namespace semi.Core.Secs;

// Encoding SECS-II items into byte arrays according to the SECS-II specification
public static class SecsEncoder
{
    public static byte[] Encode(SecsItem item)
    {
        return item.Format switch
        {
            SecsFormat.List => EncodeList(item),
            SecsFormat.Ascii => EncodeAscii(item),
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