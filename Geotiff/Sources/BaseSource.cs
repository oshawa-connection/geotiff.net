using Geotiff.JavaScriptCompatibility;

namespace Geotiff;
/**
 * TODO: Make this IDisposable. Can hold onto file handles and HTTPClients
 */
public abstract class BaseSource
{
    public async Task<IEnumerable<byte[]>> Fetch(IEnumerable<Slice> slices)
    {
        var taskList = slices.Select(slice => this.FetchSlice(slice));
        var completedTasks = await Task.WhenAll(taskList);
        return completedTasks;
    }
    
    public async Task<IEnumerable<ArrayBuffer>> Fetch(IEnumerable<Slice> slices, CancellationToken cancellationToken)
    {
        var taskList = slices.Select(slice => this.FetchSlice(slice, cancellationToken));
        var completedTasks = await Task.WhenAll(taskList);
        return completedTasks.Select(d => new ArrayBuffer(d));
    }

    /**
     *
     * @param {Slice} slice
     * @returns {ArrayBuffer}
     */
    public virtual async Task<byte[]> FetchSlice(Slice slice) {
        throw new NotImplementedException("fetching of slice not possible, not implemented");
    }
    
    /**
     *
     * @param {Slice} slice
     * @returns {ArrayBuffer}
     */
    public virtual async Task<byte[]> FetchSlice(Slice slice, CancellationToken? cancellationToken) {
        throw new Exception("fetching of slice not possible, not implemented");
    }
    
    /**
     * Returns the filesize if already determined and null otherwise
     */
    public int? getFileSize() {
        return null;
    }
}