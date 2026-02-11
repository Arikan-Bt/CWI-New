using System;
using System.Linq;
using CWI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CWI.Domain.Entities.Identity;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = "Server=localhost;Database=ArikanCWIDB;User ID=sa;Password=GuchluBirSifre123!;Encrypt=False;TrustServerCertificate=True;Integrated Security=False;";

builder.Services.AddDbContext<CWIDbContext>(options =>
    options.UseSqlServer(connectionString));

using var host = builder.Build();

using var scope = host.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<CWIDbContext>();

var user = await context.Set<User>().AsNoTracking()
    .Include(u => u.Role)
    .FirstOrDefaultAsync(u => u.UserName == "berksimsek");

if (user == null)
{
    Console.WriteLine("User 'berksimsek' not found.");
    var allUsers = await context.Set<User>().AsNoTracking().Select(u => u.UserName).ToListAsync();
    Console.WriteLine("All users: " + string.Join(", ", allUsers));
}
else
{
    Console.WriteLine($"User found: {user.UserName}, IsActive: {user.IsActive}, RoleId: {user.RoleId}");
    if (user.Role == null)
    {
        Console.WriteLine("Role is NULL (this would cause the INNER JOIN to fail if using Include)");
        var role = await context.Set<Role>().AsNoTracking().FirstOrDefaultAsync(r => r.Id == user.RoleId);
        if (role == null)
        {
            Console.WriteLine($"Role with ID {user.RoleId} NOT FOUND in database.");
        }
        else
        {
            Console.WriteLine($"Role with ID {user.RoleId} found: {role.Name}, IsActive: {role.IsActive}");
        }
    }
    else
    {
        Console.WriteLine($"Role found: {user.Role.Name}, IsActive: {user.Role.IsActive}");
    }
}
