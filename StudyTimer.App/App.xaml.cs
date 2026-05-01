using System.Windows;
using StudyTimer.App.Windows;

namespace StudyTimer.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShowLogin();
    }

    private void ShowLogin()
    {
        var login = new LoginWindow();
        if (login.ShowDialog() == true)
        {
            var main = new MainWindow();
            main.Show();
        }
        else
        {
            Shutdown();
        }
    }
}
