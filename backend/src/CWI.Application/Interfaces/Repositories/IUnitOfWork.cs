namespace CWI.Application.Interfaces.Repositories;

/// <summary>
/// Unit of Work interface'i
/// Tüm repository'leri tek bir transaction altında yönetir
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Generic repository döndürür
    /// </summary>
    /// <typeparam name="TEntity">Entity tipi</typeparam>
    /// <returns>Repository instance</returns>
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : Domain.Common.BaseEntity;
    
    /// <summary>
    /// Belirli ID tipine sahip entity için repository döndürür
    /// </summary>
    /// <typeparam name="TEntity">Entity tipi</typeparam>
    /// <typeparam name="TId">ID tipi</typeparam>
    /// <returns>Repository instance</returns>
    IGenericRepository<TEntity, TId> Repository<TEntity, TId>() 
        where TEntity : Domain.Common.BaseEntity<TId>
        where TId : struct;
    
    /// <summary>
    /// Tüm değişiklikleri veritabanına kaydeder
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Etkilenen satır sayısı</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Transaction başlatır
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Transaction'ı commit eder
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Transaction'ı rollback eder
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Aktif bir transaction var mı?
    /// </summary>
    bool HasActiveTransaction { get; }
}
