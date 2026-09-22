namespace Atm.Web.Operations;

public enum Operation
{
    Deposit,
    Withdraw,
    Transfer,
}

public static class OperationCatalog
{
    public static readonly IReadOnlyList<Operation> All = [Operation.Deposit, Operation.Withdraw, Operation.Transfer];

    public static Operation Parse(string? value) =>
        Enum.TryParse<Operation>(value, ignoreCase: true, out var operation) && Enum.IsDefined(operation)
            ? operation
            : Operation.Deposit;

    public static string Slug(this Operation operation) => operation.ToString().ToLowerInvariant();

    public static string Label(this Operation operation) => operation.ToString();
}
