using System.Net;
using System.Net.Sockets;
using semi.Core.Hsms;
using semi.Core.Secs;

namespace semi.EquipmentSimulator;

public class EquipmentServer
{
    private readonly int _port;

    // HSMS 서버 역할을 하는 클래스
    public EquipmentServer(int port)
    {
        _port = port;
    }

    // TCP 리스너를 열고, 클라이언트 연결을 기다리며, 연결이 오면 메시지를 처리하는 루프
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();

        Console.WriteLine($"[Equipment] HSMS server started. Port={_port}");

        while (!cancellationToken.IsCancellationRequested)
        {
            TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);

            Console.WriteLine("[Equipment] Host connected.");

            _ = Task.Run(
                () => HandleClientAsync(client, cancellationToken),
                cancellationToken);
        }
    }

    // 클라이언트와의 통신을 처리하는 메서드
    private async Task HandleClientAsync(
        TcpClient client,
        CancellationToken cancellationToken)
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    HsmsMessage? request = await HsmsMessageReader.ReadAsync(
                        stream,
                        cancellationToken);

                    if (request == null)
                    {
                        Console.WriteLine("[Equipment] Host disconnected.");
                        break;
                    }

                    Console.WriteLine($"[Equipment] RX: {request}");

                    HsmsMessage? response = HandleMessage(request);

                    if (response != null)
                    {
                        await HsmsMessageWriter.WriteAsync(
                            stream,
                            response,
                            cancellationToken);

                        Console.WriteLine($"[Equipment] TX: {response}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Equipment] Error: {ex.Message}");
            }
        }
    }

    // 간단한 메시지 핸들러. 실제 장비에서는 더 복잡한 로직이 들어갈 수 있음
    private HsmsMessage? HandleMessage(HsmsMessage request)
    {
        // 1. Select.req -> Select.rsp
        if (request.SType == HsmsSType.SelectRequest)
        {
            return new HsmsMessage
            {
                SessionId = request.SessionId,
                Stream = 0,
                Function = 0,
                PType = 0,
                SType = HsmsSType.SelectResponse,
                SystemBytes = request.SystemBytes,
                Body = Array.Empty<byte>()
            };
        }

        // 2. S1F1 -> S1F2
        if (request.SType == HsmsSType.DataMessage &&
            request.Stream == 1 &&
            request.Function == 1)
        {
            var s1f2Body = SecsItem.L(
                SecsItem.A("SEMI_EQP"),
                SecsItem.A("1.0.0")
            );

            return new HsmsMessage
            {
                SessionId = request.SessionId,
                Stream = 1,
                Function = 2,
                PType = 0,
                SType = HsmsSType.DataMessage,
                SystemBytes = request.SystemBytes,
                Body = SecsEncoder.Encode(s1f2Body)
            };
        }

        Console.WriteLine($"[Equipment] Unsupported message. SType={request.SType}, S={request.Stream}, F={request.Function}");

        return null;
    }
}