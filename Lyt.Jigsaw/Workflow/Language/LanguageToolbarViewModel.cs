namespace Lyt.Jigsaw.Workflow.Language;

public sealed partial class LanguageToolbarViewModel : ViewModel<LanguageToolbarView>
{
#pragma warning disable CA1822 // Mark members as static
    [RelayCommand]
    // FOR NOW 
    public void OnNext() => ViewSelector<ActivatedView>.Select(ActivatedView.Collection);
    // LATER 
    // public void OnNext() => ViewSelector<ActivatedView>.Select(ActivatedView.Intro);
#pragma warning restore CA1822 
}
