using System.Buffers.Binary;
using System.Text;

namespace semi.Core.Secs;

public static class SecsDecoder
{
    public static SecsItem Decode(byte[] data)
    {
        int offset = 0;
        return DecodeItem(data, ref offset);
    }

    private static SecsItem DecodeItem(byte[] data, ref int offset)
    {
        if (offset >= data.Length)
            throw new InvalidDataException("No more data to decode.");

        byte formatAndLengthByte = data[offset++];

        int lengthByteCount = formatAndLengthByte & 0x03;
        byte formatCode = (byte)(formatAndLengthByte & 0xFC);

        if (lengthByteCount < 1 || lengthByteCount > 3)
            throw new InvalidDataException("Invalid SECS-II length byte count.");

        if (offset + lengthByteCount > data.Length)
            throw new InvalidDataException("Invalid SECS-II length field.");

        int length = 0;

        for (int i = 0; i < lengthByteCount; i++)
        {
            length = (length << 8) | data[offset++];
        }

        var format = (SecsFormat)formatCode;

        return format switch
        {
            SecsFormat.List => DecodeList(data, ref offset, length),
            SecsFormat.Ascii => DecodeAscii(data, ref offset, length),
            SecsFormat.U4 => DecodeU4(data, ref offset, length),
            _ => throw new NotSupportedException($"Format {format} is not supported yet.")
        };
    }

    private static SecsItem DecodeList(byte[] data, ref int offset, int itemCount)
    {
        var items = new List<SecsItem>();

        for (int i = 0; i < itemCount; i++)
        {
            items.Add(DecodeItem(data, ref offset));
        }

        return SecsItem.L(items.ToArray());
    }

    private static SecsItem DecodeAscii(byte[] data, ref int offset, int length)
    {
        if (offset + length > data.Length)
            throw new InvalidDataException("Invalid ASCII item length.");

        string value = Encoding.ASCII.GetString(data, offset, length);
        offset += length;

        return SecsItem.A(value);
    }

    private static SecsItem DecodeU4(byte[] data, ref int offset, int length)
    {
        if (length % 4 != 0)
            throw new InvalidDataException("Invalid U4 item length.");

        if (offset + length > data.Length)
            throw new InvalidDataException("Invalid U4 item length.");

        int count = length / 4;
        uint[] values = new uint[count];

        for (int i = 0; i < count; i++)
        {
            values[i] = BinaryPrimitives.ReadUInt32BigEndian(
                data.AsSpan(offset + i * 4, 4));
        }

        offset += length;

        return SecsItem.U4(values);
    }
}