using StudyTimer.Core.Models;
using StudyTimer.Core.Services;
using StudyTimer.Core.Utils;

namespace StudyTimer.App;

public static class ServiceLocator
{
    private static readonly SystemDateTimeProvider DateTimeProvider = new();

    public static readonly StudyDataStore Store = new();
    public static readonly AuthService AuthService = new(Store, DateTimeProvider);
    public static readonly StudentService StudentService = new(Store);
    public static readonly SubjectService SubjectService = new(Store);
    public static readonly TimetableService TimetableService = new(Store);
    public static readonly TimerService TimerService = new(Store);
    public static readonly StudentDashboardService StudentDashboardService = new(Store);
    public static readonly GamificationService GamificationService = new(Store);
    public static readonly ThemeService ThemeService = new(Store);
    public static readonly AccessibilityService AccessibilityService = new(Store);
    public static readonly FocusModeService FocusModeService = new(Store);
    public static readonly LocalizationService LocalizationService = new(Store);
    public static readonly ReminderService ReminderService = new(Store, DateTimeProvider);
    public static readonly ReviewNoteService ReviewNoteService = new(Store, DateTimeProvider);
    public static readonly ProgressAnalyticsService ProgressAnalyticsService = new(Store);
    public static readonly ParentReportService ParentReportService = new(Store, GamificationService);
    public static readonly ParentReportExportService ParentReportExportService = new(ParentReportService, LocalizationService);
    public static readonly BackupService BackupService = new(Store);
    public static readonly AuditLogService AuditLogService = new(Store, DateTimeProvider);
    public static readonly SecurityComplianceService SecurityComplianceService = new(Store, DateTimeProvider);

    public static AuthenticatedSession? CurrentSession { get; set; }

    static ServiceLocator()
    {
        if (Store.Users.Count == 0)
        {
            AuthService.RegisterAdmin("admin", "Admin123");
        }
    }
}
