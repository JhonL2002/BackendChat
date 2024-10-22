using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace BackendChat.Results
{
    public class Result<T>
    {
        public T Value { get; set; }
        public bool IsSuccess { get; }
        public string Error { get; }

        private Result(T value, bool isSuccess, string error)
        {
            Value = value;
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result<T> Success(T value) => new Result<T>(value, true, null);
        public static Result<T> Failure(string error) => new Result<T>(default, false, error);
    }
}
