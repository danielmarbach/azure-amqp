// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Amqp
{
    using System.Threading;

    /// <summary>
    /// Serializes concurrent work items and execute each work item
    /// sequentially until it is completed.
    /// </summary>
    public sealed class SerializedWorker<T> where T : class
    {
        // the delegate should return true if work is completed
        readonly IWorkDelegate<T> workDelegate;
        readonly ConcurrentPriorityCollection<T> pendingWorkList;
        volatile int state;

        private const int IdleState = 0;
        private const int BusyState = 1;
        private const int BusyWithContinueState = 2;
        private const int WaitingForContinueState = 3;
        private const int AbortedState = 4;

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="workProcessor">The delegate to execute the work.</param>
        public SerializedWorker(IWorkDelegate<T> workProcessor)
        {
            this.workDelegate = workProcessor;
            this.state = IdleState;
            this.pendingWorkList = new ConcurrentPriorityCollection<T>();
        }

        /// <summary>
        /// Gets the count of the pending work items.
        /// </summary>
        public int Count => this.pendingWorkList.Count;

        /// <summary>
        /// Starts to do a work item. Depending on the worker state,
        /// the work may be queued, or started immediately.
        /// </summary>
        /// <param name="work">The work item.</param>
        public void DoWork(T work)
        {
            if (this.state == AbortedState)
            {
                return;
            }

            if (this.state != IdleState)
            {
                // Only do new work in idle state
                this.pendingWorkList.AddLast(work);
                return;
            }

            Interlocked.Exchange(ref this.state, BusyState);
            this.DoWorkInternal(work, false);
        }

        /// <summary>
        /// Continues to do the pending work items, if any.
        /// </summary>
        public void ContinueWork()
        {
            if (this.state == BusyWithContinueState || this.state == AbortedState)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref state, BusyWithContinueState, BusyState) == BusyState)
            {
                return;
            }

            // Idle or WaitingForContinue, we should do the work
            if (this.pendingWorkList.TryDequeue(out var work))
            {
                Interlocked.Exchange(ref this.state, BusyState);
            }

            if (work != null)
            {
                this.DoWorkInternal(work, true);
            }
        }

        /// <summary>
        /// Aborts the worker. All pending work items are discarded.
        /// </summary>
        public void Abort()
        {
            this.pendingWorkList.Clear();
            Interlocked.Exchange(ref this.state, AbortedState);
        }

        void DoWorkInternal(T work, bool fromList)
        {
            while (work != null)
            {
                if (this.workDelegate.Invoke(work))
                {
                    work = null;
                    if (this.state != AbortedState)
                    {
                        if (this.pendingWorkList.TryDequeue(out work))
                        {
                            fromList = true;
                        }

                        if (work == null)
                        {
                            // either there is no work or the worker was aborted
                            Interlocked.Exchange(ref this.state, IdleState);
                            return;
                        }

                        Interlocked.Exchange(ref this.state, BusyState);
                    }
                }
                else
                {
                    if (this.state == AbortedState)
                    {
                        work = null;
                    }
                    else if (Interlocked.CompareExchange(ref state, BusyState, BusyWithContinueState) == BusyWithContinueState)
                    {
                        // Continue called right after workFunc returned false
                    }
                    else
                    {
                        if (!fromList)
                        {
                            // add to the head since later work may be queued already
                            this.pendingWorkList.AddFirst(work);
                        }

                        Interlocked.Exchange(ref this.state, WaitingForContinueState);
                        work = null;
                    }
                }
            }
        }
    }
}