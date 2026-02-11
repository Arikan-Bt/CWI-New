using System.Linq;
using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Identity;

namespace CWI.Application.Features.Reports.Queries;

public class GetOrdersReportQuery : IRequest<OrdersReportResponse>
{
    public OrdersReportRequest Request { get; set; } = null!;

    public class GetOrdersReportQueryHandler : IRequestHandler<GetOrdersReportQuery, OrdersReportResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetOrdersReportQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<OrdersReportResponse> Handle(GetOrdersReportQuery query, CancellationToken cancellationToken)
        {
            var filters = query.Request;
            
            // Sipariş kalemleri üzerinden sorgulama yaparak düzleştirilmiş (flattened) yapıyı veritabanı seviyesinde oluşturuyoruz.
            // Bu sayede sayfalama ve sıralama doğru çalışır.
            // NOT: Include kullanılmadan direkt IQueryable üzerinde çalışıyoruz - EF Core projection/GroupBy için bunu tercih eder.
            var orderItemRepo = _unitOfWork.Repository<OrderItem, long>();
            var queryable = orderItemRepo.AsQueryable()
                .AsNoTracking()
                .Where(i => i.Order != null);

            // Yetki kontrolleri
            if (!_currentUserService.IsAdministrator)
            {
                if (_currentUserService.ProjectType.HasValue)
                {
                    var projectCode = _currentUserService.ProjectType.Value.ToString();
                    queryable = queryable.Where(i => i.Order.CreatedByGroupCode == projectCode);
                }

                if (_currentUserService.LinkedCustomerId.HasValue)
                {
                    queryable = queryable.Where(i => i.Order.CustomerId == _currentUserService.LinkedCustomerId.Value);
                }
            }

            // Filtreler
            if (!string.IsNullOrEmpty(filters.CurrentAccountCode))
            {
                queryable = queryable.Where(i => i.Order.Customer != null && i.Order.Customer.Code == filters.CurrentAccountCode);
            }

            if (!string.IsNullOrEmpty(filters.OrderStatus))
            {
                // Sipariş durumu enum kontrolü
                if (Enum.TryParse<CWI.Domain.Enums.OrderStatus>(filters.OrderStatus, out var status))
                {
                    queryable = queryable.Where(i => i.Order.Status == status);
                }
            }

            if (filters.StartDate.HasValue)
            {
                queryable = queryable.Where(i => i.Order.OrderedAt >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                queryable = queryable.Where(i => i.Order.OrderedAt <= filters.EndDate.Value);
            }


            if (!string.IsNullOrEmpty(filters.Brand))
            {
                if (filters.Brand == "Unknown")
                {
                    queryable = queryable.Where(i => i.Product == null || i.Product.Brand == null);
                }
                else
                {
                    queryable = queryable.Where(i => i.Product != null && i.Product.Brand != null && i.Product.Brand.Name == filters.Brand);
                }
            }


            // GroupBy ile veriyi Sipariş + Marka bazında grupluyoruz.
            // EF Core 8/9 karmaşık GroupBy sorgularını destekler.
            // Ancak her ihtimale karşı Select ile projeksiyon yapıp sonra işlem yapmak daha güvenli olabilir.
            // Burada önce gruplama yapıp Count alacağız.

            var groupedQuery = queryable
                .GroupBy(x => new 
                { 
                    x.Order.Id, 
                    BrandName = x.Product != null && x.Product.Brand != null ? x.Product.Brand.Name : "Unknown" 
                })
                .Select(g => new
                {
                    OrderId = g.Key.Id,
                    Brand = g.Key.BrandName,
                    
                    // Flattened properties to avoid Entity materialization issues in GroupBy
                    OrderNumber = g.Select(x => x.Order.OrderNumber).FirstOrDefault(),
                    OrderStatus = g.Select(x => x.Order.Status).FirstOrDefault(),
                    OrderDate = g.Select(x => x.Order.OrderedAt).FirstOrDefault(),
                    RequestedShipmentDate = g.Select(x => x.Order.ShippedAt).FirstOrDefault(),
                    OrderDescription = g.Select(x => x.Order.Notes).FirstOrDefault(),
                    
                    CustomerCode = g.Select(x => x.Order != null && x.Order.Customer != null ? x.Order.Customer.Code : null).FirstOrDefault(),
                    CustomerName = g.Select(x => x.Order != null && x.Order.Customer != null ? x.Order.Customer.Name : null).FirstOrDefault(),
                    CustAddr1 = g.Select(x => x.Order != null && x.Order.Customer != null ? x.Order.Customer.AddressLine1 : null).FirstOrDefault(),
                    CustAddr2 = g.Select(x => x.Order != null && x.Order.Customer != null ? x.Order.Customer.AddressLine2 : null).FirstOrDefault(),
                    CustTown = g.Select(x => x.Order != null && x.Order.Customer != null ? x.Order.Customer.Town : null).FirstOrDefault(),
                    CustCity = g.Select(x => x.Order != null && x.Order.Customer != null ? x.Order.Customer.City : null).FirstOrDefault(),
                    CustCountry = g.Select(x => x.Order != null && x.Order.Customer != null ? x.Order.Customer.Country : null).FirstOrDefault(),
                    
                    PaymentType = g.Select(x => x.Order != null && x.Order.ShippingInfo != null ? x.Order.ShippingInfo.PaymentMethod : null).FirstOrDefault(),
                    ShipmentMethod = g.Select(x => x.Order != null && x.Order.ShippingInfo != null ? x.Order.ShippingInfo.ShipmentTerms : null).FirstOrDefault(),

                    // Agregasyonlar
                    TotalQty = g.Sum(x => x.Quantity),
                    Discount = g.Sum(x => x.DiscountAmount),
                    Total = g.Sum(x => x.LineTotal),
                });

            // Toplam Kayıt Sayısı (Gruplanmış satır sayısı)
            var totalCount = await groupedQuery.CountAsync(cancellationToken);

            // Sıralama
            if (!string.IsNullOrEmpty(filters.SortField))
            {
                bool isAsc = filters.SortOrder == 1;
                switch (filters.SortField)
                {
                    case "currentAccountDescription":
                        groupedQuery = isAsc ? groupedQuery.OrderBy(x => x.CustomerName) : groupedQuery.OrderByDescending(x => x.CustomerName);
                        break;
                    case "orderDetails":
                        groupedQuery = isAsc ? groupedQuery.OrderBy(x => x.OrderNumber) : groupedQuery.OrderByDescending(x => x.OrderNumber);
                        break;
                    case "totalQty":
                        groupedQuery = isAsc ? groupedQuery.OrderBy(x => x.TotalQty) : groupedQuery.OrderByDescending(x => x.TotalQty);
                        break;
                    case "discount":
                        groupedQuery = isAsc ? groupedQuery.OrderBy(x => x.Discount) : groupedQuery.OrderByDescending(x => x.Discount);
                        break;
                    case "total":
                        groupedQuery = isAsc ? groupedQuery.OrderBy(x => x.Total) : groupedQuery.OrderByDescending(x => x.Total);
                        break;
                    case "status":
                        groupedQuery = isAsc ? groupedQuery.OrderBy(x => x.OrderStatus) : groupedQuery.OrderByDescending(x => x.OrderStatus);
                        break;
                    case "brand":
                        groupedQuery = isAsc ? groupedQuery.OrderBy(x => x.Brand) : groupedQuery.OrderByDescending(x => x.Brand);
                        break;
                    case "orderDate":
                        groupedQuery = isAsc ? groupedQuery.OrderBy(x => x.OrderDate) : groupedQuery.OrderByDescending(x => x.OrderDate);
                        break;
                    case "requestedShipmentDate":
                        groupedQuery = isAsc ? groupedQuery.OrderBy(x => x.RequestedShipmentDate) : groupedQuery.OrderByDescending(x => x.RequestedShipmentDate);
                        break;
                    default:
                        groupedQuery = groupedQuery.OrderByDescending(x => x.OrderDate);
                        break;
                }
            }
            else
            {
                groupedQuery = groupedQuery.OrderByDescending(x => x.OrderDate);
            }

            // Sayfalama
            var pagedData = await groupedQuery
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync(cancellationToken);

            // DTO'ya dönüştürme (Memory'de)
            var reportItems = pagedData.Select(x => new OrderReportDto
            {
                OrderId = x.OrderId,
                CurrentAccountCode = x.CustomerCode ?? "-",
                CurrentAccountDescription = x.CustomerName ?? "Unknown Customer",
                OrderDetails = x.OrderNumber ?? "-",
                Status = x.OrderStatus switch
                {
                    CWI.Domain.Enums.OrderStatus.Draft => "Draft",
                    CWI.Domain.Enums.OrderStatus.Pending => "Pending",
                    CWI.Domain.Enums.OrderStatus.Approved => "Approved",
                    CWI.Domain.Enums.OrderStatus.Shipped => "Shipped",
                    CWI.Domain.Enums.OrderStatus.Canceled => "Canceled",
                    CWI.Domain.Enums.OrderStatus.PreOrder => "Pre Order",
                    CWI.Domain.Enums.OrderStatus.PackedAndWaitingShipment => "Packed & Waiting Shipment",
                    _ => x.OrderStatus.ToString()
                },
                Brand = x.Brand,
                OrderDate = x.OrderDate,
                RequestedShipmentDate = x.RequestedShipmentDate,
                TotalQty = x.TotalQty,
                Discount = x.Discount,
                Total = x.Total,
                
                // Ek alanlar
                Address = $"{x.CustAddr1} {x.CustAddr2} {x.CustTown} {x.CustCity} {x.CustCountry}".Trim(),
                PaymentType = x.PaymentType,
                ShipmentMethod = x.ShipmentMethod,
                OrderDescription = x.OrderDescription,
                SubTotal = x.Total + x.Discount,
                GrandTotal = x.Total
            }).ToList();

            // Markalar listesi için ayrı bir sorgu veya mevcut yöntem
            // Eğer Brand filtresi yoksa tüm markaları dönmek isteyebiliriz menü için.
            // Fakat performans için sadece filtrelenmiş verideki markaları değil, 
            // rapor kriterlerine uyan tüm markaları dönmek daha doğru olabilir (tablar için).
            // Ancak bu pahalı bir sorgu olabilir. 
            // Mevcut yapıda sayfadaki markalar dönüyordu ama tablar tüm olası markaları göstermeli mi?
            // "Brands" listesi frontend'de tabları oluşturmak için kullanılıyor.
            // Filtrelerden bağımsız olarak (tarih filtresi hariç) o aralıktaki tüm markaları çekmek mantıklı.

            // Markaları çekmek için ayrı, hafif bir sorgu:
            // Sadece tarih ve ana filtreleri uygulayıp distinct Brand alalım.
            // Brand filtresini buraya UYGULAMIYORUZ ki diğer tabları görebilelim.
            
            var brandQuery = orderItemRepo.AsQueryable().Where(i => i.Order != null);
            
            // Yetki
            if (!_currentUserService.IsAdministrator)
            {
                 if (_currentUserService.ProjectType.HasValue)
                {
                    var projectCode = _currentUserService.ProjectType.Value.ToString();
                    brandQuery = brandQuery.Where(i => i.Order != null && i.Order.CreatedByGroupCode == projectCode);
                    // Marka'nın ProjectType'ını da kontrol et - kullanıcının projesine ait markaları göster
                    brandQuery = brandQuery.Where(i => i.Product != null && i.Product.Brand != null && i.Product.Brand.ProjectType == _currentUserService.ProjectType.Value);
                }
                 if (_currentUserService.LinkedCustomerId.HasValue)
                {
                    brandQuery = brandQuery.Where(i => i.Order != null && i.Order.CustomerId == _currentUserService.LinkedCustomerId.Value);
                }
            }

            // Tarih ve Statü Filtrelerini Brand listesi için de uygulayalım
            if (!string.IsNullOrEmpty(filters.CurrentAccountCode))
                 brandQuery = brandQuery.Where(i => i.Order != null && i.Order.Customer != null && i.Order.Customer.Code == filters.CurrentAccountCode);
            
            if (!string.IsNullOrEmpty(filters.OrderStatus) && Enum.TryParse<CWI.Domain.Enums.OrderStatus>(filters.OrderStatus, out var s))
                 brandQuery = brandQuery.Where(i => i.Order != null && i.Order.Status == s);

            if (filters.StartDate.HasValue) 
                 brandQuery = brandQuery.Where(i => i.Order != null && i.Order.OrderedAt >= filters.StartDate.Value);
            
            if (filters.EndDate.HasValue) 
                 brandQuery = brandQuery.Where(i => i.Order != null && i.Order.OrderedAt <= filters.EndDate.Value);


            var brands = await brandQuery
                .Select(i => i.Product != null && i.Product.Brand != null ? i.Product.Brand.Name : "Unknown")
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync(cancellationToken);


            return new OrdersReportResponse
            {
                Brands = brands,
                Data = reportItems,
                TotalCount = totalCount
            };
        }
    }
}
