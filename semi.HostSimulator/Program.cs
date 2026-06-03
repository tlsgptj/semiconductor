using semi.HostSimulator;

// 세션 아이디를 0x0001로 설정하여 호스트 클라이언트를 생성하고, RunAsync 메서드를 호출하여 시뮬레이터를 실행
var host = new HostClient(
    ip: "127.0.0.1",
    port: 5000,
    sessionId: 0x0001);

await host.RunAsync();