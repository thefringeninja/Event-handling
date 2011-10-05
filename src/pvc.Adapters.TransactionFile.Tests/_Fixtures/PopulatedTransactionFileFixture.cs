using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;

namespace pvc.Adapters.TransactionFile.Tests._Fixtures
{
    public class PopulatedTransactionFileFixture<T> where T : new()
    {
        public PopulatedTransactionFileFixture(string filename, int count)
        {
            ResetTransactionFile(filename);

            var writer = new TransactionFileWriter<T>(filename, new BinaryFormatter());

            for (var i = 0; i < count; i++)
            {
                writer.Enqueue(new T());
            }

            Count = count;
        }

        protected int Count { get; private set; }

        private static void ResetTransactionFile(string filename)
        {
            if (!File.Exists(filename))
            {
                return;
            }
            File.Delete(filename);
            File.Delete(string.Concat(filename, ".write.chk"));
        }
    }
}