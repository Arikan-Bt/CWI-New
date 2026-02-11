using System.Text.Json.Serialization;

namespace CWI.Application.Features.Dashboard.Queries.GetDashboard;

public class DashboardViewModel
{
    public string Role { get; set; } = string.Empty;
    public List<DashboardWidgetDto> Widgets { get; set; } = new();
}

public class DashboardWidgetDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., "StatCard", "Chart", "Table", "Banner"
    public string Title { get; set; } = string.Empty;
    public int Width { get; set; } = 12; // Bootstrap col-width (1-12)
    public int Height { get; set; } = 1; // Row span height
    public int Order { get; set; }
    public object? Data { get; set; } // The actual payload for the widget
}

public class StatCardData
{
    public string Value { get; set; } = string.Empty;
    public string Trend { get; set; } = string.Empty; // "+10%", "-5%"
    public string TrendDirection { get; set; } = "Neutral"; // "Up", "Down", "Neutral"
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class ChartData
{
    public List<string> Labels { get; set; } = new();
    public List<double> Values { get; set; } = new();
    public string ChartType { get; set; } = "bar"; // "bar", "line", "pie"
}

public class TableData
{
    public List<string> Headers { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
}
