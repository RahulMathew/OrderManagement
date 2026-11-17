namespace OrderManagement.Domain.Common
{
    public sealed class Result<T> : Result
    {
        private readonly T? _value;

        public T Value
        {
            get
            {
                if (IsFailure)
                {
                    throw new InvalidOperationException(
                        "Cannot access the value of a failed result.");
                }

                return _value!;
            }
        }

        private Result(T? value,
            bool isSuccess,
            Error? error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        public static Result<T> Success(T value)
        {
            if (value is null)
                return Failure(Error.NullValue);

            return new Result<T>(value, true, null);
        }

        public static new Result<T> Failure(Error error)
        {
            return new Result<T>(default, false, error);
        }

        public static implicit operator Result<T>(T value)
        {
            return Success(value);
        }
    }
}
