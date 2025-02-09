using NbpPlnExchangeRates.Domain.Common.Errors;

namespace NbpPlnExchangeRates.Domain.Common;

public class Result
{
    protected internal Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
        {
            throw new InvalidOperationException();
        }

        if (isSuccess is false && error is null)
        {
            throw new InvalidOperationException();
        }
        
        IsSuccess = isSuccess;
        Errors = error is null
            ? Array.Empty<Error>()
            : new[] { error };
    }

    protected internal Result(bool isSuccess, Error[] errors)
    {
        if (isSuccess && errors is not null)
        {
            throw new InvalidOperationException();
        }

        if (isSuccess is false && (errors is null || errors.Length == 0))
        {
            throw new InvalidOperationException();
        }
        
        IsSuccess = isSuccess;
        Errors = errors ?? Array.Empty<Error>();
    }
    
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;
    
    public Error[] Errors { get; }
    
    public string AggregatedErrorMessages => 
        IsFailure
            ? string.Join(Environment.NewLine, Errors.Select(e => e.ErrorMessage))
            : throw new InvalidOperationException("Can only get aggregated error messages when result is Error.");
    
    public static implicit operator Result(Error error) => 
        Failure(error);
    
    public static Result Success() =>
        new(true, (Error)null);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, (Error)null);
    
    public static Result Failure(Error error) =>
        new(false, error);
    
    public static Result Failure(Error[] errors) => 
        new(false, errors);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);

    public static Result<TValue> Failure<TValue>(Error[] errors) =>
        new(default, false, errors);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }
    
    protected internal Result(TValue? value, bool isSuccess, Error[] errors)
        : base(isSuccess, errors)
    {
        _value = value;
    }

    public TValue Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(new NullValueError());

    public static implicit operator Result<TValue>(Error error) => 
        Failure<TValue>(error);

}