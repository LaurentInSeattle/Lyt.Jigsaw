namespace Lyt.Jigsaw.Workflow.Game;

public sealed partial class PuzzleToolbarViewModel(JigsawModel jigsawModel) : ViewModel<PuzzleToolbarView>
{
    private readonly JigsawModel jigsawModel = jigsawModel;

    [RelayCommand]
    public void OnRandomize() { }

    [RelayCommand]
    public void OnShowImage(ButtonTag buttonTag) 
    {
        if (buttonTag == ButtonTag.CountinuousBegin || buttonTag == ButtonTag.CountinuousEnd)
        {
            bool show = buttonTag == ButtonTag.CountinuousBegin;
            new ShowPuzzleImageMessage(show).Publish();
        } 
    }

    [ObservableProperty]
    private double backgroundSliderValue;

    partial void OnBackgroundSliderValueChanged(double value)
        // Debug.WriteLine("Background: " + value.ToString("F2"));
        => this.jigsawModel.SetPuzzleBackground (value);
}
