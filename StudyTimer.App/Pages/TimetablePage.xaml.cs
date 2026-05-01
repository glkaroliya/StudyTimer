using System.Windows;
using System.Windows.Controls;
using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;

namespace StudyTimer.App.Pages;

public partial class TimetablePage : Page
{
    private int? _editingSlotId;
    private int? _reschedulingSlotId;

    public TimetablePage()
    {
        InitializeComponent();
        Loaded += TimetablePage_Loaded;
    }

    private void TimetablePage_Loaded(object sender, RoutedEventArgs e)
    {
        DpDate.SelectedDate = DateTime.Today;
        var students = ServiceLocator.StudentService.Search(null);
        CboStudents.ItemsSource = students;

        var session = ServiceLocator.CurrentSession;
        if (session?.User.Role == UserRole.Student)
        {
            var studentId = session.User.StudentId;
            var myStudent = students.FirstOrDefault(s => s.Id == studentId);
            if (myStudent != null) CboStudents.SelectedItem = myStudent;
            CboStudents.IsEnabled = false;
        }
        else if (students.Count > 0)
        {
            CboStudents.SelectedIndex = 0;
        }

        CboSubjects.ItemsSource = ServiceLocator.SubjectService.Search(null);
        LoadSlots();
    }

    private void Filter_Changed(object sender, SelectionChangedEventArgs e) => LoadSlots();

    private void LoadSlots()
    {
        if (CboStudents.SelectedItem is not Student student || DpDate.SelectedDate == null) return;
        try
        {
            var date = DateOnly.FromDateTime(DpDate.SelectedDate.Value);
            var slots = ServiceLocator.TimetableService.Search(student.Id, date);
            var subjects = ServiceLocator.SubjectService.Search(null);
            var subjectsById = subjects.ToDictionary(s => s.Id, s => s.Name);
            var rows = slots.Select(s => new TimetableRow(s, subjectsById.GetValueOrDefault(s.SubjectId) ?? s.SubjectId.ToString())).ToList();
            GridSlots.ItemsSource = rows;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        _editingSlotId = null;
        TxtFormTitle.Text = "Add Slot";
        DpFormDate.SelectedDate = DpDate.SelectedDate ?? DateTime.Today;
        TxtStartTime.Text = "09:00";
        TxtDuration.Text = "60";
        TxtActivity.Text = "";
        PanelForm.Visibility = Visibility.Visible;
        PanelReschedule.Visibility = Visibility.Collapsed;
    }

    private void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (GridSlots.SelectedItem is not TimetableRow row) { MessageBox.Show("Select a slot first."); return; }
        _editingSlotId = row.Id;
        TxtFormTitle.Text = "Edit Slot";
        DpFormDate.SelectedDate = row.Date.ToDateTime(TimeOnly.MinValue);
        TxtStartTime.Text = row.StartTime.ToString("HH\\:mm");
        TxtDuration.Text = row.DurationMinutes.ToString();
        TxtActivity.Text = row.ActivityDescription;
        var subjects = ServiceLocator.SubjectService.Search(null);
        CboSubjects.ItemsSource = subjects;
        CboSubjects.SelectedItem = subjects.FirstOrDefault(s => s.Id == row.SubjectId);
        PanelForm.Visibility = Visibility.Visible;
        PanelReschedule.Visibility = Visibility.Collapsed;
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (GridSlots.SelectedItem is not TimetableRow row) { MessageBox.Show("Select a slot first."); return; }
        if (MessageBox.Show("Delete this slot?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        try
        {
            ServiceLocator.TimetableService.Delete(row.Id, ServiceLocator.CurrentSession?.User.Id);
            LoadSlots();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnComplete_Click(object sender, RoutedEventArgs e)
    {
        if (GridSlots.SelectedItem is not TimetableRow row) { MessageBox.Show("Select a slot first."); return; }
        try
        {
            ServiceLocator.TimetableService.MarkCompleted(row.Id, !row.Completed, ServiceLocator.CurrentSession?.User.Id);
            LoadSlots();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnReschedule_Click(object sender, RoutedEventArgs e)
    {
        if (GridSlots.SelectedItem is not TimetableRow row) { MessageBox.Show("Select a slot first."); return; }
        _reschedulingSlotId = row.Id;
        DpRescheduleDate.SelectedDate = DateTime.Today.AddDays(1);
        TxtRescheduleTime.Text = row.StartTime.ToString("HH\\:mm");
        PanelReschedule.Visibility = Visibility.Visible;
        PanelForm.Visibility = Visibility.Collapsed;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (CboStudents.SelectedItem is not Student student) return;
        if (CboSubjects.SelectedItem is not Subject subject) { MessageBox.Show("Select a subject."); return; }
        if (DpFormDate.SelectedDate == null) { MessageBox.Show("Select a date."); return; }
        if (!TimeOnly.TryParseExact(TxtStartTime.Text, "HH:mm", null, System.Globalization.DateTimeStyles.None, out var startTime))
        { MessageBox.Show("Invalid time format. Use HH:mm"); return; }
        if (!int.TryParse(TxtDuration.Text, out var duration)) { MessageBox.Show("Invalid duration."); return; }
        try
        {
            var date = DateOnly.FromDateTime(DpFormDate.SelectedDate.Value);
            if (_editingSlotId.HasValue)
                ServiceLocator.TimetableService.Update(_editingSlotId.Value, subject.Id, date, startTime, duration, TxtActivity.Text, ServiceLocator.CurrentSession?.User.Id);
            else
                ServiceLocator.TimetableService.Create(student.Id, subject.Id, date, startTime, duration, TxtActivity.Text, ServiceLocator.CurrentSession?.User.Id);
            PanelForm.Visibility = Visibility.Collapsed;
            LoadSlots();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => PanelForm.Visibility = Visibility.Collapsed;

    private void BtnDoReschedule_Click(object sender, RoutedEventArgs e)
    {
        if (_reschedulingSlotId == null) return;
        if (DpRescheduleDate.SelectedDate == null) { MessageBox.Show("Select a date."); return; }
        if (!TimeOnly.TryParseExact(TxtRescheduleTime.Text, "HH:mm", null, System.Globalization.DateTimeStyles.None, out var t))
        { MessageBox.Show("Invalid time format."); return; }
        try
        {
            var date = DateOnly.FromDateTime(DpRescheduleDate.SelectedDate.Value);
            ServiceLocator.TimetableService.Reschedule(_reschedulingSlotId.Value, date, t, null, null, ServiceLocator.CurrentSession?.User.Id);
            PanelReschedule.Visibility = Visibility.Collapsed;
            LoadSlots();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnCancelReschedule_Click(object sender, RoutedEventArgs e) => PanelReschedule.Visibility = Visibility.Collapsed;

    private sealed record TimetableRow(TimetableSlot Slot, string SubjectName)
    {
        public int Id => Slot.Id;
        public int SubjectId => Slot.SubjectId;
        public DateOnly Date => Slot.Date;
        public TimeOnly StartTime => Slot.StartTime;
        public int DurationMinutes => Slot.DurationMinutes;
        public string ActivityDescription => Slot.ActivityDescription;
        public bool Completed => Slot.Completed;
        public bool IsRescheduled => Slot.IsRescheduled;
    }
}
