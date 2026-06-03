using System.Buffers.Binary;
using System.Net.Sockets;

namespace semi.Core.Hsms;

/// <summary>
/// TCP Stream에서 HSMS 메세지를 정확히 읽는 역할
/// </summary>
public static class HsmsMessageReader
{
    public static async Task<HsmsMessage?> ReadAsync(
        NetworkStream stream,
        CancellationToken cancellationToken = default)
    {
        byte[] lengthBuffer = new byte[4];

        bool lengthRead = await ReadExactAsync(
            stream,
            lengthBuffer,
            4,
            cancellationToken);

        if (!lengthRead)
        {
            return null;
        }

        int messageLength = BinaryPrimitives.ReadInt32BigEndian(lengthBuffer);

        if (messageLength < 10)
        {
            throw new InvalidDataException("Invalid HSMS message length.");
        }

        byte[] messageBuffer = new byte[messageLength];

        bool messageRead = await ReadExactAsync(
            stream,
            messageBuffer,
            messageLength,
            cancellationToken);

        if (!messageRead)
        {
            return null;
        }

        return FromBytes(messageBuffer);
    }

    private static HsmsMessage FromBytes(byte[] message)
    {
        ushort sessionId = BinaryPrimitives.ReadUInt16BigEndian(
            message.AsSpan(0, 2));

        byte stream = message[2];
        byte function = message[3];
        byte pType = message[4];
        var sType = (HsmsSType)message[5];

        uint systemBytes = BinaryPrimitives.ReadUInt32BigEndian(
            message.AsSpan(6, 4));

        byte[] body = message.Length > 10
            ? message.AsSpan(10).ToArray()
            : Array.Empty<byte>();

        return new HsmsMessage
        {
            SessionId = sessionId,
            Stream = stream,
            Function = function,
            PType = pType,
            SType = sType,
            SystemBytes = systemBytes,
            Body = body
        };
    }

    private static async Task<bool> ReadExactAsync(
        NetworkStream stream,
        byte[] buffer,
        int length,
        CancellationToken cancellationToken)
    {
        int offset = 0;

        while (offset < length)
        {
            int read = await stream.ReadAsync(
                buffer.AsMemory(offset, length - offset),
                cancellationToken);

            if (read == 0)
            {
                return false;
            }

            offset += read;
        }

        return true;
    }
}