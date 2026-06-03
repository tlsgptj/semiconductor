using System.Net.Sockets;
using semi.Core.Hsms;
using semi.Core.Secs;

namespace semi.HostSimulator;

// 클라이언트 역할을 하는 클래스
public class HostClient
{
    private readonly string _ip;
    private readonly int _port;
    private readonly ushort _sessionId;
    private readonly SystemByteGenerator _systemByteGenerator = new();

    public HostClient(string ip, int port, ushort sessionId)
    {
        _ip = ip;
        _port = port;
        _sessionId = sessionId;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var client = new TcpClient();

        Console.WriteLine($"[Host] Connecting to {_ip}:{_port}...");

        await client.ConnectAsync(_ip, _port, cancellationToken);

        Console.WriteLine("[Host] Connected.");

        using NetworkStream stream = client.GetStream();

        // 1. Select.req 전송
        var selectReq = new HsmsMessage
        {
            SessionId = _sessionId,
            Stream = 0,
            Function = 0,
            PType = 0,
            SType = HsmsSType.SelectRequest,
            SystemBytes = _systemByteGenerator.Next(),
            Body = Array.Empty<byte>()
        };

        await SendAndReceiveAsync(stream, selectReq, cancellationToken);

        // 2. S1F1 전송
        var s1f1 = new HsmsMessage
        {
            SessionId = _sessionId,
            Stream = 1,
            Function = 1,
            PType = 0,
            SType = HsmsSType.DataMessage,
            SystemBytes = _systemByteGenerator.Next(),
            Body = Array.Empty<byte>()
        };

        await SendAndReceiveAsync(stream, s1f1, cancellationToken);

        // 3. Equipment가 보내는 S6F11 이벤트 대기
        Console.WriteLine("[Host] Waiting for equipment event...");

        HsmsMessage? eventMessage = await HsmsMessageReader.ReadAsync(
            stream,
            cancellationToken);

        if (eventMessage != null)
        {
            Console.WriteLine($"[Host] RX EVENT: {eventMessage}");

            if (eventMessage.SType == HsmsSType.DataMessage &&
                eventMessage.Body.Length > 0)
            {
                try
                {
                    SecsItem item = SecsDecoder.Decode(eventMessage.Body);

                    Console.WriteLine("[Host] Decoded Event Body:");
                    Console.WriteLine(item.ToSmlString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Host] Event body decode failed: {ex.Message}");
                }
            }

            // S6F11을 받았으면 S6F12 ACK 응답
            if (eventMessage.Stream == 6 &&
                eventMessage.Function == 11)
            {
                var s6f12 = new HsmsMessage
                {
                    SessionId = eventMessage.SessionId,
                    Stream = 6,
                    Function = 12,
                    PType = 0,
                    SType = HsmsSType.DataMessage,
                    SystemBytes = eventMessage.SystemBytes,
                    Body = Array.Empty<byte>()
                };

                await HsmsMessageWriter.WriteAsync(
                    stream,
                    s6f12,
                    cancellationToken);

                Console.WriteLine($"[Host] TX ACK: {s6f12}");
            }
        }

        Console.WriteLine("[Host] Done.");
    }

    private static async Task SendAndReceiveAsync(
        NetworkStream stream,
        HsmsMessage request,
        CancellationToken cancellationToken)
    {
        await HsmsMessageWriter.WriteAsync(
            stream,
            request,
            cancellationToken);

        Console.WriteLine($"[Host] TX: {request}");

        HsmsMessage? response = await HsmsMessageReader.ReadAsync(
            stream,
            cancellationToken);

        if (response == null)
        {
            Console.WriteLine("[Host] No response. Connection closed.");
            return;
        }

        Console.WriteLine($"[Host] RX: {response}");

        if (response.SType == HsmsSType.DataMessage &&
            response.Body.Length > 0)
        {
            try
            {
                SecsItem item = SecsDecoder.Decode(response.Body);

                Console.WriteLine("[Host] Decoded Body:");
                Console.WriteLine(item.ToSmlString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Host] Body decode failed: {ex.Message}");
            }
        }
    }
}