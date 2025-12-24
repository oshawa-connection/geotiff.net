namespace Geotiff;

public class Tag(int rawId, string? tagName,GeotiffFieldDataType fieldDataType, object value, bool isList)
{
    public int RawId { get; } = rawId;
    public string? TagName { get; } = tagName;
    public GeotiffFieldDataType FieldDataType { get; } = fieldDataType;
    public object Value { get; set; } = value;
    public bool IsList { get; } = isList;
}