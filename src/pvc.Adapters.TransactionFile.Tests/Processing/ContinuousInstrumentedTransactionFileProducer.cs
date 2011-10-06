using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using pvc.Adapters.TransactionFile.Queues;
using pvc.Core;
using pvc.Core.Processing;
using pvc.Core.Processing.Instrumentation;

namespace pvc.Adapters.TransactionFile.Tests.Processing
{
    /// <summary>
    /// A diagnostic producer that will continously deliver any message that downstream consumers handle,
    /// instrumenting the message flow.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ContinuousInstrumentedTransactionFileProducer<T> : Produces<T>, IDisposable where T : Message
    {
        private readonly TransactionFileBlockingQueue<T> _reader;
        private readonly InstrumentedQueueProcessor<T> _processor;
        private Consumes<T> _consumer;

        public ContinuousInstrumentedTransactionFileProducer(string filename, string checksumFilename)
        {
            _reader = new TransactionFileBlockingQueue<T>(filename, checksumFilename, new BinaryFormatter());
            _processor = new InstrumentedQueueProcessor<T>(
                _reader, 
                new AlwaysSendToMeFactory(new SendToMeCommandProcessor(this)), 
                new WMIQueueProcessorInstrumentation(checksumFilename), 
                0); 
        }

        public void AttachConsumer(Consumes<T> consumer)
        {
            _consumer = consumer;
        }

        public void Dispose()
        {
            _processor.Stop();
        }

        private class AlwaysSendToMeFactory : ICommandProcessorFactory<T>
        {
            private readonly List<ICommandProcessor<T>> _toSend = new List<ICommandProcessor<T>>();

            public AlwaysSendToMeFactory(ICommandProcessor<T> toSend)
            {
                _toSend.Add(toSend);
            }

            public IList<ICommandProcessor<T>> GetProcessorsForCommand(T command)
            {
                return _toSend;
            }
        }

        private class SendToMeCommandProcessor : ICommandProcessor<T>
        {
            private readonly ContinuousInstrumentedTransactionFileProducer<T> _parent;

            public SendToMeCommandProcessor(ContinuousInstrumentedTransactionFileProducer<T> parent)
            {
                _parent = parent;
            }

            public void ProcessCommand(T command)
            {
                _parent.Send(command);
            }
        }

        private void Send(T message)
        {
            _consumer.TryConsume(message);
        }
    }
}