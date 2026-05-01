using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;

namespace StudyTimer.App.Pages;

public partial class TimerPage : Page
{
    private DispatcherTimer? _timer;
    private TimerSessionState? _state;
    private bool _isPaused;
    private int _totalSlotSeconds;

    public TimerPage()
    {
        InitializeComponent();
        Loaded += TimerPage_Loaded;
    }

    private void TimerPage_Loaded(object sender, RoutedEventArgs e)
    {
        DpDate.SelectedDate = DateTime.Today;
        var students = ServiceLocator.StudentService.Search(null);
        CboStudents.ItemsSource = students;
        var session = ServiceLocator.CurrentSession;
        if (session?.User.Role == UserRole.Student)
        {
            var myStudent = students.FirstOrDefault(s => s.Id == session.User.StudentId);
            if (myStudent != null) CboStudents.SelectedItem = myStudent;
            CboStudents.IsEnabled = false;
        }
        else if (students.Count > 0)
        {
            CboStudents.SelectedIndex = 0;
        }
    }

    private void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        if (CboStudents.SelectedItem is not Student student) { MessageBox.Show("Select a student."); return; }
        if (DpDate.SelectedDate == null) { MessageBox.Show("Select a date."); return; }
        try
        {
            StopTimer();
            var date = DateOnly.FromDateTime(DpDate.SelectedDate.Value);
            _state = ServiceLocator.TimerService.Start(student.Id, date);
            _totalSlotSeconds = _state.RemainingSeconds;
            UpdateUI();
            BtnPauseResume.IsEnabled = true;
            BtnPauseResume.Content = "Pause";
            _isPaused = false;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }
        catch (NotFoundException ex) { MessageBox.Show(ex.Message); }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_state == null || _isPaused) return;
        try
        {
            var prevSlotId = _state.CurrentSlotId;
            _state = ServiceLocator.TimerService.Tick(_state, 1);
            if (_state.CurrentSlotId != prevSlotId)
            {
                _totalSlotSeconds = _state.RemainingSeconds;
            }
            UpdateUI();
            if (_state.IsDayCompleted)
            {
                StopTimer();
            }
        }
        catch (Exception ex)
        {
            StopTimer();
            MessageBox.Show(ex.Message);
        }
    }

    private void UpdateUI()
    {
        if (_state == null) return;
        var ts = TimeSpan.FromSeconds(_state.RemainingSeconds);
        TxtCountdown.Text = ts.ToString(@"hh\:mm\:ss");
        TxtFocusMode.Visibility = _state.IsFocusModeEnabled ? Visibility.Visible : Visibility.Collapsed;
        PanelDayCompleted.Visibility = _state.IsDayCompleted ? Visibility.Visible : Visibility.Collapsed;

        if (_totalSlotSeconds > 0)
        {
            var progress = (1.0 - (double)_state.RemainingSeconds / _totalSlotSeconds) * 100;
            PbProgress.Value = Math.Clamp(progress, 0, 100);
        }

        var slot = ServiceLocator.Store.TimetableSlots.FirstOrDefault(x => x.Id == _state.CurrentSlotId);
        if (slot != null)
        {
            var subject = ServiceLocator.SubjectService.Search(null).FirstOrDefault(s => s.Id == slot.SubjectId);
            TxtSubject.Text = subject?.Name ?? $"Subject #{slot.SubjectId}";
            TxtActivity.Text = slot.ActivityDescription;
        }
    }

    private void BtnPauseResume_Click(object sender, RoutedEventArgs e)
    {
        _isPaused = !_isPaused;
        BtnPauseResume.Content = _isPaused ? "Resume" : "Pause";
    }

    public void StopTimer()
    {
        if (_timer != null)
        {
            _timer.Tick -= Timer_Tick;
            _timer.Stop();
            _timer = null;
        }
        _state = null;
        _isPaused = false;
        _totalSlotSeconds = 0;
    }
}
