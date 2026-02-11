using CWI.Application.Interfaces.Services;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Identity;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Globalization;

namespace CWI.Application.Features.Dashboard.Queries.GetDashboard;

public class GetDashboardQuery : IRequest<DashboardViewModel>
{
}

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardViewModel>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardQueryHandler(ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardViewModel> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var vm = new DashboardViewModel();
        vm.Widgets = new List<DashboardWidgetDto>();

        if (_currentUserService.IsAdministrator)
        {
            vm.Role = "Admin";
            vm.Widgets.AddRange(await GetAdminWidgetsAsync(cancellationToken));
        }
        else if (_currentUserService.LinkedCustomerId.HasValue)
        {
            vm.Role = "Customer";
            vm.Widgets.AddRange(await GetCustomerWidgetsAsync(_currentUserService.LinkedCustomerId.Value, cancellationToken));
        }
        else
        {
            vm.Role = "Guest";
            vm.Widgets.AddRange(GetGuestWidgets());
        }

        return vm;
    }

    private async Task<List<DashboardWidgetDto>> GetAdminWidgetsAsync(CancellationToken cancellationToken)
    {
        var usCulture = new CultureInfo("en-US");
        var orderRepo = _unitOfWork.Repository<Order, long>();
        var userRepo = _unitOfWork.Repository<User>();
        var orderItemRepo = _unitOfWork.Repository<OrderItem, long>();

        // 1. Total Revenue
        var revenue = await orderRepo.AsQueryable()
            .Where(o => !o.IsCanceled)
            .SumAsync(o => o.GrandTotal, cancellationToken);

        // 2. Active Users
        var activeUsers = await userRepo.AsQueryable()
            .CountAsync(u => u.IsActive, cancellationToken);

        // 3. Pending Orders
        var pendingOrders = await orderRepo.AsQueryable()
            .CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.PreOrder, cancellationToken);

        // 4. Top Selling Products
        var topProducts = await orderItemRepo.AsQueryable()
            .GroupBy(x => new { x.ProductId, x.ProductName })
            .Select(g => new 
            {
                ProductName = g.Key.ProductName,
                TotalRevenue = g.Sum(x => x.LineTotal),
                Count = g.Count()
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(5)
            .ToListAsync(cancellationToken);

        var topProductRows = topProducts.Select(p => new List<string> 
        { 
            p.ProductName, 
            "General", 
            p.TotalRevenue.ToString("C2", usCulture) 
        }).ToList();

        // 5. Monthly Sales Chart
        var sixMonthsAgo = DateTime.Now.AddMonths(-5).Date;
        var monthlySales = await orderRepo.AsQueryable()
             .Where(o => o.OrderedAt >= sixMonthsAgo && !o.IsCanceled)
             .GroupBy(o => new { o.OrderedAt.Year, o.OrderedAt.Month })
             .Select(g => new 
             {
                 Year = g.Key.Year,
                 Month = g.Key.Month,
                 Total = g.Sum(o => o.GrandTotal)
             })
             .ToListAsync(cancellationToken);
        
        var chartLabels = new List<string>();
        var chartValues = new List<double>();
        for (int i = 5; i >= 0; i--)
        {
            var date = DateTime.Now.AddMonths(-i);
            var monthData = monthlySales.FirstOrDefault(x => x.Year == date.Year && x.Month == date.Month);
            
            chartLabels.Add(date.ToString("MMM"));
            chartValues.Add(monthData != null ? (double)monthData.Total : 0);
        }

        return new List<DashboardWidgetDto>
        {
            new DashboardWidgetDto
            {
                Id = "admin-revenue-stat",
                Type = "StatCard",
                Title = "Total Revenue",
                Order = 1,
                Width = 3,
                Data = new StatCardData { Value = revenue.ToString("C0", usCulture), Trend = "YTD", TrendDirection = "Neutral", Description = "Total Earnings", Icon = "pi pi-dollar" }
            },
            new DashboardWidgetDto
            {
                Id = "admin-users-stat",
                Type = "StatCard",
                Title = "Active Users",
                Order = 2,
                Width = 3,
                Data = new StatCardData { Value = activeUsers.ToString(), Trend = "Active", TrendDirection = "Neutral", Description = "System Users", Icon = "pi pi-users" }
            },
            new DashboardWidgetDto
            {
                Id = "admin-orders-stat",
                Type = "StatCard",
                Title = "Pending Action",
                Order = 3,
                Width = 3,
                Data = new StatCardData { Value = pendingOrders.ToString(), Trend = "Orders", TrendDirection = pendingOrders > 0 ? "Down" : "Up", Description = "Needs Verification", Icon = "pi pi-shopping-cart" }
            },
            new DashboardWidgetDto
            {
                Id = "admin-system-health",
                Type = "StatCard",
                Title = "System Health",
                Order = 4,
                Width = 3,
                Data = new StatCardData { Value = "98%", Trend = "Stable", TrendDirection = "Neutral", Description = "Uptime", Icon = "pi pi-server" }
            },
            new DashboardWidgetDto
            {
                Id = "admin-sales-chart",
                Type = "Chart",
                Title = "Sales Overview (Last 6 Months)",
                Order = 5,
                Width = 8,
                Data = new ChartData 
                { 
                    Labels = chartLabels,
                    Values = chartValues,
                    ChartType = "bar"
                }
            },
            new DashboardWidgetDto
            {
                Id = "admin-top-products",
                Type = "Table",
                Title = "Top Selling Products",
                Order = 6,
                Width = 4,
                Data = new TableData 
                { 
                    Headers = new() { "Product", "Category", "Revenue" },
                    Rows = topProductRows
                }
            }
        };
    }

    private async Task<List<DashboardWidgetDto>> GetCustomerWidgetsAsync(int customerId, CancellationToken cancellationToken)
    {
        var usCulture = new CultureInfo("en-US");
        var transactionRepo = _unitOfWork.Repository<CustomerTransaction, long>();
        var orderRepo = _unitOfWork.Repository<Order, long>();

        // 1. Balance Calculation
        var balanceData = await transactionRepo.AsQueryable()
            .Where(t => t.CustomerId == customerId)
            .GroupBy(t => t.CustomerId)
            .Select(g => new { Debit = g.Sum(t => t.DebitAmount), Credit = g.Sum(t => t.CreditAmount) })
            .FirstOrDefaultAsync(cancellationToken);
        
        decimal balance = balanceData != null ? balanceData.Debit - balanceData.Credit : 0;

        // 2. Active Orders
        var activeOrderCount = await orderRepo.AsQueryable()
            .Where(o => o.CustomerId == customerId && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.PreOrder || o.Status == OrderStatus.PackedAndWaitingShipment))
            .CountAsync(cancellationToken);

        // 3. Total Spent (Lifetime Value)
        var totalSpent = await orderRepo.AsQueryable()
            .Where(o => o.CustomerId == customerId && !o.IsCanceled)
            .SumAsync(o => o.GrandTotal, cancellationToken);

        // 4. Recent Orders
        var recentOrders = await orderRepo.AsQueryable()
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderedAt)
            .Take(5)
            .Select(o => new { o.OrderNumber, o.OrderedAt, o.Status, o.GrandTotal })
            .ToListAsync(cancellationToken);

        var recentOrderRows = recentOrders.Select(o => new List<string>
        {
            o.OrderNumber,
            o.OrderedAt.ToString("yyyy-MM-dd"),
            o.Status.ToString(),
            o.GrandTotal.ToString("C2", usCulture)
        }).ToList();

        return new List<DashboardWidgetDto>
        {
            new DashboardWidgetDto
            {
                Id = "cust-balance-stat",
                Type = "StatCard",
                Title = "Current Balance",
                Order = 1,
                Width = 4,
                Data = new StatCardData { Value = balance.ToString("C2", usCulture), Trend = "", TrendDirection = "Neutral", Description = "Current Debt", Icon = "pi pi-wallet" }
            },
            new DashboardWidgetDto
            {
                Id = "cust-orders-stat",
                Type = "StatCard",
                Title = "Active Orders",
                Order = 2,
                Width = 4,
                Data = new StatCardData { Value = activeOrderCount.ToString(), Trend = "Processing", TrendDirection = "Neutral", Description = "In Progress", Icon = "pi pi-box" }
            },
             new DashboardWidgetDto
            {
                Id = "cust-spent-stat",
                Type = "StatCard",
                Title = "Total Spent",
                Order = 3,
                Width = 4,
                Data = new StatCardData { Value = totalSpent.ToString("C2", usCulture), Trend = "Lifetime", TrendDirection = "Neutral", Description = "Total Purchases", Icon = "pi pi-shopping-bag" }
            },
            new DashboardWidgetDto
            {
                Id = "cust-history-table",
                Type = "Table",
                Title = "Recent Orders",
                Order = 4,
                Width = 12,
                Data = new TableData 
                { 
                    Headers = new() { "Order #", "Date", "Status", "Total" },
                    Rows = recentOrderRows
                }
            }
        };
    }

    private List<DashboardWidgetDto> GetGuestWidgets()
    {
        return new List<DashboardWidgetDto>
        {
            new DashboardWidgetDto
            {
                Id = "guest-welcome",
                Type = "Banner",
                Title = "Welcome to CWI Portal",
                Order = 1,
                Width = 12,
                Data = new { Message = "Please contact support to link your customer account." }
            }
        };
    }
}
