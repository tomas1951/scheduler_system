using Avalonia;
using Avalonia.Controls;
using SchedulerClientApp.ViewModels;

namespace SchedulerClientApp.Views;

public partial class MainView : UserControl
{

    public MainView()
    {
        InitializeComponent();

        var textBlock = this.FindControl<TextBlock>("logTextBlock");
        if (textBlock is not null)
            textBlock.PropertyChanged += ScrollToEnd;
    }

    private void ScrollToEnd(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TextBlock.TextProperty)
        {
            // Scroll to end
            var scrollViewer = this.FindControl<ScrollViewer>("scrollViewer1");
            scrollViewer?.ScrollToEnd();
        }
    }
}
