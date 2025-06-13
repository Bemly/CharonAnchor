namespace Lagrange.Milky.Extension;

public static class ReaderWriterLockSlimExtension
{
    public static ReadLockDisposable UsingReadLock(this ReaderWriterLockSlim @lock)
    {
        @lock.EnterReadLock();
        return new ReadLockDisposable(@lock);
    }

    public static WriteLockDisposable UsingWriteLock(this ReaderWriterLockSlim @lock)
    {
        @lock.EnterWriteLock();
        return new WriteLockDisposable(@lock);
    }

    public static UpgradeableReadLockDisposable UsingUpgradeableReadLock(this ReaderWriterLockSlim @lock)
    {
        @lock.EnterUpgradeableReadLock();
        return new UpgradeableReadLockDisposable(@lock);
    }

    public readonly ref struct ReadLockDisposable(ReaderWriterLockSlim @lock) : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = @lock;

        public void Dispose() => _lock.ExitReadLock();
    }

    public readonly ref struct WriteLockDisposable(ReaderWriterLockSlim @lock) : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = @lock;

        public void Dispose() => _lock.ExitWriteLock();
    }

    public readonly ref struct UpgradeableReadLockDisposable(ReaderWriterLockSlim @lock) : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = @lock;

        public void Dispose() => _lock.ExitUpgradeableReadLock();
    }
}