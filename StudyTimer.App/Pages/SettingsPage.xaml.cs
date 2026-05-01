using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using StudyTimer.Core.Models;

namespace StudyTimer.App.Pages;

public partial class SettingsPage : Page
{
    private int _studentId;

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += SettingsPage_Loaded;
    }

    private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        var session = ServiceLocator.CurrentSession;
        if (session == null) return;

        if (session.User.Role == UserRole.Admin)
        {
            PanelSecurity.Visibility = Visibility.Visible;
            var students = ServiceLocator.StudentService.Search(null);
            if (students.Count > 0)
            {
                _studentId = students[0].Id;
                LoadPreferences();
            }
        }
        else
        {
            _studentId = session.User.StudentId ?? 0;
            if (_studentId > 0) LoadPreferences();
        }
    }

    private void LoadPreferences()
    {
        if (_studentId <= 0) return;
        try
        {
            var theme = ServiceLocator.ThemeService.GetByStudentId(_studentId);
            CboThemeMode.SelectedIndex = (int)theme.Mode - 1;
            CboThemeVariant.SelectedIndex = (int)theme.Variant - 1;

            var acc = ServiceLocator.AccessibilityService.GetByStudentId(_studentId);
            CboFontScale.SelectedIndex = acc.FontScale switch
            {
                FontScaleOption.Small => 0,
                FontScaleOption.Normal => 1,
                FontScaleOption.Large => 2,
                FontScaleOption.ExtraLarge => 3,
                _ => 1
            };
            ChkDyslexia.IsChecked = acc.UseDyslexiaFriendlyFont;
            ChkHighContrast.IsChecked = acc.HighContrastMode;

            var focus = ServiceLocator.FocusModeService.GetByStudentId(_studentId);
            ChkFocusMode.IsChecked = focus.Enabled;

            var lang = ServiceLocator.LocalizationService.GetByStudentId(_studentId);
            CboLanguage.SelectedIndex = (int)lang.Language - 1;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not load preferences: {ex.Message}");
        }
    }

    private void BtnSaveTheme_Click(object sender, RoutedEventArgs e)
    {
        if (_studentId <= 0) { MessageBox.Show("No student selected."); return; }
        var mode = (ThemeMode)(CboThemeMode.SelectedIndex + 1);
        var variant = (ThemeVariant)(CboThemeVariant.SelectedIndex + 1);
        try
        {
            ServiceLocator.ThemeService.SetTheme(_studentId, mode, variant);
            MessageBox.Show("Theme saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnSaveAccessibility_Click(object sender, RoutedEventArgs e)
    {
        if (_studentId <= 0) { MessageBox.Show("No student selected."); return; }
        var fontScale = CboFontScale.SelectedIndex switch
        {
            0 => FontScaleOption.Small,
            2 => FontScaleOption.Large,
            3 => FontScaleOption.ExtraLarge,
            _ => FontScaleOption.Normal
        };
        try
        {
            ServiceLocator.AccessibilityService.SetPreference(_studentId, fontScale, ChkDyslexia.IsChecked == true, ChkHighContrast.IsChecked == true);
            MessageBox.Show("Accessibility settings saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnSaveFocusMode_Click(object sender, RoutedEventArgs e)
    {
        if (_studentId <= 0) { MessageBox.Show("No student selected."); return; }
        try
        {
            ServiceLocator.FocusModeService.SetPreference(_studentId, ChkFocusMode.IsChecked == true);
            MessageBox.Show("Focus mode saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnSaveLanguage_Click(object sender, RoutedEventArgs e)
    {
        if (_studentId <= 0) { MessageBox.Show("No student selected."); return; }
        var lang = (SupportedLanguage)(CboLanguage.SelectedIndex + 1);
        try
        {
            ServiceLocator.LocalizationService.SetLanguage(_studentId, lang);
            MessageBox.Show("Language saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnExportBackup_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var json = ServiceLocator.BackupService.ExportJson();
            var dlg = new SaveFileDialog { Filter = "JSON files (*.json)|*.json", FileName = "StudyTimerBackup.json" };
            if (dlg.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(dlg.FileName, json);
                MessageBox.Show("Backup exported!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnImportBackup_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dlg = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
            if (dlg.ShowDialog() == true)
            {
                var json = System.IO.File.ReadAllText(dlg.FileName);
                ServiceLocator.BackupService.RestoreJson(json);
                MessageBox.Show("Backup imported!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void BtnCompliance_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var report = ServiceLocator.SecurityComplianceService.RunChecks();
            var text = $"Generated: {report.GeneratedAtUtc:yyyy-MM-dd HH:mm} UTC\n" +
                       $"Total Users: {report.TotalUsers}\n" +
                       $"Locked: {report.LockedUsers}\n" +
                       $"High Risk: {report.HighRiskUsers}\n" +
                       $"Compliant: {report.IsCompliant}\n" +
                       $"Findings:\n{string.Join("\n", report.Findings.Select(f => $"  • {f}"))}";
            TxtCompliance.Text = text;
            TxtCompliance.Visibility = Visibility.Visible;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }
}
