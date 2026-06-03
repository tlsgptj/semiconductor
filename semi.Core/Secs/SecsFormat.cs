namespace semi.Core.Secs;

// SECS 데이터 타입 정의
public enum SecsFormat : byte
{
    List = 0x00,
    Binary = 0x20,
    Boolean = 0x24,
    Ascii = 0x40,

    I8 = 0x60,
    I1 = 0x64,
    I2 = 0x68,
    I4 = 0x70,

    F8 = 0x80,
    F4 = 0x90,

    U8 = 0xA0,
    U1 = 0xA4,
    U2 = 0xA8,
    U4 = 0xB0
}