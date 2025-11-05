namespace Lyt.Jigsaw.Workflow.Game;

public sealed partial class PuzzleToolbarViewModel : ViewModel<PuzzleToolbarView>
{
    private readonly JigsawModel jigsawModel;

    [RelayCommand]
    public void OnRandomize() { }

    [ObservableProperty]
    private double backgroundSliderValue;

    public PuzzleToolbarViewModel(JigsawModel jigsawModel) => this.jigsawModel = jigsawModel;

    partial void OnBackgroundSliderValueChanged(double value)
        // Debug.WriteLine("Background: " + value.ToString("F2"));
        => this.jigsawModel.SetPuzzleBackground (value);
}
