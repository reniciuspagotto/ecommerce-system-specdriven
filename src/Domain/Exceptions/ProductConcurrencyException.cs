namespace Domain.Exceptions;

public class ProductConcurrencyException : Exception
{
    public Guid ProductId { get; }
    public int ExpectedVersion { get; }
    public int ActualVersion { get; }

    public ProductConcurrencyException(Guid productId, int expectedVersion, int actualVersion) 
        : base($"Concurrency conflict for product '{productId}'. Expected version {expectedVersion}, but current version is {actualVersion}.")
    {
        ProductId = productId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
}
