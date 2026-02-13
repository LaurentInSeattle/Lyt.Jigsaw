namespace Lyt.Jigsaw.Workflow.Game;

public sealed partial class PuzzleToolbarViewModel: ViewModel<PuzzleToolbarView>, IRecipient<PuzzleChangedMessage>
{
    private readonly JigsawModel jigsawModel;

    private bool showEdges;

    [ObservableProperty]
    private double backgroundSliderValue;

    [ObservableProperty]
    private string progress = "-" ;

    public PuzzleToolbarViewModel(JigsawModel jigsawModel)
    {
        this.jigsawModel = jigsawModel;
        this.Subscribe<PuzzleChangedMessage>();    
    }

    public void Receive(PuzzleChangedMessage message)
    {
        switch (message.Change)
        {
            default:
                return;

            case PuzzleChange.Start:
                this.View.ZoomController.SetMin(); 
                break;

            case PuzzleChange.Progress:
                this.Progress = string.Format( "{0:D} %", (int) message.Parameter);
                break;
        }
    }

#pragma warning disable CA1822 
    // Mark members as static
    // Relay commands cannot be static
    
    [RelayCommand]
    public void OnShowEdges()
    {
        this.showEdges = !this.showEdges;
        new ShowEdgesMessage(this.showEdges).Publish();
    }

    [RelayCommand]
    public void OnHint()
        => this.jigsawModel.ProvidePuzzleHint();

    [RelayCommand]
    public void OnShowImage(ButtonTag buttonTag) 
    {
        if (buttonTag == ButtonTag.CountinuousBegin || buttonTag == ButtonTag.CountinuousEnd)
        {
            bool show = buttonTag == ButtonTag.CountinuousBegin;
            new ShowPuzzleImageMessage(show).Publish();
        } 
    }

    [RelayCommand]
    public void OnFullscreen() =>
        new ToolbarCommandMessage(ToolbarCommandMessage.ToolbarCommand.PlayFullscreen).Publish();

    [RelayCommand]
    public void OnRearrange() =>
        new ToolbarCommandMessage(ToolbarCommandMessage.ToolbarCommand.Rearrange).Publish();

#pragma warning restore CA1822 // Mark members as static

    partial void OnBackgroundSliderValueChanged(double value)
        => this.jigsawModel.SetPuzzleBackground (value);
}
