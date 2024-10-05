

namespace Lyvads.Application.Dtos.SuperAdminDtos;

public class DashboardSummaryDto
{
    public int TotalCreators { get; set; }
    public int TotalRegularUsers { get; set; }
    public int TotalSignups { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalSuperAdmins { get; set; }
    public double ImpressionPercentage { get; set; }
}

public class RevenueReportDto
{
    public decimal YearlyRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal WeeklyRevenue { get; set; }
    public decimal DailyRevenue { get; set; }
}

public class TopRequestDto
{
    public string? RegularUser { get; set; }
    public string? RequestType { get; set; }
    public int RequestCount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime TimePeriod { get; set; }
}

public class TopCreatorDto
{
    public string? CreatorName { get; set; }
    public string? Industry { get; set; }
    public int NumberOfCollaborations { get; set; }
    public decimal TotalAmountEarned { get; set; }
}

public class CollaborationStatusReportDto
{
    public int SuccessfulCollaborations { get; set; }
    public int PendingCollaborations { get; set; }
    public int DeclinedCollaborations { get; set; }
    public DateTime TimePeriod { get; set; }
}
