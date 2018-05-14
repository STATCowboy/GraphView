﻿
namespace GraphView.Transaction
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    internal class SingletonPartitionedVersionTable : VersionTable
    {
        
        /// <summary>
        /// A dict array to store all versions, recordKey => {versionKey => versionEntry}
        /// Every version table may be stored on several partitions, and for every partition, it has a dict
        /// 
        /// The idea to use the version entry rather than versionBlob is making sure never create a new version entry
        /// unless upload it
        /// </summary>
        private readonly Dictionary<object, Dictionary<long, VersionEntry>>[] dicts;

        /// <summary>
        /// Request queues for partitions
        /// </summary>
        private readonly Queue<VersionEntryRequest>[] requestQueues;

        /// <summary>
        /// Spinlocks for partitions
        /// </summary>
        private readonly SpinLock[] queueLocks;

        private readonly PartitionVersionEntryRequestVisitor[] requestVisitors;

        private static readonly int RECORD_CAPACITY = 1000000;

        internal static readonly int VERSION_CAPACITY = 16;

        internal int PartitionCount { get; private set; }

        public SingletonPartitionedVersionTable(VersionDb versionDb, string tableId, int partitionCount)
            : base (versionDb, tableId)
        {
            this.PartitionCount = partitionCount;
            this.dicts = new Dictionary<object, Dictionary<long, VersionEntry>>[partitionCount];
            this.requestQueues = new Queue<VersionEntryRequest>[partitionCount];
            this.queueLocks = new SpinLock[partitionCount];
            this.requestVisitors = new PartitionVersionEntryRequestVisitor[partitionCount];

            for (int pid = 0; pid < partitionCount; pid ++)
            {
                this.dicts[pid] = new Dictionary<object, Dictionary<long, VersionEntry>>(SingletonPartitionedVersionTable.RECORD_CAPACITY);
                this.requestQueues[pid] = new Queue<VersionEntryRequest>();
                this.queueLocks[pid] = new SpinLock();
                this.requestVisitors[pid] = new PartitionVersionEntryRequestVisitor(this.dicts[pid]);
            }
        }

        internal override void Clear()
        {
            for (int pid = 0; pid < this.PartitionCount; pid++)
            {
                this.dicts[pid].Clear();
                this.requestQueues[pid].Clear();
            }
        }

        internal override void EnqueueTxRequest(TxRequest req)
        {
            // Debug.Assert(req is VersionEntryRequest);

            VersionEntryRequest verReq = req as VersionEntryRequest;
            int pk = this.VersionDb.PhysicalPartitionByKey(verReq.RecordKey);

            bool lockTaken = false;
            try
            {
                this.queueLocks[pk].Enter(ref lockTaken);
                this.requestQueues[pk].Enqueue(verReq);
            }
            finally
            {
                if (lockTaken)
                {
                    this.queueLocks[pk].Exit();
                }
            }
        }

        /// <summary>
        /// Dequeue all items to an IEnumerable container
        /// </summary>
        /// <param name="pk"></param>
        /// <returns></returns>
        private IEnumerable<VersionEntryRequest> DequeueRequests(int pk)
        {
            Queue<VersionEntryRequest> queue = this.requestQueues[pk];
            VersionEntryRequest[] reqArray = null;

            // Check whether the queue is empty at first
            if (queue.Count > 0)
            {
                bool lockTaken = false;
                try
                {
                    this.queueLocks[pk].Enter(ref lockTaken);
                    // In case other running threads also flush the same queue
                    if (queue.Count > 0)
                    {
                        reqArray = queue.ToArray();
                        queue.Clear();
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        this.queueLocks[pk].Exit();
                    }
                }
            }
            return reqArray;
        }

        internal override void Visit(int partitionKey)
        {
            IEnumerable<VersionEntryRequest> flushReqs = this.DequeueRequests(partitionKey);

            if (flushReqs == null)
            {
                return;
            }

            PartitionVersionEntryRequestVisitor visitor = this.requestVisitors[partitionKey];
            foreach (VersionEntryRequest req in flushReqs)
            {
                visitor.Invoke(req);
            }
        }
    }
}