using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Queries;

public record GetPackingListQuery(long OrderId) : IRequest<PackingListDto>;

public class PackingListDto
{
    public List<PackingListItemDto> Items { get; set; } = new();
    public List<PackingListCartonDto> Cartons { get; set; } = new();
}

public class PackingListItemDto
{
    public long OrderItemId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Qty { get; set; }
    public string CartonNo { get; set; } = string.Empty;
}

public class PackingListCartonDto
{
    public long Id { get; set; } // OrderPackageId
    public string CartonNo { get; set; } = string.Empty;
    public decimal? NetWeight { get; set; }
    public decimal? GrossWeight { get; set; }
    public string Measurements { get; set; } = string.Empty;
}

public class GetPackingListQueryHandler : IRequestHandler<GetPackingListQuery, PackingListDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPackingListQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PackingListDto> Handle(GetPackingListQuery request, CancellationToken cancellationToken)
    {
        // 1. Get Order Items
        var orderItems = await _unitOfWork.Repository<OrderItem, long>().AsQueryable()
            .Include(x => x.Product)
            .Where(x => x.OrderId == request.OrderId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // 2. Get Cartons (OrderPackages) and their items
        var cartons = await _unitOfWork.Repository<OrderPackage, long>().AsQueryable()
            .Include(x => x.Items)
            .Where(x => x.OrderId == request.OrderId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Build a lookup for Item -> Carton(s)
        var itemCartonMap = new Dictionary<long, List<string>>();
        foreach (var carton in cartons)
        {
            foreach (var pkgItem in carton.Items)
            {
                if (!itemCartonMap.ContainsKey(pkgItem.OrderItemId))
                {
                    itemCartonMap[pkgItem.OrderItemId] = new List<string>();
                }
                itemCartonMap[pkgItem.OrderItemId].Add(carton.PackageNumber);
            }
        }

        // 3. Map Items
        var result = new PackingListDto();
        
        result.Items = orderItems.Select(item => new PackingListItemDto
        {
            OrderItemId = item.Id,
            ProductCode = item.Product.Sku, // Code is Sku in Product entity
            ProductName = item.ProductName,
            Qty = item.Quantity,
            CartonNo = itemCartonMap.ContainsKey(item.Id) 
                ? string.Join(", ", itemCartonMap[item.Id]) 
                : ""
        }).ToList();

        // 4. Map Cartons
        result.Cartons = cartons.Select(c => new PackingListCartonDto
        {
            Id = c.Id,
            CartonNo = c.PackageNumber,
            NetWeight = c.NetWeight,
            GrossWeight = c.GrossWeight,
            Measurements = FormatMeasurements(c.Length, c.Width, c.Height)
        }).OrderBy(c => c.CartonNo).ToList();

        return result;
    }

    private string FormatMeasurements(decimal? l, decimal? w, decimal? h)
    {
        if (!l.HasValue && !w.HasValue && !h.HasValue) return "00*00*00";
        return $"{(l ?? 0):00}*{(w ?? 0):00}*{(h ?? 0):00}";
    }
}
