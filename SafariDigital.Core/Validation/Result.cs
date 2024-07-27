namespace SafariDigital.Core.Validation;

public class Result
{
    public List<ResultMessage> Errors { get; init; } = [];
    public List<ResultMessage> Warnings { get; init; } = [];
    public List<ResultMessage> Infos { get; init; } = [];

    public bool HasError => Errors.Count > 0;
    public bool HasWarning => Warnings.Count > 0;

    public Result Merge(Result result)
    {
        Errors.AddRange(result.Errors);
        Warnings.AddRange(result.Warnings);
        Infos.AddRange(result.Infos);
        return this;
    }

    public void AddError(Exception ex) => Errors.Add(new ResultMessage(ex));
    public void AddError(System.Enum message) => Errors.Add(new ResultMessage(message));
    public void AddError(System.Enum message, Exception ex) => Errors.Add(new ResultMessage(ex, message));
    public void AddWarning(System.Enum message) => Warnings.Add(new ResultMessage(message));
    public void AddInfo(System.Enum message) => Infos.Add(new ResultMessage(message));

    public TB ValidateExpression<TB>(Result<TB> func)
    {
        Merge(func);
        return func.Value;
    }

    public async Task<TB> ValidateExpressionAsync<TB>(Task<Result<TB>> func)
    {
        var subVm = await func;
        Merge(subVm);
        return subVm.Value;
    }
}

public class Result<T> : Result
{
    public Result()
    {
    }

    public Result(T value)
    {
        Value = value;
    }

    public T? Value { get; set; }

    public new Result<T> AddError(Exception ex)
    {
        Errors.Add(new ResultMessage(ex));
        return this;
    }

    public new Result<T> AddError(System.Enum message)
    {
        Errors.Add(new ResultMessage(message));
        return this;
    }

    public new Result<T> AddError(System.Enum message, Exception ex)
    {
        Errors.Add(new ResultMessage(ex, message));
        return this;
    }

    public Result<T> AddWarning(Exception ex)
    {
        Warnings.Add(new ResultMessage(ex));
        return this;
    }

    public new Result<T> AddWarning(System.Enum message)
    {
        Warnings.Add(new ResultMessage(message));
        return this;
    }

    public new Result<T> AddInfo(System.Enum message)
    {
        Infos.Add(new ResultMessage(message));
        return this;
    }
}