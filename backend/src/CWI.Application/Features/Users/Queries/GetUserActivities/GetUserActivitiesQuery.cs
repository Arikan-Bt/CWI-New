using CWI.Application.DTOs.Users;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Identity;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Purchasing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Users.Queries.GetUserActivities;

public record GetUserActivitiesQuery(int UserId) : IRequest<List<UserActivityDto>>;

public class GetUserActivitiesHandler : IRequestHandler<GetUserActivitiesQuery, List<UserActivityDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserActivitiesHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<UserActivityDto>> Handle(GetUserActivitiesQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>()
            .AsQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null) return new List<UserActivityDto>();

        // 1. Sales Orders (Siparişler)
        // Kullanıcının oluşturduğu veya bağlı olduğu cariye ait siparişler
        var salesOrders = await _unitOfWork.Repository<Order, long>()
            .AsQueryable()
            .AsNoTracking()
            .Include(o => o.Currency)
            .Where(o => o.CreatedByUsername == user.UserName || (user.LinkedCustomerId.HasValue && o.CustomerId == user.LinkedCustomerId.Value))
            .OrderByDescending(o => o.OrderedAt)
            .Take(100)
            .Select(o => new UserActivityDto
            {
                Date = o.OrderedAt,
                Operation = "Sales Order",
                Description = $"Order #{o.OrderNumber} - {o.TotalQuantity} items - {o.GrandTotal} {o.Currency.Code}",
                Status = o.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // 2. Purchase Orders (Satın Almalar)
        // Eğer kullanıcı bir tedarikçiye bağlıysa ona ait satın alma siparişleri
        var purchaseOrders = new List<UserActivityDto>();
        if (user.LinkedCustomerId.HasValue)
        {
            purchaseOrders = await _unitOfWork.Repository<PurchaseOrder, long>()
                .AsQueryable()
                .AsNoTracking()
                .Where(po => po.SupplierId == user.LinkedCustomerId.Value)
                .OrderByDescending(po => po.OrderedAt)
                .Take(100)
                .Select(po => new UserActivityDto
                {
                    Date = po.OrderedAt,
                    Operation = "Purchase Order",
                    Description = $"P.Order #{po.OrderNumber} - {po.TotalQuantity} items - {po.TotalAmount}",
                    Status = po.IsReceived ? "Received" : "Pending"
                })
                .ToListAsync(cancellationToken);
        }

        // Birleştir ve tarihe göre sırala
        return salesOrders
            .Concat(purchaseOrders)
            .OrderByDescending(a => a.Date)
            .Take(100)
            .ToList();

    }
}
