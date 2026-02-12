using CWI.Application.Interfaces.Services;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Inventory;
using CWI.Domain.Entities.Purchasing;
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
        var transactionRepo = _unitOfWork.Repository<CustomerTransaction, long>();
        var vendorInvoiceRepo = _unitOfWork.Repository<VendorInvoice, int>();
        var vendorPaymentRepo = _unitOfWork.Repository<VendorPayment, long>();
        var inventoryRepo = _unitOfWork.Repository<InventoryItem, long>();

        var now = DateTime.UtcNow;
        var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
        var lastTwelveMonthsStart = startOfCurrentMonth.AddMonths(-11);

        var yearlyRevenueMonthly = await orderRepo.AsQueryable()
            .AsNoTracking()
            .Where(o => !o.IsCanceled && o.OrderedAt >= lastTwelveMonthsStart)
            .GroupBy(o => new { o.OrderedAt.Year, o.OrderedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Total = g.Sum(o => o.GrandTotal)
            })
            .ToListAsync(cancellationToken);

        var yearlyLabels = new List<string>();
        var yearlyValues = new List<double>();
        for (var i = 11; i >= 0; i--)
        {
            var date = startOfCurrentMonth.AddMonths(-i);
            var monthData = yearlyRevenueMonthly.FirstOrDefault(x => x.Year == date.Year && x.Month == date.Month);
            yearlyLabels.Add(date.ToString("MMM yy", CultureInfo.InvariantCulture));
            yearlyValues.Add(monthData != null ? (double)monthData.Total : 0);
        }

        var seasonalRevenue = await orderRepo.AsQueryable()
            .AsNoTracking()
            .Where(o => !o.IsCanceled && o.OrderedAt >= lastTwelveMonthsStart)
            .GroupBy(o => string.IsNullOrWhiteSpace(o.Season) ? "Unspecified" : o.Season!)
            .Select(g => new
            {
                Season = g.Key,
                Total = g.Sum(o => o.GrandTotal)
            })
            .OrderBy(x => x.Season)
            .ToListAsync(cancellationToken);

        var twelveMonthsAgo = lastTwelveMonthsStart;
        var monthlySales = await orderRepo.AsQueryable()
            .AsNoTracking()
            .Where(o => o.OrderedAt >= twelveMonthsAgo && !o.IsCanceled)
            .GroupBy(o => new { o.OrderedAt.Year, o.OrderedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Total = g.Sum(o => o.GrandTotal)
            })
            .ToListAsync(cancellationToken);

        var monthlyLabels = new List<string>();
        var monthlyValues = new List<double>();
        for (var i = 11; i >= 0; i--)
        {
            var date = startOfCurrentMonth.AddMonths(-i);
            var monthData = monthlySales.FirstOrDefault(x => x.Year == date.Year && x.Month == date.Month);
            monthlyLabels.Add(date.ToString("MMM yy", CultureInfo.InvariantCulture));
            monthlyValues.Add(monthData != null ? (double)monthData.Total : 0);
        }

        var shippedQuantity = await orderRepo.AsQueryable()
            .AsNoTracking()
            .Where(o => !o.IsCanceled && o.Status == OrderStatus.Shipped)
            .SumAsync(o => o.TotalQuantity, cancellationToken);

        var pendingQuantity = await orderRepo.AsQueryable()
            .AsNoTracking()
            .Where(o => !o.IsCanceled && (
                o.Status == OrderStatus.Pending ||
                o.Status == OrderStatus.PreOrder ||
                o.Status == OrderStatus.Approved))
            .SumAsync(o => o.TotalQuantity, cancellationToken);

        var waitingQuantity = await orderRepo.AsQueryable()
            .AsNoTracking()
            .Where(o => !o.IsCanceled && o.Status == OrderStatus.PackedAndWaitingShipment)
            .SumAsync(o => o.TotalQuantity, cancellationToken);

        var customerBalanceSummary = await transactionRepo.AsQueryable()
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalDebit = g.Sum(x => x.DebitAmount),
                TotalCredit = g.Sum(x => x.CreditAmount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalVendorInvoice = await vendorInvoiceRepo.AsQueryable()
            .AsNoTracking()
            .SumAsync(x => x.TotalAmount, cancellationToken);

        var totalVendorPayment = await vendorPaymentRepo.AsQueryable()
            .AsNoTracking()
            .SumAsync(x => x.Amount, cancellationToken);

        var stockSummary = await inventoryRepo.AsQueryable()
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                OnHand = g.Sum(x => x.QuantityOnHand),
                Reserved = g.Sum(x => x.QuantityReserved)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalDebit = customerBalanceSummary?.TotalDebit ?? 0;
        var totalCredit = customerBalanceSummary?.TotalCredit ?? 0;
        var salesBalancePayment = totalCredit;
        var customerBalance = totalDebit - totalCredit;
        var vendorBalance = totalVendorInvoice - totalVendorPayment;

        var stockTotalQuantity = stockSummary?.OnHand ?? 0;
        var reservedQuantity = stockSummary?.Reserved ?? 0;
        var availableQuantity = stockTotalQuantity - reservedQuantity;

        return new List<DashboardWidgetDto>
        {
            new DashboardWidgetDto
            {
                Id = "admin-yearly-revenue",
                Type = "Chart",
                Title = "Yearly Revenue",
                Order = 1,
                Width = 4,
                Data = new ChartData
                {
                    Labels = yearlyLabels,
                    Values = yearlyValues,
                    ChartType = "bar"
                }
            },
            new DashboardWidgetDto
            {
                Id = "admin-seasonal-revenue",
                Type = "Chart",
                Title = "Seasonal Revenue",
                Order = 2,
                Width = 4,
                Data = new ChartData
                {
                    Labels = seasonalRevenue.Select(x => x.Season).ToList(),
                    Values = seasonalRevenue.Select(x => (double)x.Total).ToList(),
                    ChartType = "bar"
                }
            },
            new DashboardWidgetDto
            {
                Id = "admin-sales-overview-6m",
                Type = "Chart",
                Title = "Sales Overview (Last 12 Months)",
                Order = 3,
                Width = 4,
                Data = new ChartData
                {
                    Labels = monthlyLabels,
                    Values = monthlyValues,
                    ChartType = "bar"
                }
            },
            new DashboardWidgetDto
            {
                Id = "admin-shipped-qty",
                Type = "StatCard",
                Title = "Shipped Quantity",
                Order = 4,
                Width = 3,
                Data = new StatCardData
                {
                    Value = shippedQuantity.ToString("N0", usCulture),
                    Trend = "Status",
                    TrendDirection = "Neutral",
                    Description = "Shipped",
                    Icon = "pi pi-send"
                }
            },
            new DashboardWidgetDto
            {
                Id = "admin-pending-qty",
                Type = "StatCard",
                Title = "Pending Quantity",
                Order = 5,
                Width = 3,
                Data = new StatCardData
                {
                    Value = pendingQuantity.ToString("N0", usCulture),
                    Trend = "Status",
                    TrendDirection = "Neutral",
                    Description = "Pending / PreOrder / Approved",
                    Icon = "pi pi-clock"
                }
            },
            new DashboardWidgetDto
            {
                Id = "admin-waiting-qty",
                Type = "StatCard",
                Title = "Waiting Quantity",
                Order = 6,
                Width = 3,
                Data = new StatCardData
                {
                    Value = waitingQuantity.ToString("N0", usCulture),
                    Trend = "Status",
                    TrendDirection = "Neutral",
                    Description = "Packed & Waiting Shipment",
                    Icon = "pi pi-box"
                }
            },
            new DashboardWidgetDto
            {
                Id = "admin-balance-stock-overview",
                Type = "CompositeKpi",
                Title = "Balance & Stock Overview",
                Order = 7,
                Width = 12,
                Data = new CompositeKpiData
                {
                    Sections =
                    {
                        new CompositeKpiSection
                        {
                            Title = "Balance",
                            Items =
                            {
                                new CompositeKpiItem
                                {
                                    Label = "Sales Balance Payment",
                                    Value = salesBalancePayment.ToString("C2", usCulture),
                                    Icon = "pi pi-money-bill",
                                    Trend = "Collected",
                                    TrendDirection = "Neutral"
                                },
                                new CompositeKpiItem
                                {
                                    Label = "Vendor Balance",
                                    Value = vendorBalance.ToString("C2", usCulture),
                                    Icon = "pi pi-briefcase",
                                    Trend = "Open",
                                    TrendDirection = "Neutral"
                                },
                                new CompositeKpiItem
                                {
                                    Label = "Customer Balance",
                                    Value = customerBalance.ToString("C2", usCulture),
                                    Icon = "pi pi-users",
                                    Trend = "Outstanding",
                                    TrendDirection = "Neutral"
                                }
                            }
                        },
                        new CompositeKpiSection
                        {
                            Title = "Stock",
                            Items =
                            {
                                new CompositeKpiItem
                                {
                                    Label = "Stock Total Quantity",
                                    Value = stockTotalQuantity.ToString("N0", usCulture),
                                    Icon = "pi pi-database",
                                    Trend = "On Hand",
                                    TrendDirection = "Neutral"
                                },
                                new CompositeKpiItem
                                {
                                    Label = "Reserved Quantity",
                                    Value = reservedQuantity.ToString("N0", usCulture),
                                    Icon = "pi pi-lock",
                                    Trend = "Reserved",
                                    TrendDirection = "Neutral"
                                },
                                new CompositeKpiItem
                                {
                                    Label = "Available Quantity",
                                    Value = availableQuantity.ToString("N0", usCulture),
                                    Icon = "pi pi-check-circle",
                                    Trend = "Available",
                                    TrendDirection = "Neutral"
                                }
                            }
                        }
                    }
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
            .AsNoTracking()
            .Where(t => t.CustomerId == customerId)
            .GroupBy(t => t.CustomerId)
            .Select(g => new { Debit = g.Sum(t => t.DebitAmount), Credit = g.Sum(t => t.CreditAmount) })
            .FirstOrDefaultAsync(cancellationToken);
        
        decimal balance = balanceData != null ? balanceData.Debit - balanceData.Credit : 0;

        // 2. Active Orders
        var activeOrderCount = await orderRepo.AsQueryable()
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.PreOrder || o.Status == OrderStatus.PackedAndWaitingShipment))
            .CountAsync(cancellationToken);

        // 3. Total Spent (Lifetime Value)
        var totalSpent = await orderRepo.AsQueryable()
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId && !o.IsCanceled)
            .SumAsync(o => o.GrandTotal, cancellationToken);

        // 4. Recent Orders
        var recentOrders = await orderRepo.AsQueryable()
            .AsNoTracking()
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
