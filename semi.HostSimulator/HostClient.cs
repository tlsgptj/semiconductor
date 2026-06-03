using System.Net.Sockets;
using semi.Core.Hsms;

namespace semi.HostSimulator;

// 클라이언트 역할을 하는 클래스
public class HostClient
{
    private readonly string _ip;
    private readonly int _port;
    private readonly ushort _sessionId;
    private readonly SystemByteGenerator _systemByteGenerator = new();

    // IP, 포트, 세션 ID를 받아서 초기화
    public HostClient(string ip, int port, ushort sessionId)
    {
        _ip = ip;
        _port = port;
        _sessionId = sessionId;
    }

    // TCP 클라이언트를 생성하고, 서버에 연결한 후, HSMS 메시지를 주고받는 메인 로직
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

        Console.WriteLine("[Host] Done.");
    }

    // 메시지를 보내고, 응답을 기다리는 공통 로직
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
    }
}