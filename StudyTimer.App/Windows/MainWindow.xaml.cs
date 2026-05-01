using System.Windows;
using StudyTimer.App.Pages;
using StudyTimer.Core.Models;

namespace StudyTimer.App.Windows;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var session = ServiceLocator.CurrentSession;
        if (session != null)
        {
            TxtUserInfo.Text = $"{session.User.Username} ({session.User.Role})";
            if (session.User.Role == UserRole.Student)
            {
                BtnStudents.Visibility = Visibility.Collapsed;
                BtnSubjects.Visibility = Visibility.Collapsed;
            }
        }
        Navigate("Dashboard");
    }

    private void NavBtn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is string tag)
        {
            Navigate(tag);
        }
    }

    private void Navigate(string page)
    {
        if (MainFrame.Content is TimerPage timerPage)
        {
            timerPage.StopTimer();
        }

        MainFrame.Navigate(page switch
        {
            "Dashboard" => (object)new DashboardPage(),
            "Timetable" => new TimetablePage(),
            "Timer" => new TimerPage(),
            "Students" => new StudentsPage(),
            "Subjects" => new SubjectsPage(),
            "Progress" => new ProgressPage(),
            "Settings" => new SettingsPage(),
            _ => new DashboardPage()
        });
    }

    private void BtnSignOut_Click(object sender, RoutedEventArgs e)
    {
        if (MainFrame.Content is TimerPage timerPage)
        {
            timerPage.StopTimer();
        }
        ServiceLocator.CurrentSession = null;
        // Prevent auto-shutdown when last window closes during re-login flow
        Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        Close();
        var login = new LoginWindow();
        if (login.ShowDialog() == true)
        {
            Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
            var main = new MainWindow();
            main.Show();
        }
        else
        {
            Application.Current.Shutdown();
        }
    }
}
