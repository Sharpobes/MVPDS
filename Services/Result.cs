namespace MVPDS.Services;

public class Result
{
    public bool IsSuccess { get; private set; }
    public string Error { get; private set; }

    public static Result Success() => new Result { IsSuccess = true };
    public static Result Failure(string error) => new Result { IsSuccess = false, Error = error };
}