namespace Lyt.Jigsaw.Workflow.Language;

public sealed partial class LanguageToolbarViewModel : ViewModel<LanguageToolbarView>
{
#pragma warning disable CA1822 // Mark members as static
    [RelayCommand]
    public void OnNext()
    {
        var model = App.GetRequiredService<JigsawModel>();
        model.ClearFirstRun();
        // FOR NOW 
        ViewSelector<ActivatedView>.Select(ActivatedView.Collection);
        // LATER 
        // public void OnNext() => ViewSelector<ActivatedView>.Select(ActivatedView.Intro);
    }

#pragma warning restore CA1822
}
