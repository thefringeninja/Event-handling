namespace pvc.Adapters.TransactionFile
{
    public interface IChecksum
    {
        void SetValue(long value);

        long GetValue();

        void Reset();

        string Name { get; }
    }
}
