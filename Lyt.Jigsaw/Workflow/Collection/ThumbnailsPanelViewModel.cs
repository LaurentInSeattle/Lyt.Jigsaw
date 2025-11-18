namespace Lyt.Jigsaw.Workflow.Collection;

using Lyt.Jigsaw.Model.GameObjects;

using static Lyt.Persistence.FileManagerModel;

public sealed partial class ThumbnailsPanelViewModel :
    ViewModel<ThumbnailsPanelView>,
    ISelectListener,
    IRecipient<LanguageChangedMessage>
{
    private readonly JigsawModel jigsawModel;
    private readonly FileManagerModel fileManagerModel;
    private readonly CollectionViewModel collectionViewModel;

    [ObservableProperty]
    private bool showInProgress;

    [ObservableProperty]
    private ObservableCollection<ThumbnailViewModel> thumbnails;

    [ObservableProperty]
    private int providersSelectedIndex;

    private ThumbnailViewModel? selectedThumbnail;
    private Model.GameObjects.Game? selectedGame;
    private List<ThumbnailViewModel>? allThumbnails;
    private List<ThumbnailViewModel>? filteredThumbnails;

    public ThumbnailsPanelViewModel(CollectionViewModel collectionViewModel)
    {
        this.jigsawModel = App.GetRequiredService<JigsawModel>();
        this.fileManagerModel = App.GetRequiredService<FileManagerModel>();
        this.collectionViewModel = collectionViewModel;
        this.Thumbnails = [];
        this.ShowInProgress = this.jigsawModel.ShowInProgress;
        this.Subscribe<LanguageChangedMessage>();
    }

    public void Receive(LanguageChangedMessage _) { } //  => this.PopulateComboBox();

    internal void LoadThumnails()
    {
        var games = this.jigsawModel.SavedGames.Values;
        var sortedGames = (from game in games orderby game.Started descending select game).ToList();
        this.allThumbnails = new(sortedGames.Count);
        foreach (var game in sortedGames)
        {
            if (!this.jigsawModel.ThumbnailCache.TryGetValue(game.Name, out byte[]? thumbnailBytes))
            {
                continue;
            }

            if (thumbnailBytes is null || thumbnailBytes.Length == 0)
            {
                continue;
            }

            // Make sure the game image is still present on disk 
            var fileIdImage = new FileId(Area.User, Kind.Binary, game.ImageName);
            if (!this.fileManagerModel.Exists(fileIdImage))
            {
                continue;
            }

            this.allThumbnails.Add(new ThumbnailViewModel(this, game, thumbnailBytes));
        }

        this.Filter();
        Schedule.OnUiThread(66, () => { this.UpdateVisualSelection(); }, DispatcherPriority.Background);
    }

    public ThumbnailViewModel? SelectedThumbnail => this.selectedThumbnail;

    public void OnSelect(object selectedObject)
    {
        if (selectedObject is ThumbnailViewModel thumbnailViewModel)
        {
            this.selectedThumbnail = thumbnailViewModel;
            var game = thumbnailViewModel.Game;
            if (this.selectedGame is null || this.selectedGame.Name != game.Name)
            {
                this.selectedGame = game;
                this.collectionViewModel.Select(game);
            }

            this.UpdateVisualSelection();
        }
    }

    internal void UpdateVisualSelection()
    {
        if (this.selectedGame is not null)
        {
            foreach (ThumbnailViewModel thumbnailViewModel in this.Thumbnails)
            {
                if (thumbnailViewModel.Game == this.selectedGame)
                {
                    thumbnailViewModel.ShowSelected();
                }
                else
                {
                    thumbnailViewModel.ShowDeselected(this.selectedGame);
                }
            }
        }
    }

    private void Filter()
    {
        if ((this.allThumbnails is not null) && (this.allThumbnails.Count > 0))
        {
            this.filteredThumbnails =
                [.. (from thumbnail in this.allThumbnails
                     where thumbnail.Game.IsCompleted == !this.ShowInProgress
                     select thumbnail)];
        }
        else
        {
            this.filteredThumbnails = null;
        }

        if (this.filteredThumbnails is not null && this.filteredThumbnails.Count > 0)
        {
            this.Thumbnails = [.. this.filteredThumbnails];

            // Clear selection: the selected game is not in the filtered list
            // Force select on the first one so that it will show up in the main area
            this.selectedGame = null;
            this.OnSelect(this.Thumbnails[0]);
        }
        else
        {
            // Empty list, Clear selection in main area too
            this.Thumbnails = [];
            this.selectedGame = null;
            this.collectionViewModel.ClearSelection(); 
        }
    }

    partial void OnProvidersSelectedIndexChanged(int value) => this.Filter();

    partial void OnShowInProgressChanged(bool value)
    {
        this.jigsawModel.ShowInProgress = value;
        this.Filter();
        Schedule.OnUiThread(66, () => { this.UpdateVisualSelection(); }, DispatcherPriority.Background);
    }
}
