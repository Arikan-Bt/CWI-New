using System;
using System.Linq;
using CWI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDbContext<CWIDbContext>(options =>
    options.UseSqlServer("Server=localhost;Database=ArikanCWIDB;User=sa;Password=GuchluBirSifre123!;TrustServerCertificate=True"));

using var host = builder.Build();
using var scope = host.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<CWIDbContext>();

var customers = await context.Customers.ToListAsync();
Console.WriteLine($"Total Customers: {customers.Count}");
foreach (var c in customers.Take(10))
{
    Console.WriteLine($"- {c.Code}: {c.Name} (Active: {c.IsActive})");
}

var users = await context.Users.Include(u => u.Role).ToListAsync();
Console.WriteLine("\nUsers:");
foreach (var u in users)
{
    Console.WriteLine($"- {u.UserName}: IsAdmin={u.IsAdministrator}, Role={u.Role?.Name}, LinkedCustomer={u.LinkedCustomerId}");
}
