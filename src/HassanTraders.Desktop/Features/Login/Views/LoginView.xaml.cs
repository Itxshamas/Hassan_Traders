using System.Windows.Controls;

namespace HassanTraders.Desktop.Features.Login;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        Loaded += (_, _) => UsernameBox.Focus();
    }
}
