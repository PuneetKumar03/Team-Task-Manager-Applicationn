namespace TaskManager.Utility.Helpers;

/// <summary>
/// Centralizes all date formatting and calculation logic.
/// Views and Controllers call these methods instead of
/// writing date logic inline.
/// </summary>
public static class DateHelper
{
    // ── Display Formatting ─────────────────────────────────────────────────────

    /// <summary>
    /// Formats a date for display in the UI.
    /// Example: 25 Dec 2025
    /// </summary>
    public static string FormatForDisplay(DateTime? date)
        => date?.ToString("dd MMM yyyy") ?? "No date set";

    /// <summary>
    /// Formats a date for HTML date input fields (value attribute).
    /// Example: 2025-12-25
    /// </summary>
    public static string FormatForInput(DateTime? date)
        => date?.ToString("yyyy-MM-dd") ?? string.Empty;

    // ── Status Calculations ────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if the deadline has passed.
    /// Always compares in UTC to avoid timezone bugs.
    /// </summary>
    public static bool IsOverdue(DateTime? deadline)
        => deadline.HasValue && deadline.Value.ToUniversalTime() < DateTime.UtcNow;

    /// <summary>
    /// Returns true if the deadline is within the next N days.
    /// Used to show "due soon" warnings on the dashboard.
    /// </summary>
    public static bool IsDueSoon(DateTime? deadline, int withinDays = 3)
        => deadline.HasValue
        && deadline.Value.ToUniversalTime() >= DateTime.UtcNow
        && deadline.Value.ToUniversalTime() <= DateTime.UtcNow.AddDays(withinDays);

    /// <summary>
    /// Returns a human-readable relative time string.
    /// Examples: "2 days ago", "in 3 days", "Today", "Overdue by 5 days"
    /// Used in task cards and activity feeds.
    /// </summary>
    public static string GetRelativeTime(DateTime? deadline)
    {
        if (!deadline.HasValue) return "No deadline";

        var diff = deadline.Value.ToUniversalTime() - DateTime.UtcNow;
        var days = (int)Math.Round(diff.TotalDays);

        if (days == 0) return "Today";
        if (days == 1) return "Tomorrow";
        if (days == -1) return "Yesterday";
        if (days > 1) return $"In {days} days";
        return $"Overdue by {Math.Abs(days)} days";
    }

    /// <summary>
    /// Returns a CSS class based on deadline urgency.
    /// Used to color deadline labels in Views.
    /// </summary>
    public static string GetDeadlineCssClass(DateTime? deadline)
    {
        if (!deadline.HasValue) return "text-muted";
        if (IsOverdue(deadline)) return "text-danger fw-bold";
        if (IsDueSoon(deadline, 3)) return "text-warning fw-bold";
        if (IsDueSoon(deadline, 7)) return "text-info";
        return "text-muted";
    }

    /// <summary>
    /// Calculates how many days remain until a deadline.
    /// Returns negative numbers for overdue tasks.
    /// </summary>
    public static int DaysRemaining(DateTime? deadline)
        => deadline.HasValue
            ? (int)Math.Ceiling(
                (deadline.Value.ToUniversalTime() - DateTime.UtcNow).TotalDays)
            : 0;
}