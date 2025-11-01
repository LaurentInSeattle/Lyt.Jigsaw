namespace Lyt.Jigsaw.Workflow.Collection;

using static ApplicationMessagingExtensions;
using static ToolbarCommandMessage;

public sealed partial class CollectionToolbarViewModel : ViewModel<CollectionToolbarView>
{
#pragma warning disable CA1822 // Mark members as static

    [RelayCommand]
    public void OnPlay() => Command(ToolbarCommand.Play);

    [RelayCommand]
    public void OnRemoveFromCollection() => Command(ToolbarCommand.RemoveFromCollection);
    
    [RelayCommand]
    public void OnSaveToDesktop() => Command(ToolbarCommand.CollectionSaveToDesktop);

#pragma warning restore CA1822
}
