namespace Lyt.Jigsaw.Model.PictureObjects;

public class PictureMetadata
{
    private static readonly List<string> SupportedPicturesFileExtensions =
    [
        "jpg",
        "jpeg",
        "JPG",
        "JPEG",
        "png",
        "PNG"
    ];

    public PictureMetadata()
    {
        this.Date = DateTime.Now.Date;
    }

    public string? UrlFileExtension()
    {
        if (string.IsNullOrWhiteSpace(this.Url))
        {
            return string.Empty;
        }

        string[] tokens = this.Url.Split(['.'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string maybeExtension = tokens[^1];
        if (SupportedPicturesFileExtensions.Contains(maybeExtension) ) 
        {
            return maybeExtension;
        }

        foreach (string extension in SupportedPicturesFileExtensions)
        {
            if (maybeExtension.Contains(extension, StringComparison.InvariantCultureIgnoreCase))
            {
                return extension;
            }
        }

        return "jpg"; 
    }

    public string TodayImageFilePath()
    {
        string? maybeExtension = this.UrlFileExtension();
        string extension = string.IsNullOrWhiteSpace(maybeExtension) ? "jpg" : maybeExtension;
        return string.Format("{0}_Today.{1}", "TODO_Name", extension);
    }

    [JsonRequired]
    public DateTime Date { get; set; }

    [JsonRequired]
    public string? Url { get; set; } = string.Empty;

    [JsonRequired]
    public string? Title { get; set; } = string.Empty;

    [JsonRequired]
    public string? Description { get; set; } = string.Empty;

    [JsonRequired]
    public string? Copyright { get; set; } = string.Empty;

    public string? TranslationLanguage { get; set; } = string.Empty;

    public string? TranslatedTitle { get; set; } = string.Empty;

    public string? TranslatedDescription { get; set; } = string.Empty;

}
