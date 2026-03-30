namespace Geotiff.Interfaces;

public interface IGetTagable
{
    public IEnumerable<Tag> GetAllRawTags();

    /// <summary>
    /// Lists all standard, extended and GDAL tags known to this library.
    /// Unrecognised tags will be excluded, use GetAllRawTags instead for these.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Tag> GetAllKnownTags();

    /// <summary>
    /// Returns null if the tag is not found in the ImageFileDirectory.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Tag? GetTag(int id);

    /// <summary>
    /// Returns null if the tag is not found in the ImageFileDirectory.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Tag? GetTag(string name);

    public bool HasTag(string name);
    public bool HasTag(int id);
}