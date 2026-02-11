using CWI.Application.DTOs.Users;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Identity;
using CWI.Domain.Entities.System;
using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(UpdateUserRequest Request) : IRequest<bool>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<bool> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var user = await _unitOfWork.Repository<User>().AsQueryable()
            .Include(u => u.BrandAccess)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        
        if (user == null) return false;

        user.FirstName = request.Name;
        user.LastName = request.Surname;
        user.Email = request.Email;
        user.RoleId = request.RoleId;
        user.PhoneNumber = request.MobilePhone;
        user.IsActive = request.Status == "Active";
        user.UpdatedAt = DateTime.UtcNow;

        // Bağlı cari güncelle
        if (!string.IsNullOrEmpty(request.CurrentAccount))
        {
            var customer = await _unitOfWork.Repository<CWI.Domain.Entities.Customers.Customer>()
                .FirstOrDefaultAsync(c => c.Code == request.CurrentAccount, cancellationToken);
            user.LinkedCustomerId = customer?.Id;
        }
        else
        {
            user.LinkedCustomerId = null;
        }

        // Marka erişimlerini senkronize et (Sync)
        user.BrandAccess.Clear();
        if (request.AllowedBrands != null && request.AllowedBrands.Any())
        {
            foreach (var brandId in request.AllowedBrands)
            {
                user.BrandAccess.Add(new UserBrandAccess
                {
                    UserId = user.Id,
                    BrandId = brandId,
                    GrantedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }
        }

        user.RestrictedBrands = request.RestrictedBrands != null && request.RestrictedBrands.Any()
            ? string.Join(",", request.RestrictedBrands)
            : null;

        user.AllowedProducts = request.AllowedProducts != null && request.AllowedProducts.Any()
            ? string.Join(",", request.AllowedProducts)
            : null;

        user.BlockedProducts = request.BlockedProducts != null && request.BlockedProducts.Any()
            ? string.Join(",", request.BlockedProducts)
            : null;

        if (!string.IsNullOrWhiteSpace(request.Password) && request.Password != "••••••")
        {
            user.PasswordHash = _authService.HashPassword(request.Password);
        }

        _unitOfWork.Repository<User>().Update(user);

        // Aktivite logu kaydet
        var activityLog = new ApplicationLog
        {
            Level = "Information",
            Message = $"User updated: {user.UserName} ({user.FullName})",
            Source = "UpdateUserCommandHandler",
            UserName = "System Admin", // Normalde ICurrentUserService'den alınmalı
            LoggedAt = DateTime.UtcNow
        };
        _unitOfWork.Repository<ApplicationLog, long>().Add(activityLog);

        await _unitOfWork.SaveChangesAsync(cancellationToken);


        return true;
    }
}
