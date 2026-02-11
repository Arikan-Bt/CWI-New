using System.Linq.Expressions;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Common;
using CWI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CWI.Infrastructure.Repositories;

/// <summary>
/// Generic Repository implementasyonu
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
/// <typeparam name="TId">Primary key tipi</typeparam>
public class GenericRepository<TEntity, TId> : IGenericRepository<TEntity, TId>
    where TEntity : BaseEntity<TId>
    where TId : struct
{
    protected readonly CWIDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    
    public GenericRepository(CWIDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }
    
    #region Okuma İşlemleri (Read)
    
    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }
    
    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        List<Expression<Func<TEntity, object>>>? includes = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;
        
        if (disableTracking)
            query = query.AsNoTracking();
        
        if (includes != null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }
        
        if (predicate != null)
            query = query.Where(predicate);
        
        if (orderBy != null)
            return await orderBy(query).ToListAsync(cancellationToken);
        
        return await query.ToListAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
    }
    
    /// <inheritdoc />
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await _dbSet.CountAsync(cancellationToken);
        
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }
    
    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }
    
    /// <inheritdoc />
    public virtual async Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();
        
        if (predicate != null)
            query = query.Where(predicate);
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        if (orderBy != null)
            query = orderBy(query);
        
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (items, totalCount);
    }
    
    #endregion
    
    #region Yazma İşlemleri (Write)
    
    /// <inheritdoc />
    public virtual TEntity Add(TEntity entity)
    {
        return _dbSet.Add(entity).Entity;
    }
    
    /// <inheritdoc />
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }
    
    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }
    
    /// <inheritdoc />
    public virtual void Update(TEntity entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }
    
    /// <inheritdoc />
    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
    }
    
    /// <inheritdoc />
    public virtual void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }
    
    /// <inheritdoc />
    public virtual async Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            Delete(entity);
        }
    }
    
    /// <inheritdoc />
    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }
    
    #endregion
    
    #region IQueryable
    
    /// <inheritdoc />
    public virtual IQueryable<TEntity> AsQueryable()
    {
        return _dbSet.AsNoTracking();
    }
    
    /// <inheritdoc />
    public virtual IQueryable<TEntity> AsQueryableTracking()
    {
        return _dbSet;
    }
    
    #endregion
}

/// <summary>
/// Int tipinde ID kullanan entity'ler için generic repository
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
public class GenericRepository<TEntity> : GenericRepository<TEntity, int>, IGenericRepository<TEntity>
    where TEntity : BaseEntity<int>
{
    public GenericRepository(CWIDbContext context) : base(context)
    {
    }
}
