using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

public abstract class BaseSource
{
    public virtual async Task<IEnumerable<byte[]>> FetchAsync(IEnumerable<Slice> slices,
        CancellationToken? cancellationToken = null)
    {
        IEnumerable<Task<byte[]>>? taskList = slices.Select(slice => FetchSliceAsync(slice, cancellationToken));
        byte[][]? completedTasks = await Task.WhenAll(taskList);
        return completedTasks.Select(d => d);
    }

    /**
     *
     * @param {Slice} slice
     * @returns {ArrayBuffer}
     */
    public abstract Task<byte[]> FetchSliceAsync(Slice slice, CancellationToken? cancellationToken = null);
    
    public abstract byte[] FetchSlice(Slice slice);
}