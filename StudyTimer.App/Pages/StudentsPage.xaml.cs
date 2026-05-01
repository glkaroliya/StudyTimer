using System.Windows;
using System.Windows.Controls;
using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;

namespace StudyTimer.App.Pages;

public partial class StudentsPage : Page
{
    private int? _editingId;

    public StudentsPage()
    {
        InitializeComponent();
        Loaded += (_, _) => LoadStudents();
    }

    private void LoadStudents() => GridStudents.ItemsSource = ServiceLocator.StudentService.Search(null);

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        _editingId = null;
        TxtFormTitle.Text = "Add Student";
        TxtName.Text = "";
        TxtGrade.Text = "";
        ChkActive.IsChecked = true;
        PanelForm.Visibility = Visibility.Visible;
    }

    private void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (GridStudents.SelectedItem is not Student s) { MessageBox.Show("Select a student."); return; }
        _editingId = s.Id;
        TxtFormTitle.Text = "Edit Student";
        TxtName.Text = s.Name;
        TxtGrade.Text = s.Grade.ToString();
        ChkActive.IsChecked = s.IsActive;
        PanelForm.Visibility = Visibility.Visible;
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (GridStudents.SelectedItem is not Student s) { MessageBox.Show("Select a student."); return; }
        if (MessageBox.Show($"Delete student '{s.Name}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        try
        {
            ServiceLocator.StudentService.Delete(s.Id, ServiceLocator.CurrentSession?.User.Id);
            LoadStudents();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(TxtGrade.Text, out var grade)) { MessageBox.Show("Enter a valid grade (1-12)."); return; }
        try
        {
            if (_editingId.HasValue)
                ServiceLocator.StudentService.Update(_editingId.Value, TxtName.Text, grade, ChkActive.IsChecked == true, ServiceLocator.CurrentSession?.User.Id);
            else
                ServiceLocator.StudentService.Create(TxtName.Text, grade, ChkActive.IsChecked == true, ServiceLocator.CurrentSession?.User.Id);
            PanelForm.Visibility = Visibility.Collapsed;
            LoadStudents();
        }
        catch (ValidationException ex) { MessageBox.Show(ex.Message); }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => PanelForm.Visibility = Visibility.Collapsed;
}
