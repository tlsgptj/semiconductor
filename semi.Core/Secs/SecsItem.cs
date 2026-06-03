namespace semi.Core.Secs;

// SECS 아이템을 표현하는 클래스
public class SecsItem
{
    public SecsFormat Format { get; }

    public string? AsciiValue { get; }

    public List<SecsItem>? Items { get; }

    private SecsItem(SecsFormat format, string? asciiValue = null, List<SecsItem>? items = null)
    {
        Format = format;
        AsciiValue = asciiValue;
        Items = items;
    }

    public static SecsItem A(string value)
    {
        return new SecsItem(SecsFormat.Ascii, asciiValue: value);
    }

    public static SecsItem L(params SecsItem[] items)
    {
        return new SecsItem(SecsFormat.List, items: items.ToList());
    }
}