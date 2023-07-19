using Avalonia;
using Avalonia.Controls;
using SchedulerClientApp.ViewModels;

namespace SchedulerClientApp.Views;

public partial class MainView : UserControl
{
    //private ScrollViewer messageLogScrollViewer;

    public MainView()
    {
        InitializeComponent();

        //ScrollViewer? messageLogScrollViewer = this.FindControl<ScrollViewer>("MessageLogScrollViewer");

        // Subscribe to property changes
        //var textBlock = this.FindControl<TextBlock>("logTextBlock");
        //textBlock.PropertyChanged += ScrollToEnd;

        var viewModel = new MainViewModel();
        var scrollViewer = this.FindControl<ScrollViewer>("scrollViewer");
        //viewModel.MessageAdded += (sender, e) => scrollViewer?.ScrollToEnd();
    }

    //public void ScrollTextToEnd()
    //{
    //    messageLogScrollViewer.ScrollToEnd();
    //}

    //private void ScrollToEnd(object? sender, AvaloniaPropertyChangedEventArgs e)
    //{
    //    if (e.Property == TextBlock.TextProperty)
    //    {
    //        // Scroll to end
    //        var scrollViewer = this.FindControl<ScrollViewer>("scrollViewer");
    //        scrollViewer.ScrollToEnd();
    //    }
    //}
}
