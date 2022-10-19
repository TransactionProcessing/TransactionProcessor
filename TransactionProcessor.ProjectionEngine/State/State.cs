namespace TransactionProcessor.ProjectionEngine.State;

public record State()
{
    public Byte[] Version { get; init; }
    public Boolean IsInitialised => this.Version != null;
    public Boolean IsNotInitialised => this.Version == null;

    public Boolean ChangesApplied { get; init; }
}