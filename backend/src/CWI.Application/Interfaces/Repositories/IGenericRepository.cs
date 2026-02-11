using System.Linq.Expressions;
using CWI.Domain.Common;

namespace CWI.Application.Interfaces.Repositories;

/// <summary>
/// Generic repository interface'i
/// Tüm CRUD işlemleri için temel operasyonları tanımlar
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
/// <typeparam name="TId">Primary key tipi</typeparam>
public interface IGenericRepository<TEntity, TId> 
    where TEntity : BaseEntity<TId>
    where TId : struct
{
    #region Okuma İşlemleri (Read)
    
    /// <summary>
    /// ID'ye göre entity getirir
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity veya null</returns>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tüm entity'leri getirir (sadece okuma amaçlı)
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity listesi</returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Koşula göre entity'leri getirir
    /// </summary>
    /// <param name="predicate">Filtre koşulu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity listesi</returns>
    Task<IReadOnlyList<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Koşul, sıralama ve include ile entity'leri getirir
    /// </summary>
    /// <param name="predicate">Filtre koşulu</param>
    /// <param name="orderBy">Sıralama</param>
    /// <param name="includes">Include edilecek navigation property'ler</param>
    /// <param name="disableTracking">Tracking kapalı mı?</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity listesi</returns>
    Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        List<Expression<Func<TEntity, object>>>? includes = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Koşula uyan ilk entity'yi getirir
    /// </summary>
    /// <param name="predicate">Filtre koşulu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity veya null</returns>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Koşula uygun entity sayısını döndürür
    /// </summary>
    /// <param name="predicate">Filtre koşulu (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Entity sayısı</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Koşula uygun entity var mı kontrol eder
    /// </summary>
    /// <param name="predicate">Filtre koşulu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Var ise true</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sayfalama ile entity'leri getirir
    /// </summary>
    /// <param name="pageNumber">Sayfa numarası (1'den başlar)</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <param name="predicate">Filtre koşulu</param>
    /// <param name="orderBy">Sıralama</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış entity listesi ve toplam sayı</returns>
    Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Yazma İşlemleri (Write)
    
    /// <summary>
    /// Yeni entity ekler
    /// </summary>
    /// <param name="entity">Eklenecek entity</param>
    /// <returns>Eklenen entity</returns>
    TEntity Add(TEntity entity);
    
    /// <summary>
    /// Yeni entity ekler (async)
    /// </summary>
    /// <param name="entity">Eklenecek entity</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Eklenen entity</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Birden fazla entity ekler
    /// </summary>
    /// <param name="entities">Eklenecek entity'ler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Entity günceller
    /// </summary>
    /// <param name="entity">Güncellenecek entity</param>
    void Update(TEntity entity);
    
    /// <summary>
    /// Birden fazla entity günceller
    /// </summary>
    /// <param name="entities">Güncellenecek entity'ler</param>
    void UpdateRange(IEnumerable<TEntity> entities);
    
    /// <summary>
    /// Entity siler
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    void Delete(TEntity entity);
    
    /// <summary>
    /// ID'ye göre entity siler
    /// </summary>
    /// <param name="id">Silinecek entity ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Birden fazla entity siler
    /// </summary>
    /// <param name="entities">Silinecek entity'ler</param>
    void DeleteRange(IEnumerable<TEntity> entities);
    
    #endregion
    
    #region IQueryable
    
    /// <summary>
    /// IQueryable döndürür (sadece okuma amaçlı, no tracking)
    /// </summary>
    /// <returns>IQueryable</returns>
    IQueryable<TEntity> AsQueryable();
    
    /// <summary>
    /// IQueryable döndürür (tracking ile)
    /// </summary>
    /// <returns>IQueryable</returns>
    IQueryable<TEntity> AsQueryableTracking();
    
    #endregion
}

/// <summary>
/// Int tipinde ID kullanan entity'ler için generic repository
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
public interface IGenericRepository<TEntity> : IGenericRepository<TEntity, int>
    where TEntity : BaseEntity<int>
{
}
