namespace Lyt.Jigsaw.Controls;

public partial class ZoomController : UserControl
{
    public ZoomController()
    {
        this.InitializeComponent();
        this.Opacity = 1.0;
        this.Slider.Minimum = 1.0;
        this.Slider.Maximum = 2.0;
        this.Slider.SmallChange = 0.20;
        this.Slider.TickFrequency = 0.20;
        this.Slider.Value = 1.0;
    }

    public void SetMin() => this.Slider.Value = this.Slider.Minimum;

    public void SetMax() => this.Slider.Value = this.Slider.Maximum;

    private void OnSliderValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        => new ZoomRequestMessage(e.NewValue, this.Tag).Publish();

    private void OnButtonMaxClick(object? sender, RoutedEventArgs e) => this.SetMax();

    private void OnButtonMinClick(object? sender, RoutedEventArgs e) => this.SetMin();

    /// <summary> Max Styled Property </summary>
    public static readonly StyledProperty<double> MaxProperty =
        AvaloniaProperty.Register<ZoomController, double>(
            nameof(Max),
            defaultValue: 2.0,
            inherits: false,
            defaultBindingMode: BindingMode.OneWay,
            validate: null,
            coerce: null,
            enableDataValidation: false);

    /// <summary> Gets or sets the Max property.</summary>
    public double Max
    {
        get => this.GetValue(MaxProperty);
        set
        {
            this.SetValue(MaxProperty, value);
            this.Slider.Maximum = value;
        }
    }

    /// <summary> Min Styled Property </summary>
    public static readonly StyledProperty<double> MinProperty =
        AvaloniaProperty.Register<ZoomController, double>(
            nameof(Min),
            defaultValue: 1.0,
            inherits: false,
            defaultBindingMode: BindingMode.OneWay,
            validate: null,
            coerce: null,
            enableDataValidation: false);

    /// <summary> Gets or sets the Min property.</summary>
    public double Min
    {
        get => this.GetValue(MinProperty);
        set
        {
            this.SetValue(MinProperty, value);
            this.Slider.Minimum = value;
        }
    }

    /// <summary> MaxText Styled Property </summary>
    public static readonly StyledProperty<string> MaxTextProperty =
        AvaloniaProperty.Register<ZoomController, string>(
            nameof(MaxText),
            defaultValue: "Max",
            inherits: false,
            defaultBindingMode: BindingMode.OneWay,
            validate: null,
            coerce: null,
            enableDataValidation: false);

    /// <summary> Gets or sets the MaxText property.</summary>
    public string MaxText
    {
        get => this.GetValue(MaxTextProperty);
        set
        {
            this.SetValue(MaxTextProperty, value);
            this.maxButton.Text = value;
        }
    }

    /// <summary> MinText Styled Property </summary>
    public static readonly StyledProperty<string> MinTextProperty =
        AvaloniaProperty.Register<ZoomController, string>(
            nameof(MinText),
            defaultValue: "Fit",
            inherits: false,
            defaultBindingMode: BindingMode.OneWay,
            validate: null,
            coerce: null,
            enableDataValidation: false);

    /// <summary> Gets or sets the MinText property.</summary>
    public string MinText
    {
        get => this.GetValue(MinTextProperty);
        set
        {
            this.SetValue(MinTextProperty, value);
            this.minButton.Text = value;
        }
    }
}