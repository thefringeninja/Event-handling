using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using pvc.Adapters.TransactionFile.Queues.TransactionFile;

namespace pvc.Adapters.TransactionFile.Tests._Fixtures
{
    public class EmptyTransactionFileFixture
    {
        public EmptyTransactionFileFixture(string filename)
        {
            ResetTransactionFile(filename);

            using (new TransactionFileWriter<object>(filename, new BinaryFormatter())) { }
        }

        public static void ResetTransactionFile(string filename)
        {
            var checksum = string.Concat(filename, ".write.chk");
            if (!File.Exists(filename) && !File.Exists(checksum))
            {
                return;
            }
            File.Delete(filename);
            File.Delete(checksum);
        }
    }
}