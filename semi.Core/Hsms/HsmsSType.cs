namespace semi.Core.Hsms
{
    // HSMS Control Type enum으로 정의
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
}