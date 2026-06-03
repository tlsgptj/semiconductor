# SECS/GEM HSMS Practice Project

C# 콘솔 앱으로 SECS/GEM 통신의 기본 구조를 연습하기 위한 프로젝트입니다.

이 프로젝트는 실제 반도체 장비와 MES/Host 간 통신에서 사용되는 **HSMS**, **SECS-II**, **GEM Event Report**의 핵심 개념을 단순화하여 구현합니다.

현재 구현된 기능은 다음과 같습니다.

* HSMS TCP/IP 기반 Host ↔ Equipment 연결
* Select.req / Select.rsp 처리
* S1F1 / S1F2 메시지 송수신
* SECS-II Body 인코딩
* SECS-II Body 디코딩
* S6F11 Collection Event Report 송신
* S6F12 Event Report Acknowledge 응답

---

## 1. 프로젝트 목적

이 프로젝트의 목적은 SECS/GEM을 완성형으로 구현하는 것이 아니라, 반도체 장비 통신의 기본 흐름을 직접 코드로 이해하는 것입니다.

실제 장비 통신에서는 SEMI 표준에 정의된 다양한 메시지, 타이머, 상태 모델, 예외 처리, Online/Offline 제어 등이 필요합니다.
이 프로젝트에서는 학습을 위해 가장 핵심적인 흐름만 구현했습니다.

구현 흐름은 다음과 같습니다.

```text
HostSimulator
    ↓ TCP Connect

EquipmentSimulator
    ↓

Host → Equipment : Select.req
Equipment → Host : Select.rsp

Host → Equipment : S1F1
Equipment → Host : S1F2

Equipment → Host : S6F11
Host → Equipment : S6F12
```

---

## 2. SECS/GEM, HSMS, SECS-II 개념

### 2.1 HSMS란?

HSMS는 **High-Speed SECS Message Services**의 약자입니다.

기존 SECS-I이 시리얼 통신 기반이었다면, HSMS는 TCP/IP 기반으로 SECS 메시지를 주고받는 방식입니다.

즉, HSMS는 메시지의 의미 자체를 정의하는 것이 아니라, SECS-II 메시지를 TCP/IP 위에서 어떻게 주고받을지 정의하는 통신 계층입니다.

이 프로젝트에서는 `TcpListener`, `TcpClient`, `NetworkStream`을 이용해 HSMS 통신 구조를 구현했습니다.

---

### 2.2 SECS-II란?

SECS-II는 Host와 Equipment가 주고받는 메시지의 구조를 정의합니다.

SECS-II 메시지는 보통 다음과 같은 형식으로 표현됩니다.

```text
S1F1
S1F2
S6F11
S6F12
S2F41
...
```

여기서 `S`는 Stream, `F`는 Function을 의미합니다.

예를 들어:

```text
S1F1  = Are You There?
S1F2  = On Line Data
S6F11 = Collection Event Report
S6F12 = Collection Event Report Acknowledge
```

이 프로젝트에서는 다음 메시지를 구현했습니다.

| 메시지        | 방향               | 의미            |
| ---------- | ---------------- | ------------- |
| Select.req | Host → Equipment | HSMS 연결 선택 요청 |
| Select.rsp | Equipment → Host | HSMS 연결 선택 응답 |
| S1F1       | Host → Equipment | 장비 응답 확인      |
| S1F2       | Equipment → Host | 장비 모델명, 버전 응답 |
| S6F11      | Equipment → Host | 장비 이벤트 보고     |
| S6F12      | Host → Equipment | 이벤트 수신 확인     |

---

### 2.3 GEM이란?

GEM은 **Generic Equipment Model**의 약자입니다.

GEM은 장비가 Host/MES와 통신할 때 어떤 상태, 이벤트, 알람, 변수, Remote Command 구조를 가져야 하는지 정의하는 장비 표준 모델입니다.

예를 들어 실제 GEM 장비에서는 다음과 같은 기능이 필요합니다.

```text
Communication State
Control State
Processing State
Collection Event
Alarm Report
Status Variable
Equipment Constant
Remote Command
```

이 프로젝트에서는 GEM 전체를 구현하지 않고, GEM의 핵심 흐름 중 하나인 **Collection Event Report**, 즉 `S6F11/S6F12`만 간단히 구현했습니다.

---

## 3. 프로젝트 구조

```text
semiconductor/
│
├─ semi.Core/
│  ├─ Hsms/
│  │  ├─ HsmsMessage.cs
│  │  ├─ HsmsMessageReader.cs
│  │  ├─ HsmsMessageWriter.cs
│  │  ├─ HsmsSType.cs
│  │  └─ SystemByteGenerator.cs
│  │
│  └─ Secs/
│     ├─ SecsFormat.cs
│     ├─ SecsItem.cs
│     ├─ SecsEncoder.cs
│     └─ SecsDecoder.cs
│
├─ semi.EquipmentSimulator/
│  ├─ Program.cs
│  └─ EquipmentServer.cs
│
└─ semi.HostSimulator/
   ├─ Program.cs
   └─ HostClient.cs
```

---

## 4. 프로젝트별 역할

### 4.1 semi.Core

공통 라이브러리 프로젝트입니다.

Host와 Equipment가 공통으로 사용하는 HSMS 메시지 모델, 인코더, 디코더가 들어 있습니다.

주요 역할:

```text
HSMS 메시지 모델 정의
HSMS 패킷 인코딩
HSMS 패킷 디코딩
SECS-II Item 인코딩
SECS-II Item 디코딩
SystemBytes 생성
```

---

### 4.2 semi.EquipmentSimulator

장비 역할을 하는 콘솔 앱입니다.

TCP 서버로 동작하며 Host의 접속을 기다립니다.

주요 흐름:

```text
5000번 포트에서 대기
Host 접속 수락
Select.req 수신
Select.rsp 응답
S1F1 수신
S1F2 응답
S6F11 이벤트 보고
S6F12 ACK 수신
```

---

### 4.3 semi.HostSimulator

Host/MES 역할을 하는 콘솔 앱입니다.

TCP 클라이언트로 동작하며 Equipment에 접속합니다.

주요 흐름:

```text
127.0.0.1:5000 접속
Select.req 전송
Select.rsp 수신
S1F1 전송
S1F2 수신
S6F11 이벤트 수신
S6F12 ACK 전송
```

---

## 5. HSMS 메시지 구조

HSMS 메시지는 다음과 같은 구조를 가집니다.

```text
4 bytes  : Message Length
10 bytes : HSMS Header
N bytes  : SECS-II Body
```

전체 구조를 그림으로 표현하면 다음과 같습니다.

```text
+------------------+
| 4-byte Length    |
+------------------+
| 10-byte Header   |
+------------------+
| SECS-II Body     |
+------------------+
```

---

### 5.1 4-byte Length

맨 앞의 4바이트는 이후에 따라오는 메시지 길이를 의미합니다.

즉, 10바이트 Header와 Body 길이의 합입니다.

```text
Message Length = 10-byte Header + Body Length
```

예를 들어 Body가 19바이트라면 전체 Length는 다음과 같습니다.

```text
10 + 19 = 29
```

---

### 5.2 10-byte HSMS Header

이 프로젝트에서 사용하는 HSMS Header 구조는 다음과 같습니다.

```text
Byte 0~1 : Session ID
Byte 2   : Stream
Byte 3   : Function
Byte 4   : PType
Byte 5   : SType
Byte 6~9 : SystemBytes
```

코드에서는 `HsmsMessage` 클래스로 표현합니다.

```csharp
public class HsmsMessage
{
    public ushort SessionId { get; set; }
    public byte Stream { get; set; }
    public byte Function { get; set; }
    public byte PType { get; set; }
    public HsmsSType SType { get; set; }
    public uint SystemBytes { get; set; }
    public byte[] Body { get; set; } = Array.Empty<byte>();
}
```

---

## 6. HsmsSType

HSMS에는 일반 데이터 메시지와 제어 메시지가 있습니다.

이 프로젝트에서는 `HsmsSType` enum으로 구분합니다.

```csharp
public enum HsmsSType : byte
{
    DataMessage = 0,
    SelectRequest = 1,
    SelectResponse = 2,
    DeselectRequest = 3,
    DeselectResponse = 4,
    LinktestRequest = 5,
    LinktestResponse = 6,
    RejectRequest = 7,
    SeparateRequest = 9
}
```

주요 타입은 다음과 같습니다.

| SType | 이름               | 의미                               |
| ----: | ---------------- | -------------------------------- |
|     0 | DataMessage      | S1F1, S1F2, S6F11 같은 SECS-II 메시지 |
|     1 | SelectRequest    | Host가 HSMS 연결 선택 요청              |
|     2 | SelectResponse   | Equipment가 Select 요청에 응답         |
|     5 | LinktestRequest  | 연결 확인 요청                         |
|     6 | LinktestResponse | 연결 확인 응답                         |
|     9 | SeparateRequest  | 연결 종료 요청                         |

---

## 7. SystemBytes란?

`SystemBytes`는 요청과 응답을 매칭하기 위한 식별자입니다.

예를 들어 Host가 `S1F1`을 보낼 때 `SystemBytes=2`로 보냈다면, Equipment는 `S1F2`를 응답할 때 같은 `SystemBytes=2`를 돌려줘야 합니다.

```text
Host → Equipment : S1F1, SystemBytes=2
Equipment → Host : S1F2, SystemBytes=2
```

이렇게 해야 Host가 어떤 요청에 대한 응답인지 구분할 수 있습니다.

이 프로젝트에서는 `SystemByteGenerator`가 요청을 보낼 때마다 값을 증가시킵니다.

```csharp
public class SystemByteGenerator
{
    private uint _current = 0;

    public uint Next()
    {
        return Interlocked.Increment(ref _current);
    }
}
```

---

## 8. SECS-II Body 구조

SECS-II Body는 단순 문자열이 아니라, 타입과 길이를 가진 Item 구조입니다.

대표적인 타입은 다음과 같습니다.

| 타입   | 의미                      |
| ---- | ----------------------- |
| L    | List                    |
| A    | ASCII                   |
| U4   | Unsigned 4-byte Integer |
| B    | Binary                  |
| BOOL | Boolean                 |
| I4   | Signed 4-byte Integer   |
| F4   | 4-byte Float            |

현재 프로젝트에서는 학습용으로 다음 타입만 구현했습니다.

```text
L
A
U4
```

---

## 9. S1F2 Body 예시

이 프로젝트에서 Equipment는 `S1F1`을 받으면 `S1F2`로 장비 모델명과 소프트웨어 버전을 응답합니다.

SML 형태로 표현하면 다음과 같습니다.

```text
S1F2
<L[2]
  <A "SEMI_EQP">
  <A "1.0.0">
>
```

의미는 다음과 같습니다.

| 항목      | 값        | 의미       |
| ------- | -------- | -------- |
| MDLN    | SEMI_EQP | 장비 모델명   |
| SOFTREV | 1.0.0    | 소프트웨어 버전 |

코드에서는 다음과 같이 Body를 생성합니다.

```csharp
var s1f2Body = SecsItem.L(
    SecsItem.A("SEMI_EQP"),
    SecsItem.A("1.0.0")
);

Body = SecsEncoder.Encode(s1f2Body);
```

---

## 10. SECS-II 인코딩 원리

SECS-II Item은 다음 구조로 인코딩됩니다.

```text
Format + LengthByteCount
Length
Data
```

예를 들어 ASCII `"SEMI_EQP"`는 다음과 같이 표현됩니다.

```text
A 타입
길이 8
데이터 "SEMI_EQP"
```

실제 Hex는 다음과 비슷합니다.

```text
41 08 53 45 4D 49 5F 45 51 50
```

여기서:

```text
41 = ASCII 타입 + Length Byte Count 1
08 = 데이터 길이 8
53 45 4D 49 5F 45 51 50 = "SEMI_EQP"
```

---

## 11. List 인코딩 원리

List는 일반 데이터 타입과 약간 다릅니다.

ASCII나 U4의 Length는 데이터 바이트 길이입니다.

하지만 List의 Length는 바이트 길이가 아니라 **내부 Item 개수**입니다.

예를 들어:

```text
<L[2]
  <A "SEMI_EQP">
  <A "1.0.0">
>
```

이 경우 List의 길이는 2입니다.

```text
01 02
```

여기서:

```text
01 = List 타입 + Length Byte Count 1
02 = 내부 Item 개수 2
```

---

## 12. S6F11 Collection Event Report

`S6F11`은 장비가 Host에게 이벤트를 보고할 때 사용하는 메시지입니다.

예를 들어 장비가 RUN 상태로 전환되거나, 공정이 완료되거나, 특정 센서값이 갱신되었을 때 사용될 수 있습니다.

이 프로젝트에서는 Equipment가 `S1F2` 응답을 보낸 뒤, 1초 후에 `S6F11` 이벤트를 전송합니다.

```text
Equipment → Host : S6F11
Host → Equipment : S6F12
```

---

## 13. S6F11 Body 예시

현재 프로젝트에서 사용하는 S6F11 Body는 다음과 같습니다.

```text
S6F11
<L[3]
  <U4 1>
  <U4 100>
  <L[1]
    <L[2]
      <U4 200>
      <L[2]
        <A "RUN">
        <U4 25>
      >
    >
  >
>
```

각 항목의 의미는 다음과 같습니다.

| 항목     |   값 | 의미                  |
| ------ | --: | ------------------- |
| DATAID |   1 | 데이터 식별자             |
| CEID   | 100 | Collection Event ID |
| RPTID  | 200 | Report ID           |
| 상태값    | RUN | 장비 상태               |
| 예시 값   |  25 | 센서값 또는 카운트값         |

코드에서는 다음과 같이 생성합니다.

```csharp
var body = SecsItem.L(
    SecsItem.U4(1),
    SecsItem.U4(100),
    SecsItem.L(
        SecsItem.L(
            SecsItem.U4(200),
            SecsItem.L(
                SecsItem.A("RUN"),
                SecsItem.U4(25)
            )
        )
    )
);
```

---

## 14. S6F12 Acknowledge

Host는 `S6F11`을 수신하면 Equipment에게 `S6F12`를 응답합니다.

이 프로젝트에서는 단순화를 위해 Body를 비워서 보냅니다.

```text
Host → Equipment : S6F12
```

코드에서는 다음과 같이 응답합니다.

```csharp
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
```

중요한 점은 `S6F12`의 `SystemBytes`는 수신한 `S6F11`의 `SystemBytes`와 같아야 한다는 것입니다.

```text
Equipment → Host : S6F11, SystemBytes=1001
Host → Equipment : S6F12, SystemBytes=1001
```

---

## 15. 실행 방법

### 15.1 빌드

루트 폴더에서 다음 명령어를 실행합니다.

```powershell
dotnet build
```

---

### 15.2 Equipment 실행

터미널 1에서 EquipmentSimulator를 먼저 실행합니다.

```powershell
dotnet run --project .\semi.EquipmentSimulator\semi.EquipmentSimulator.csproj
```

정상 실행 시 다음과 같이 출력됩니다.

```text
[Equipment] HSMS server started. Port=5000
```

---

### 15.3 Host 실행

터미널 2에서 HostSimulator를 실행합니다.

```powershell
dotnet run --project .\semi.HostSimulator\semi.HostSimulator.csproj
```

---

## 16. 실행 결과 예시

Host 쪽 출력 예시:

```text
[Host] Connecting to 127.0.0.1:5000...
[Host] Connected.

[Host] TX: CONTROL SelectRequest, SessionId=1, SystemBytes=1
[Host] RX: CONTROL SelectResponse, SessionId=1, SystemBytes=1

[Host] TX: DATA S1F1, SessionId=1, SystemBytes=2, Body=0 bytes
[Host] RX: DATA S1F2, SessionId=1, SystemBytes=2, Body=19 bytes

[Host] Decoded Body:
<L[2]
  <A "SEMI_EQP">
  <A "1.0.0">
>

[Host] Waiting for equipment event...

[Host] RX EVENT: DATA S6F11, SessionId=1, SystemBytes=1001
[Host] Decoded Event Body:
<L[3]
  <U4 1>
  <U4 100>
  <L[1]
    <L[2]
      <U4 200>
      <L[2]
        <A "RUN">
        <U4 25>
      >
    >
  >
>

[Host] TX ACK: DATA S6F12, SessionId=1, SystemBytes=1001
[Host] Done.
```

Equipment 쪽 출력 예시:

```text
[Equipment] HSMS server started. Port=5000
[Equipment] Host connected.

[Equipment] RX: CONTROL SelectRequest, SessionId=1, SystemBytes=1
[Equipment] TX: CONTROL SelectResponse, SessionId=1, SystemBytes=1

[Equipment] RX: DATA S1F1, SessionId=1, SystemBytes=2
[Equipment] TX: DATA S1F2, SessionId=1, SystemBytes=2

[Equipment] TX EVENT: DATA S6F11, SessionId=1, SystemBytes=1001

[Equipment] RX: DATA S6F12, SessionId=1, SystemBytes=1001
[Equipment] S6F12 received. Event report acknowledged by Host.
```

---

## 17. 현재 구현의 한계

이 프로젝트는 학습용이므로 실제 SECS/GEM 장비 통신과 비교했을 때 많은 기능이 생략되어 있습니다.

현재 생략된 기능:

```text
HSMS 상태머신
T3/T5/T6/T7/T8 타이머
Linktest.req / Linktest.rsp 주기 처리
Reject.req 처리
Separate.req 처리
Wait Bit 처리
정식 GEM Communication State
Online/Offline Control State
Alarm Report S5F1/S5F2
Remote Command S2F41/S2F42
Status Variable 관리
Equipment Constant 관리
Report 정의/링크/활성화 절차
다중 Host/다중 Equipment 처리
로그 파일 저장
재접속 처리
```

따라서 이 프로젝트는 실제 양산 장비에 바로 적용하기 위한 코드가 아니라, SECS/GEM 통신 구조를 이해하기 위한 연습용 구현입니다.

---

## 18. 다음 확장 방향

이 프로젝트를 더 발전시키려면 다음 순서로 구현하면 좋습니다.

### 18.1 Linktest 구현

HSMS 연결이 살아 있는지 확인하기 위해 `Linktest.req`와 `Linktest.rsp`를 구현합니다.

```text
Host → Equipment : Linktest.req
Equipment → Host : Linktest.rsp
```

---

### 18.2 S5F1/S5F2 Alarm Report 구현

장비 알람을 Host에게 보고하는 기능입니다.

```text
Equipment → Host : S5F1 Alarm Report
Host → Equipment : S5F2 Alarm Report Acknowledge
```

---

### 18.3 S2F41/S2F42 Remote Command 구현

Host가 Equipment에게 명령을 내리는 구조입니다.

예를 들어:

```text
START
STOP
PAUSE
RESUME
```

흐름은 다음과 같습니다.

```text
Host → Equipment : S2F41 Remote Command
Equipment → Host : S2F42 Command Acknowledge
```

---

### 18.4 Fake PLC 연동

실제 PLC 대신 Fake PLC 클래스를 만들어 장비 상태 변화를 시뮬레이션할 수 있습니다.

예시:

```text
FakePlc.RunSignal = true
FakePlc.AlarmSignal = true
FakePlc.Temperature = 25
```

이 값을 Equipment가 읽어서 다음 메시지로 변환할 수 있습니다.

```text
RUN 상태 변경 → S6F11 Event Report
ALARM 발생 → S5F1 Alarm Report
온도값 변경 → Status Variable Report
```

---

### 18.5 Worker Service 전환

현재는 콘솔 앱으로 구현되어 있습니다.

실무에서는 Equipment Interface 프로그램을 Windows Service 또는 Worker Service 형태로 실행하는 경우가 많습니다.

그 이유는 장비 PC가 부팅될 때 자동으로 통신 프로그램이 실행되어야 하기 때문입니다.

```text
PC 부팅
→ Equipment Interface Service 자동 실행
→ HSMS 서버 시작
→ Host/MES 연결 대기
→ PLC 상태 수집
→ Event/Alarm 보고
```

---

## 19. 학습 포인트

이 프로젝트를 통해 확인할 수 있는 핵심 학습 포인트는 다음과 같습니다.

```text
TCP/IP 기반 장비 통신 구조
HSMS Header 구조
SECS-II Item 인코딩 방식
SECS-II Item 디코딩 방식
SystemBytes를 이용한 요청/응답 매칭
Host와 Equipment 역할 분리
장비 이벤트를 S6F11로 보고하는 흐름
Host가 S6F12로 이벤트 수신 확인하는 흐름
```

---

## 20. 전체 흐름 요약

```text
1. EquipmentSimulator 실행
2. HostSimulator 실행
3. Host가 Equipment에 TCP 접속
4. Host가 Select.req 전송
5. Equipment가 Select.rsp 응답
6. Host가 S1F1 전송
7. Equipment가 S1F2 응답
8. Host가 S1F2 Body 디코딩
9. Equipment가 S6F11 이벤트 보고
10. Host가 S6F11 Body 디코딩
11. Host가 S6F12 ACK 응답
12. Equipment가 S6F12 수신 확인
```

---

## 21. 정리

이 프로젝트는 C# 콘솔 앱으로 SECS/GEM의 기본 통신 흐름을 직접 구현한 연습용 프로젝트입니다.

단순 TCP 통신을 넘어서 HSMS Header, SECS-II Body, S1F1/S1F2, S6F11/S6F12 구조를 직접 구현함으로써 반도체 장비와 Host/MES 간 표준 통신의 기본 원리를 이해할 수 있습니다.

실무 수준으로 확장하려면 HSMS 상태머신, 타이머, Linktest, Alarm, Remote Command, GEM 상태 모델, PLC 연동, 로그 저장, 재접속 처리 등을 추가로 구현해야 합니다.
