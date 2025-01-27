namespace Kuchinashi.DataSystem
{
    public interface IReadableData
    {
        public abstract ReadableData DeSerialization();

        public abstract T DeSerialization<T>() where T : new();

        public abstract bool Validation<T>(out T value) where T : new();
    }
}