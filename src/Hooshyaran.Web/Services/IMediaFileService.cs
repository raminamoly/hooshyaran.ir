using Hooshyaran.Web.ViewModels;

namespace Hooshyaran.Web.Services;

public interface IMediaFileService
{
    long MaxFileSize { get; }

    IReadOnlyCollection<string> AllowedExtensions { get; }

    Task<MediaLibraryResult> ListAsync(string? searchTerm = null, string? fileType = null, string? usageFilter = null);

    Task<MediaFileItem> UploadAsync(IFormFile file, CancellationToken cancellationToken = default);

    Task SaveMetadataAsync(MediaMetadataInput input);

    Task<bool> DeleteAsync(string url);

    Task<int> DeleteUnusedAsync(CancellationToken cancellationToken = default);

    bool IsManagedMediaUrl(string? url);
}
