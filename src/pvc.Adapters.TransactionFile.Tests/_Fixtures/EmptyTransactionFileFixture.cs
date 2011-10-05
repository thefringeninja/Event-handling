using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;

namespace pvc.Adapters.TransactionFile.Tests._Fixtures
{
    public class EmptyTransactionFileFixture<T> where T : new()
    {
        public EmptyTransactionFileFixture(string filename)
        {
            ResetTransactionFile(filename);

            new TransactionFileWriter<T>(filename, new BinaryFormatter());
        }

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