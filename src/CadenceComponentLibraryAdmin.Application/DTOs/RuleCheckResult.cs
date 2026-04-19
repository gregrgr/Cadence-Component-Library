namespace CadenceComponentLibraryAdmin.Application.DTOs;

public sealed class RuleCheckResult
{
    private readonly List<string> _errors = [];

    public bool Succeeded => _errors.Count == 0;

    public IReadOnlyList<string> Errors => _errors;

    public static RuleCheckResult Success() => new();

    public static RuleCheckResult Failure(params IEnumerable<string>[] errors)
    {
        var result = new RuleCheckResult();

        foreach (var batch in errors)
        {
            result._errors.AddRange(batch.Where(static x => !string.IsNullOrWhiteSpace(x)));
        }

        return result;
    }

    public void AddError(string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            _errors.Add(error);
        }
    }
}
