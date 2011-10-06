using System;
using System.IO;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Tests._Fixtures;
using pvc.Core;

namespace pvc.Adapters.TransactionFile.Tests.Processing
{
    [TestFixture]
    public class With_instrumented_processor : InstrumentedProcessorFixture
    {
        private const string Checksum = "total_messages_value_is_valid_checksum";

        [Serializable]
        public class FakeMessage : Message { }

        public With_instrumented_processor() : base(Checksum)
        {
            
        }

        [Test]
        public void total_messages_value_is_valid()
        {
            const string filename = "total_messages_value_is_valid";
            if(File.Exists(filename))
            {
                File.Delete(filename);
                File.Delete(filename + ".write.chk");
                File.Delete(filename + "_checksum.read.chk");
            }
            
            // This consumer handles messages by writing them to disk
            var consumer = new TransactionFileConsumer<Message>(filename);
            consumer.Handle(new FakeMessage());
            consumer.Handle(new FakeMessage());

            // This producer reads messages written to disk and instruments the results
            var producer = new InstrumentedTransactionFileProducer<Message>(filename, Checksum);
            producer.AttachConsumer(consumer);
            
            Assert.AreEqual(2, Instrument.TotalMessages);
        }
    }
}
