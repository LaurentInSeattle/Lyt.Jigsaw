namespace Lyt.Jigsaw.Workflow.Collection;

public sealed partial class DropViewModel : ViewModel<DropView>
{
    /// <summary> Returns true if the path is a valid image file. </summary>
    internal bool OnDrop(string path)
    {
        try             
        {
            byte[] imageBytes = File.ReadAllBytes(path);
            if ((imageBytes is null) || (imageBytes.Length < 256))
            {
                throw new Exception("Failed to read image from disk: " + path);
            }

            var bitmap = WriteableBitmap.Decode(new MemoryStream(imageBytes));
            if (bitmap is not null)
            {
                var collectionViewModel = App.GetRequiredService<CollectionViewModel>(); 
                return collectionViewModel.Select(path, imageBytes);
            }

            throw new Exception("Failed to load image: " + path); 
        }
        catch (Exception ex) 
        { 
            this.Logger.Warning(ex.ToString());
            return false;
        }
    }
}
