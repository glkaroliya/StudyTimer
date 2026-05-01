using System.Windows;
using System.Windows.Controls;
using StudyTimer.Core.Models;

namespace StudyTimer.App.Pages;

public partial class DashboardPage : Page
{
    private int _selectedStudentId;

    public DashboardPage()
    {
        InitializeComponent();
        DpDate.SelectedDate = DateTime.Today;
        Loaded += DashboardPage_Loaded;
    }

    private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        var session = ServiceLocator.CurrentSession;
        if (session == null) return;

        if (session.User.Role == UserRole.Admin)
        {
            PanelStudentSelector.Visibility = Visibility.Visible;
            var students = ServiceLocator.StudentService.Search(null);
            CboStudents.ItemsSource = students;
            if (students.Count > 0)
            {
                CboStudents.SelectedIndex = 0;
                _selectedStudentId = students[0].Id;
            }
        }
        else
        {
            _selectedStudentId = session.User.StudentId ?? 0;
        }

        LoadDashboard();
    }

    private void CboStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CboStudents.SelectedItem is Student s)
        {
            _selectedStudentId = s.Id;
            LoadDashboard();
        }
    }

    private void DpDate_Changed(object sender, SelectionChangedEventArgs e)
    {
        LoadDashboard();
    }

    private void LoadDashboard()
    {
        if (_selectedStudentId <= 0 || DpDate.SelectedDate == null) return;
        try
        {
            var date = DateOnly.FromDateTime(DpDate.SelectedDate.Value);
            var dashboard = ServiceLocator.StudentDashboardService.Get(_selectedStudentId, date);
            TxtCompleted.Text = dashboard.CompletedCount.ToString();
            TxtMissed.Text = dashboard.MissedCount.ToString();
            TxtReviews.Text = dashboard.ReviewNotes.Count.ToString();
            GridSlots.ItemsSource = dashboard.Slots;

            var profile = ServiceLocator.GamificationService.GetProfile(_selectedStudentId, date);
            TxtStreak.Text = profile.CurrentStreakDays.ToString();
            TxtPoints.Text = profile.TotalPoints.ToString();
            TxtBadges.Text = profile.Badges.Count == 0
                ? "None"
                : string.Join(", ", profile.Badges.Select(b => b.Badge.ToString()));

            var reminders = ServiceLocator.ReminderService.GetPendingReminders(_selectedStudentId);
            ListReminders.ItemsSource = reminders;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}
