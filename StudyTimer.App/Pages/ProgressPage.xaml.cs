using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using StudyTimer.Core.Models;

namespace StudyTimer.App.Pages;

public partial class ProgressPage : Page
{
    private ProgressAnalytics? _analytics;

    public ProgressPage()
    {
        InitializeComponent();
        DpPeriod.SelectedDate = DateTime.Today;
        Loaded += ProgressPage_Loaded;
    }

    private void ProgressPage_Loaded(object sender, RoutedEventArgs e)
    {
        var students = ServiceLocator.StudentService.Search(null);
        CboStudents.ItemsSource = students;
        var session = ServiceLocator.CurrentSession;
        if (session?.User.Role == UserRole.Student)
        {
            var mine = students.FirstOrDefault(s => s.Id == session.User.StudentId);
            if (mine != null) CboStudents.SelectedItem = mine;
            CboStudents.IsEnabled = false;
        }
        else if (students.Count > 0)
        {
            CboStudents.SelectedIndex = 0;
        }
    }

    private void BtnLoad_Click(object sender, RoutedEventArgs e)
    {
        if (CboStudents.SelectedItem is not Student student) { MessageBox.Show("Select a student."); return; }
        if (DpPeriod.SelectedDate == null) { MessageBox.Show("Select a date."); return; }
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var selectedDate = DateOnly.FromDateTime(DpPeriod.SelectedDate.Value);
            if (CboPeriod.SelectedIndex == 0) // Weekly
            {
                var weekStart = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);
                _analytics = ServiceLocator.ProgressAnalyticsService.GetWeekly(student.Id, weekStart, today);
            }
            else // Monthly
            {
                _analytics = ServiceLocator.ProgressAnalyticsService.GetMonthly(student.Id, selectedDate.Year, selectedDate.Month, today);
            }

            TxtTotal.Text = _analytics.TotalCount.ToString();
            TxtCompleted.Text = _analytics.CompletedCount.ToString();
            TxtMissed.Text = _analytics.MissedCount.ToString();
            DrawChart(_analytics);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void DrawChart(ProgressAnalytics analytics)
    {
        ChartCanvas.Children.Clear();
        if (analytics.ChartPoints.Count == 0) return;

        var maxCount = analytics.ChartPoints.Max(p => p.TotalCount);
        if (maxCount == 0) maxCount = 1;

        var canvasHeight = 160.0;
        var canvasWidth = ChartCanvas.ActualWidth > 0 ? ChartCanvas.ActualWidth : 600;
        var barHeight = canvasHeight / analytics.ChartPoints.Count - 4;
        var maxWidth = canvasWidth - 80;

        for (int i = 0; i < analytics.ChartPoints.Count; i++)
        {
            var p = analytics.ChartPoints[i];
            var y = i * (barHeight + 4);

            var label = new TextBlock { Text = p.Label, FontSize = 10, Width = 60 };
            Canvas.SetLeft(label, 0);
            Canvas.SetTop(label, y + barHeight / 2 - 7);
            ChartCanvas.Children.Add(label);

            var totalBar = new Rectangle
            {
                Width = maxWidth * p.TotalCount / maxCount,
                Height = barHeight,
                Fill = Brushes.LightGray
            };
            Canvas.SetLeft(totalBar, 62);
            Canvas.SetTop(totalBar, y);
            ChartCanvas.Children.Add(totalBar);

            var completedBar = new Rectangle
            {
                Width = maxWidth * p.CompletedCount / maxCount,
                Height = barHeight,
                Fill = new SolidColorBrush(Color.FromRgb(39, 174, 96))
            };
            Canvas.SetLeft(completedBar, 62);
            Canvas.SetTop(completedBar, y);
            ChartCanvas.Children.Add(completedBar);
        }
    }

    private void BtnPrintable_Click(object sender, RoutedEventArgs e)
    {
        if (CboStudents.SelectedItem is not Student student) { MessageBox.Show("Select a student."); return; }
        if (DpPeriod.SelectedDate == null) { MessageBox.Show("Select a period date."); return; }
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var date = DateOnly.FromDateTime(DpPeriod.SelectedDate.Value);
            DateOnly startDate, endDate;
            if (CboPeriod.SelectedIndex == 0)
            {
                startDate = date.AddDays(-(int)date.DayOfWeek);
                endDate = startDate.AddDays(6);
            }
            else
            {
                startDate = new DateOnly(date.Year, date.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            var text = ServiceLocator.ParentReportExportService.GeneratePrintableText(student.Id, startDate, endDate, today);
            TxtReport.Text = text;
            TxtReport.Visibility = Visibility.Visible;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnPdf_Click(object sender, RoutedEventArgs e)
    {
        if (CboStudents.SelectedItem is not Student student) { MessageBox.Show("Select a student."); return; }
        if (DpPeriod.SelectedDate == null) { MessageBox.Show("Select a period date."); return; }
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var date = DateOnly.FromDateTime(DpPeriod.SelectedDate.Value);
            DateOnly startDate, endDate;
            if (CboPeriod.SelectedIndex == 0)
            {
                startDate = date.AddDays(-(int)date.DayOfWeek);
                endDate = startDate.AddDays(6);
            }
            else
            {
                startDate = new DateOnly(date.Year, date.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            var bytes = ServiceLocator.ParentReportExportService.GeneratePdfBytes(student.Id, startDate, endDate, today);
            var dlg = new SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", FileName = $"Report_{student.Name}.pdf" };
            if (dlg.ShowDialog() == true)
            {
                System.IO.File.WriteAllBytes(dlg.FileName, bytes);
                MessageBox.Show("PDF saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }
}
