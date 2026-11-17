namespace OrderManagement.Domain.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error? Error { get; }

        protected Result(bool isSuccess, Error? error)
        {
            if (isSuccess && error is not null)
                throw new ArgumentException(
                    "Successful result cannot have an error.");

            if (!isSuccess && error is null)
                throw new ArgumentException(
                    "Failed result must have an error.");

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success()
            => new(true, null);

        public static Result Failure(Error error)
            => new(false, error);
    }
}
