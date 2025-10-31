namespace Lyt.Jigsaw.Model.PictureObjects; 

public sealed class Picture
{
    public Picture() { /* Required for serialization */ }

    public Picture(PictureMetadata pictureMetadata)
    {
        this.PictureMetadata = pictureMetadata;
        this.SetImageFilePaths(); 
    }

    [JsonRequired]
    public PictureMetadata PictureMetadata { get; set; } = new(); 

    [JsonRequired]
    public bool Keep { get; set; } = false;

    [JsonRequired]
    public string ImageFilePath { get; set; } = string.Empty;

    [JsonRequired]
    public string ThumbnailFilePath { get; set; } = string.Empty ;

    public string Label => 
        string.Format ( 
            "{0}" , 
            this.PictureMetadata.Date.ToShortDateString() );

    private void SetImageFilePaths ()
    {
        var meta = this.PictureMetadata;
        var date = meta.Date;
        string? maybeExtension = meta.UrlFileExtension();
        string extension = string.IsNullOrWhiteSpace(maybeExtension) ? "jpg" : maybeExtension;
        this.ImageFilePath = 
            string.Format(
                "{0}_{1}_{2}_{3}.{4}", 
                "TODO_Name", date.Year, date.Month, date.Day, extension);
        this.ThumbnailFilePath =
            string.Format(
                "{0}_{1}_{2}_{3}_Thumb.{4}",
                "TODO_Name", date.Year, date.Month, date.Day, extension);
    }
}
