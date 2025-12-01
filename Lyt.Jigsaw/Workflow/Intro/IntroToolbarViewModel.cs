namespace Lyt.Jigsaw.Workflow.Intro;

public sealed partial class IntroToolbarViewModel : ViewModel<IntroToolbarView>
{
#pragma warning disable CA1822 // Mark members as static
    [RelayCommand]
    public void OnNext()
#pragma warning restore CA1822 
    {
        var jigsawModel = App.GetRequiredService<JigsawModel>();
        jigsawModel.IsFirstRun = false;
        jigsawModel.Save();

        ViewSelector<ActivatedView>.Select(ActivatedView.Collection);
    }
}
