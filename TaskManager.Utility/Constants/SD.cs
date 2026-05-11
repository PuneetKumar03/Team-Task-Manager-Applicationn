namespace TaskManager.Utility.Constants;

/// <summary>
/// SD = Static Details.
/// Single source of truth for every magic string in the application.
/// If a string appears more than once in your codebase, it belongs here.
/// Never type "Admin" or "Member" as raw strings anywhere else.
/// </summary>
public static class SD
{
    // ── Roles ─────────────────────────────────────────────────────────────────
    public const string Role_Admin = "Admin";
    public const string Role_Member = "Member";

    // ── Task Status Display Labels ─────────────────────────────────────────────
    public const string Status_Todo = "Todo";
    public const string Status_InProgress = "In Progress";
    public const string Status_InReview = "In Review";
    public const string Status_Completed = "Completed";

    // ── Task Priority Display Labels ───────────────────────────────────────────
    public const string Priority_Low = "Low";
    public const string Priority_Medium = "Medium";
    public const string Priority_High = "High";
    public const string Priority_Critical = "Critical";

    // ── TempData Keys (used in Controllers to pass flash messages to Views) ────
    public const string TempData_Success = "success";
    public const string TempData_Error = "error";
    public const string TempData_Warning = "warning";

    // ── Pagination ─────────────────────────────────────────────────────────────
    public const int PageSize_Default = 10;
    public const int PageSize_Small = 5;

    // ── Date Formats ───────────────────────────────────────────────────────────
    public const string DateFormat_Display = "dd MMM yyyy";
    public const string DateFormat_Input = "yyyy-MM-dd";

    // ── Badge CSS Classes (Bootstrap 5) ───────────────────────────────────────
    // Used in Views to render colored badges without logic in Razor files.

    public static string GetStatusBadgeClass(string status) => status switch
    {
        Status_Todo => "bg-secondary",
        Status_InProgress => "bg-primary",
        Status_InReview => "bg-warning text-dark",
        Status_Completed => "bg-success",
        _ => "bg-secondary"
    };

    public static string GetPriorityBadgeClass(string priority) => priority switch
    {
        Priority_Low => "bg-success",
        Priority_Medium => "bg-info text-dark",
        Priority_High => "bg-warning text-dark",
        Priority_Critical => "bg-danger",
        _ => "bg-secondary"
    };
}