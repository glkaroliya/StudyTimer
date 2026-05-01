using System.Windows;
using System.Windows.Controls;
using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;

namespace StudyTimer.App.Pages;

public partial class SubjectsPage : Page
{
    private int? _editingId;

    public SubjectsPage()
    {
        InitializeComponent();
        Loaded += (_, _) => LoadSubjects();
    }

    private void LoadSubjects() => GridSubjects.ItemsSource = ServiceLocator.SubjectService.Search(null);

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        _editingId = null;
        TxtFormTitle.Text = "Add Subject";
        TxtName.Text = "";
        TxtDescription.Text = "";
        PanelForm.Visibility = Visibility.Visible;
    }

    private void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (GridSubjects.SelectedItem is not Subject s) { MessageBox.Show("Select a subject."); return; }
        _editingId = s.Id;
        TxtFormTitle.Text = "Edit Subject";
        TxtName.Text = s.Name;
        TxtDescription.Text = s.Description;
        PanelForm.Visibility = Visibility.Visible;
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (GridSubjects.SelectedItem is not Subject s) { MessageBox.Show("Select a subject."); return; }
        if (MessageBox.Show($"Delete subject '{s.Name}'?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        try
        {
            ServiceLocator.SubjectService.Delete(s.Id, ServiceLocator.CurrentSession?.User.Id);
            LoadSubjects();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_editingId.HasValue)
                ServiceLocator.SubjectService.Update(_editingId.Value, TxtName.Text, TxtDescription.Text, ServiceLocator.CurrentSession?.User.Id);
            else
                ServiceLocator.SubjectService.Create(TxtName.Text, TxtDescription.Text, ServiceLocator.CurrentSession?.User.Id);
            PanelForm.Visibility = Visibility.Collapsed;
            LoadSubjects();
        }
        catch (ValidationException ex) { MessageBox.Show(ex.Message); }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => PanelForm.Visibility = Visibility.Collapsed;
}
