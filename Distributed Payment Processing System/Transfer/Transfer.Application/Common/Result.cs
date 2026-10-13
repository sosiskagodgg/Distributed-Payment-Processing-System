

namespace Transfer.Application.Common
{
    public class Result<T>
    {
        public T? Value { get; private set; }
        public bool IsSuccess { get; private set; }
        public string? Error { get; private set; }
        
        public static Result<T> Ok(T value)
        {
            return new Result<T>()
            {
                Value = value,
                IsSuccess = true
            };
        }
        public static Result<T> Fail(string error)
        {
            return new Result<T>()
            {
                IsSuccess = false,
                Error = error
            };
        }

    }
}
