using System.Collections;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Common;
using CWI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace CWI.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementasyonu
/// Tüm repository'leri tek bir transaction altında yönetir
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly CWIDbContext _context;
    private readonly Hashtable _repositories;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;
    
    public UnitOfWork(CWIDbContext context)
    {
        _context = context;
        _repositories = new Hashtable();
    }
    
    /// <inheritdoc />
    public bool HasActiveTransaction => _currentTransaction != null;
    
    /// <inheritdoc />
    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity).Name;
        
        if (_repositories.ContainsKey(type))
        {
            return (IGenericRepository<TEntity>)_repositories[type]!;
        }
        
        var repositoryType = typeof(GenericRepository<>);
        var repositoryInstance = Activator.CreateInstance(
            repositoryType.MakeGenericType(typeof(TEntity)), 
            _context);
        
        _repositories.Add(type, repositoryInstance);
        
        return (IGenericRepository<TEntity>)_repositories[type]!;
    }
    
    /// <inheritdoc />
    public IGenericRepository<TEntity, TId> Repository<TEntity, TId>() 
        where TEntity : BaseEntity<TId>
        where TId : struct
    {
        var type = $"{typeof(TEntity).Name}_{typeof(TId).Name}";
        
        if (_repositories.ContainsKey(type))
        {
            return (IGenericRepository<TEntity, TId>)_repositories[type]!;
        }
        
        var repositoryType = typeof(GenericRepository<,>);
        var repositoryInstance = Activator.CreateInstance(
            repositoryType.MakeGenericType(typeof(TEntity), typeof(TId)), 
            _context);
        
        _repositories.Add(type, repositoryInstance);
        
        return (IGenericRepository<TEntity, TId>)_repositories[type]!;
    }
    
    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Zaten aktif bir transaction var.");
        }
        
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("Commit edilecek aktif transaction yok.");
        }
        
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
    
    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            return;
        }
        
        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
        _disposed = true;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
