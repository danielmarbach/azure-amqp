#nullable enable

namespace Microsoft.Azure.Amqp
{
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;

    sealed class ConcurrentPriorityCollection<T>
    {
        private readonly ConcurrentQueue<T> queuedItems = new ConcurrentQueue<T>();
        private readonly ConcurrentStack<T> stackedItems = new ConcurrentStack<T>();

        public int Count => queuedItems.Count + stackedItems.Count;

        public bool TryDequeue([MaybeNullWhen(false)] out T item)
        {
            return !stackedItems.TryPop(out item) && queuedItems.TryDequeue(out item);
        }

        public void AddLast(T item)
        {
            queuedItems.Enqueue(item);
        }

        public void AddFirst(T item)
        {
            stackedItems.Push(item);
        }

        public void Clear()
        {
            stackedItems.Clear();
            while (queuedItems.TryDequeue(out _))
            {
            }
        }
    }
}