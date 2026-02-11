namespace CWI.Application.Common.Models;

/// <summary>
/// API operasyonları için generic result wrapper
/// </summary>
public class Result<T>
{
    /// <summary>İşlem başarılı mı?</summary>
    public bool Success { get; private set; }
    
    /// <summary>Sonuç verisi</summary>
    public T? Data { get; private set; }
    
    /// <summary>Hata mesajı</summary>
    public string? Error { get; private set; }
    
    /// <summary>Hata detayları</summary>
    public List<string> Errors { get; private set; } = new();
    
    /// <summary>
    /// Başarılı sonuç oluşturur
    /// </summary>
    public static Result<T> Succeed(T data)
    {
        return new Result<T>
        {
            Success = true,
            Data = data
        };
    }
    
    /// <summary>
    /// Başarısız sonuç oluşturur
    /// </summary>
    public static Result<T> Failure(string error)
    {
        return new Result<T>
        {
            Success = false,
            Error = error,
            Errors = new List<string> { error }
        };
    }
    
    /// <summary>
    /// Birden fazla hata ile başarısız sonuç oluşturur
    /// </summary>
    public static Result<T> Failure(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        return new Result<T>
        {
            Success = false,
            Error = errorList.FirstOrDefault(),
            Errors = errorList
        };
    }
}

/// <summary>
/// Veri içermeyen result wrapper
/// </summary>
public class Result
{
    /// <summary>İşlem başarılı mı?</summary>
    public bool Success { get; private set; }
    
    /// <summary>Hata mesajı</summary>
    public string? Error { get; private set; }
    
    /// <summary>Hata detayları</summary>
    public List<string> Errors { get; private set; } = new();
    
    /// <summary>
    /// Başarılı sonuç oluşturur
    /// </summary>
    public static Result Succeed()
    {
        return new Result
        {
            Success = true
        };
    }
    
    /// <summary>
    /// Başarısız sonuç oluşturur
    /// </summary>
    public static Result Failure(string error)
    {
        return new Result
        {
            Success = false,
            Error = error,
            Errors = new List<string> { error }
        };
    }
    
    /// <summary>
    /// Birden fazla hata ile başarısız sonuç oluşturur
    /// </summary>
    public static Result Failure(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        return new Result
        {
            Success = false,
            Error = errorList.FirstOrDefault(),
            Errors = errorList
        };
    }
}
