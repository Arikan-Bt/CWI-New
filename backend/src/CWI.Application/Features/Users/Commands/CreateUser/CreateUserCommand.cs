using CWI.Application.Common.Caching;
using CWI.Application.DTOs.Users;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Identity;
using MediatR;

namespace CWI.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(CreateUserRequest Request) : IRequest<int>, IInvalidatesCache
{
    public IReadOnlyCollection<string> CachePrefixesToInvalidate => [CachePrefixes.LookupUsers];
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<int> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var existingUser = await _unitOfWork.Repository<User>().FirstOrDefaultAsync(u => u.UserName == request.ClientCode, cancellationToken);
        if (existingUser != null)
            throw new Exception("This user code is already in use.");

        var user = new User
        {
            UserName = request.ClientCode ?? request.Email,
            Email = request.Email,
            FirstName = request.Name,
            LastName = request.Surname,
            PasswordHash = _authService.HashPassword(request.Password),
            RoleId = request.RoleId,
            PhoneNumber = request.MobilePhone,
            IsActive = request.Status == "Active",
            ProjectType = CWI.Domain.Enums.ProjectType.CWI,
            CreatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(request.CurrentAccount))
        {
            var customer = await _unitOfWork.Repository<CWI.Domain.Entities.Customers.Customer>()
                .FirstOrDefaultAsync(c => c.Code == request.CurrentAccount, cancellationToken);
            if (customer != null)
            {
                user.LinkedCustomerId = customer.Id;
            }
        }

        if (request.AllowedBrands != null && request.AllowedBrands.Any())
        {
            foreach (var brandId in request.AllowedBrands)
            {
                user.BrandAccess.Add(new UserBrandAccess
                {
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

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
