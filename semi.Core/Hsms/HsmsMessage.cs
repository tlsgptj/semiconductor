namespace semi.Core.Hsms;

public class HsmsMessage
{
    public ushort SessionId { get; set; }

    public byte Stream { get; set; }

    public byte Function { get; set; }

    public byte PType { get; set; }

    public HsmsSType SType { get; set; }

    public uint SystemBytes { get; set; }

    public byte[] Body { get; set; } = Array.Empty<byte>();

    public bool IsDataMessage => SType == HsmsSType.DataMessage;

    public override string ToString()
    {
        string bodyHex = Body.Length > 0
            ? Convert.ToHexString(Body)
            : "-";

        if (IsDataMessage)
        {
            return $"DATA S{Stream}F{Function}, SessionId={SessionId}, SystemBytes={SystemBytes}, Body={Body.Length} bytes, Hex={bodyHex}";
        }

        return $"CONTROL {SType}, SessionId={SessionId}, SystemBytes={SystemBytes}";
    }
}