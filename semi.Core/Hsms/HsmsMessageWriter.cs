using System.Buffers.Binary;
using System.Net.Sockets;

namespace semi.Core.Hsms;

/// <summary>
/// Hsms Message 객체를 실제 TCP로 보낼 수 있는 byte[]로 바꿔주는 역할
/// 4 byte Length + 10 byte HSMS header + Body
/// </summary>
public static class HsmsMessageWriter
{
    public static async Task WriteAsync(
        NetworkStream stream,
        HsmsMessage message,
        CancellationToken cancellationToken = default)
    {
        byte[] packet = ToBytes(message);

        await stream.WriteAsync(packet, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    public static byte[] ToBytes(HsmsMessage message)
    {
        int messageLength = 10 + message.Body.Length;

        byte[] packet = new byte[4 + messageLength];

        // 4-byte length
        BinaryPrimitives.WriteInt32BigEndian(
            packet.AsSpan(0, 4),
            messageLength);

        // 10-byte HSMS header
        BinaryPrimitives.WriteUInt16BigEndian(
            packet.AsSpan(4, 2),
            message.SessionId);

        packet[6] = message.Stream;
        packet[7] = message.Function;
        packet[8] = message.PType;
        packet[9] = (byte)message.SType;

        BinaryPrimitives.WriteUInt32BigEndian(
            packet.AsSpan(10, 4),
            message.SystemBytes);

        if (message.Body.Length > 0)
        {
            Buffer.BlockCopy(
                message.Body,
                0,
                packet,
                14,
                message.Body.Length);
        }

        return packet;
    }
}