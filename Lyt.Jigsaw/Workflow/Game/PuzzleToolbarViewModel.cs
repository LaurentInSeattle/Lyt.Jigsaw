namespace Lyt.Jigsaw.Workflow.Game;

public sealed partial class PuzzleToolbarViewModel: ViewModel<PuzzleToolbarView>, IRecipient<PuzzleChangedMessage>
{
    private readonly JigsawModel jigsawModel;

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

            case PuzzleChange.Progress:
                this.Progress = string.Format( "{0:D} %", (int) message.Parameter);
                break;
        }
    }

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

    partial void OnBackgroundSliderValueChanged(double value)
        // Debug.WriteLine("Background: " + value.ToString("F2"));
        => this.jigsawModel.SetPuzzleBackground (value);
}
