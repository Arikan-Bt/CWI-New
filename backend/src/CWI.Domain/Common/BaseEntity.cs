namespace CWI.Domain.Common;

/// <summary>
/// Tüm entity'ler için temel sınıf
/// </summary>
/// <typeparam name="TId">Primary key tipi</typeparam>
public abstract class BaseEntity<TId> where TId : struct
{
    /// <summary>
    /// Birincil anahtar (Primary Key)
    /// </summary>
    public TId Id { get; set; }
}

/// <summary>
/// Int tipinde Id kullanan entity'ler için temel sınıf
/// </summary>
public abstract class BaseEntity : BaseEntity<int>
{
}

/// <summary>
/// Long tipinde Id kullanan entity'ler için temel sınıf
/// </summary>
public abstract class BaseLongEntity : BaseEntity<long>
{
}
