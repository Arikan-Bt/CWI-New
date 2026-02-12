using CWI.Application.DTOs.Users;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(int Id) : IRequest<UserDto?>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // AsNoTracking: Sadece okuma amaçlı sorgu, tracking gereksiz
        // WarehouseAccess Include kaldırıldı: DTO'da kullanılmıyor
        var user = await _unitOfWork.Repository<User>().AsQueryable()
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.LinkedCustomer)
            .Include(u => u.BrandAccess)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Name = user.FirstName ?? string.Empty,
            Surname = user.LastName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = user.Role.Name,
            RoleId = user.RoleId,
            Status = user.IsActive ? "Active" : "Inactive",
            ClientCode = user.UserName,
            MobilePhone = user.PhoneNumber,
            ProjectType = user.ProjectType,
            CurrentAccount = user.LinkedCustomer?.Code,
            AllowedBrands = user.BrandAccess.Select(b => b.BrandId).ToList(),
            RestrictedBrands = !string.IsNullOrEmpty(user.RestrictedBrands) 
                ? user.RestrictedBrands.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList() 
                : new List<int>(),
            AllowedProducts = !string.IsNullOrEmpty(user.AllowedProducts)
                ? user.AllowedProducts.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>(),
            BlockedProducts = !string.IsNullOrEmpty(user.BlockedProducts)
                ? user.BlockedProducts.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>()
        };
    }
}
