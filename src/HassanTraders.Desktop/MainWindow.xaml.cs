using Wpf.Ui.Controls;
using HassanTraders.Desktop.Shell;

namespace HassanTraders.Desktop;

public partial class MainWindow : FluentWindow
{
    public MainWindow(RootViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
