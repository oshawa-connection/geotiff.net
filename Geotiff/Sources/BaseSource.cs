using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

public abstract class BaseSource
{
    public async Task<IEnumerable<ArrayBuffer>> Fetch(IEnumerable<Slice> slices,
        CancellationToken? cancellationToken = null)
    {
        IEnumerable<Task<ArrayBuffer>>? taskList = slices.Select(slice => FetchSlice(slice, cancellationToken));
        ArrayBuffer[]? completedTasks = await Task.WhenAll(taskList);
        return completedTasks.Select(d => d);
    }

    /**
     *
     * @param {Slice} slice
     * @returns {ArrayBuffer}
     */
    public abstract Task<ArrayBuffer> FetchSlice(Slice slice, CancellationToken? cancellationToken = null);

    /**
     * Returns the filesize if already determined and null otherwise
     */
    public int? getFileSize()
    {
        return null;
    }
}