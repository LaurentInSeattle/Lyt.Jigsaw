namespace Lyt.Jigsaw.Workflow.Settings;

public sealed partial class SettingsViewModel(JigsawModel jigsawModel) : ViewModel<SettingsView>
{
    private readonly JigsawModel jigsawModel = jigsawModel;

    /* 
     * TODO 
     * 
     
    public override void OnViewLoaded()
    {
        base.OnViewLoaded();
        this.Populate();
    }

    public override void Activate(object? activationParameters)
    {
        base.Activate(activationParameters);
        this.Populate();
    }

    private void OnToolbarCommand(ToolbarCommandMessage message)
    {
        switch (message.Command)
        {
            // Ignore all other commands 
            default:
                break;
        }
    }

    private void Populate()
    {
        //With (this.isPopulating) 
        //{
        //}
    }
    */
}
