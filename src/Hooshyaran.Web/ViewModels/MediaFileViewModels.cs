namespace Hooshyaran.Web.ViewModels;

public record MediaFileItem(
    string Name,
    string Url,
    string Extension,
    string ContentType,
    string Size,
    long SizeInBytes,
    DateTimeOffset LastModified,
    bool IsImage,
    string AltText,
    string Title,
    string Description,
    string SeoDescription,
    string Hash,
    bool IsUsed,
    int UsageCount,
    IReadOnlyList<string> UsedIn,
    bool HasDuplicate,
    int DuplicateCount);

public record MediaLibraryResult(
    IReadOnlyList<MediaFileItem> Files,
    string SearchTerm,
    string FileType,
    string UsageFilter,
    int TotalCount,
    int UsedCount,
    int UnusedCount,
    int DuplicateCount);

public class MediaMetadataInput
{
    public string Url { get; set; } = string.Empty;

    public string AltText { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SeoDescription { get; set; } = string.Empty;
}
