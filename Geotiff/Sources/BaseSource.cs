using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

public abstract class BaseSource
{
    public virtual async Task<IEnumerable<ArrayBuffer>> FetchAsync(IEnumerable<Slice> slices,
        CancellationToken? cancellationToken = null)
    {
        IEnumerable<Task<ArrayBuffer>>? taskList = slices.Select(slice => FetchSliceAsync(slice, cancellationToken));
        ArrayBuffer[]? completedTasks = await Task.WhenAll(taskList);
        return completedTasks.Select(d => d);
    }

    /**
     *
     * @param {Slice} slice
     * @returns {ArrayBuffer}
     */
    public abstract Task<ArrayBuffer> FetchSliceAsync(Slice slice, CancellationToken? cancellationToken = null);
    
    public abstract ArrayBuffer FetchSlice(Slice slice);
}