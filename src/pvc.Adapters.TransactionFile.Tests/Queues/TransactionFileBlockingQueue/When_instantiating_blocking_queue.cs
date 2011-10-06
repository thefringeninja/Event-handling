﻿using System;
using NUnit.Framework;
using Queue = pvc.Adapters.TransactionFile.Queues.TransactionFileBlockingQueue<object>;

namespace pvc.Adapters.TransactionFile.Tests.Queues.TransactionFileBlockingQueue
{
    [TestFixture]
    public class When_instantiating_blocking_queue
    {
        [Test]
        public void filename_is_required()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new Queue(null, null);
            });
        }

        [Test]
        public void checksum_file_is_not_required()
        {
            Assert.DoesNotThrow(
                () =>
                    {
                        var queue = new Queue("formatter_is_not_required", null);
                        Assert.IsNotNull(queue);
                    }
                );
        }

        [Test]
        public void formatter_is_not_required()
        {
            Assert.DoesNotThrow(
                () =>
                    {
                        var queue = new Queue("formatter_is_not_required", "formatter_is_not_required");
                        Assert.IsNotNull(queue);
                    });
        }
    }
}
