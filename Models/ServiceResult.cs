namespace Banko.Models
{
  public class ServiceResult<T>
  {
    public T? Data { get; private set; }
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<string> Errors { get; private set; } = new List<string>(); // For multiple errors

    private ServiceResult(T? data, bool isSuccess, string? errorMessage = null, List<string>? errors = null)
    {
      Data = data;
      IsSuccess = isSuccess;
      ErrorMessage = errorMessage;
      if (errors != null)
      {
        Errors.AddRange(errors);
      }
      else if (!string.IsNullOrEmpty(errorMessage))
      {
        Errors.Add(errorMessage);
      }
    }

    public static ServiceResult<T> Success(T data) => new ServiceResult<T>(data, true);
    public static ServiceResult<T> Failure(string errorMessage) => new ServiceResult<T>(default, false, errorMessage);
    public static ServiceResult<T> Failure(List<string> errors) => new ServiceResult<T>(default, false, null, errors);
  }

  // Non-generic version for operations that don't return data on success
  public class ServiceResult
  {
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<string> Errors { get; private set; } = new List<string>();

    private ServiceResult(bool isSuccess, string? errorMessage = null, List<string>? errors = null)
    {
      IsSuccess = isSuccess;
      ErrorMessage = errorMessage;
      if (errors != null)
      {
        Errors.AddRange(errors);
      }
      else if (!string.IsNullOrEmpty(errorMessage))
      {
        Errors.Add(errorMessage);
      }
    }

    public static ServiceResult Success() => new ServiceResult(true);
    public static ServiceResult Failure(string errorMessage) => new ServiceResult(false, errorMessage);
    public static ServiceResult Failure(List<string> errors) => new ServiceResult(false, null, errors);
  }
}