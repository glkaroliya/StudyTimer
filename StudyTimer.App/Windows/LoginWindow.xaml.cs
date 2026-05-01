using System.Windows;
using StudyTimer.Core.Exceptions;

namespace StudyTimer.App.Windows;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        BtnRegisterLink.Visibility = ServiceLocator.Store.Users.Count == 0
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void BtnSignIn_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Visibility = Visibility.Collapsed;
        try
        {
            var session = ServiceLocator.AuthService.Login(TxtUsername.Text, PwdPassword.Password);
            ServiceLocator.CurrentSession = session;
            DialogResult = true;
        }
        catch (UnauthorizedException ex)
        {
            ShowError(ex.Message);
        }
        catch (ValidationException ex)
        {
            ShowError(ex.Message);
        }
    }

    private void BtnRegisterLink_Click(object sender, RoutedEventArgs e)
    {
        PanelRegister.Visibility = PanelRegister.Visibility == Visibility.Visible
            ? Visibility.Collapsed : Visibility.Visible;
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Visibility = Visibility.Collapsed;
        try
        {
            ServiceLocator.AuthService.RegisterAdmin(TxtRegUsername.Text, PwdRegPassword.Password);
            BtnRegisterLink.Visibility = Visibility.Collapsed;
            PanelRegister.Visibility = Visibility.Collapsed;
            TxtUsername.Text = TxtRegUsername.Text;
            MessageBox.Show("Admin registered! Please sign in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (ValidationException ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ShowError(string message)
    {
        TxtError.Text = message;
        TxtError.Visibility = Visibility.Visible;
    }
}
