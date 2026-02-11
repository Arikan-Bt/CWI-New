namespace CWI.Domain.Common;

/// <summary>
/// Soft delete desteği için interface
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Kayıt aktif mi?
    /// </summary>
    bool IsActive { get; set; }
}
