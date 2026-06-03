using System.Text;

namespace semi.Core.Secs;

// SECS-II 아이템을 표현하는 클래스
public class SecsItem
{
    public SecsFormat Format { get; }

    public string? AsciiValue { get; }

    public uint[]? U4Values { get; }

    public List<SecsItem>? Items { get; }

    private SecsItem(
        SecsFormat format,
        string? asciiValue = null,
        uint[]? u4Values = null,
        List<SecsItem>? items = null)
    {
        Format = format;
        AsciiValue = asciiValue;
        U4Values = u4Values;
        Items = items;
    }

    public static SecsItem A(string value)
    {
        return new SecsItem(SecsFormat.Ascii, asciiValue: value);
    }

    public static SecsItem U4(params uint[] values)
    {
        return new SecsItem(SecsFormat.U4, u4Values: values);
    }

    public static SecsItem L(params SecsItem[] items)
    {
        return new SecsItem(SecsFormat.List, items: items.ToList());
    }

    public string ToSmlString()
    {
        var sb = new StringBuilder();
        WriteSml(sb, 0);
        return sb.ToString();
    }

    private void WriteSml(StringBuilder sb, int depth)
    {
        string indent = new string(' ', depth * 2);

        if (Format == SecsFormat.List)
        {
            int count = Items?.Count ?? 0;

            sb.AppendLine($"{indent}<L[{count}]");

            if (Items != null)
            {
                foreach (var item in Items)
                {
                    item.WriteSml(sb, depth + 1);
                }
            }

            sb.AppendLine($"{indent}>");
            return;
        }

        if (Format == SecsFormat.Ascii)
        {
            sb.AppendLine($"{indent}<A \"{AsciiValue}\">");
            return;
        }

        if (Format == SecsFormat.U4)
        {
            string values = U4Values == null
                ? ""
                : string.Join(" ", U4Values);

            sb.AppendLine($"{indent}<U4 {values}>");
            return;
        }

        sb.AppendLine($"{indent}<{Format}>");
    }

    public override string ToString()
    {
        return ToSmlString();
    }
}