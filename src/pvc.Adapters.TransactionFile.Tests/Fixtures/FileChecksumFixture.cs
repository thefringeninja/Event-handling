using System.IO;
using pvc.Adapters.TransactionFile.Checksums;

namespace pvc.Adapters.TransactionFile.Tests.Fixtures
{
    public class FileChecksumFixture
    {
        public IChecksum Checksum { get; internal set; }

        public FileChecksumFixture(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            Checksum = new FileChecksum(filename);
        }
    }
}