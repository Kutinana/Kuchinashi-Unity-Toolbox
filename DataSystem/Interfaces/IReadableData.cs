namespace Kuchinashi.DataSystem
{
    public interface IReadableData
    {
        public abstract IReadableData DeSerialization();

        public abstract T DeSerialization<T>() where T : IReadableData, new();

        public abstract bool Validation<T>(out T value) where T : IReadableData, new();
    }
}