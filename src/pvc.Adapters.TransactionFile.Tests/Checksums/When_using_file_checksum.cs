using System.IO;
using System.Threading;
using NUnit.Framework;
using pvc.Adapters.TransactionFile.Checksums;
using pvc.Adapters.TransactionFile.Tests._Fixtures;

namespace pvc.Adapters.TransactionFile.Tests.Checksums
{
    [TestFixture]
    public class When_using_file_checksum : FileChecksumFixture
    {
        private const string Filename = "When_using_file_checksum";

        public When_using_file_checksum() : base(Filename)
        {


        }

        [Test]
        public void name_is_set()
        {
            Assert.AreEqual("When_using_file_checksum", Checksum.Name);
        }

        [Test]
        public void constructing_results_in_existing_file_with_default_value()
        {
            Assert.IsNotNull(Checksum);
            Assert.IsTrue(File.Exists(Filename));
            Assert.AreEqual(1, new FileInfo(Filename).Length);
            Assert.AreEqual(0, Checksum.GetValue(), "Value was not zero when file was empty");
        }

        [Test]
        public void value_is_obtained_after_writing()
        {
            Checksum.SetValue(12345);
            var value = Checksum.GetValue();
            Assert.AreEqual(12345, value);
        }

        [Test]
        public void using_an_empty_existing_file_resets_file_properly()
        {
            const string filename = "empty_existing_file_is_reset_properly";
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            File.CreateText(filename).Dispose();
            Assert.IsTrue(File.Exists(filename), "Empty file was not created");
            Assert.AreEqual(0, new FileInfo(filename).Length, "Empty file was not empty");

            var checksum = new FileChecksum(filename);
            Assert.IsNotNull(checksum);
            Assert.AreEqual(0, checksum.GetValue(), "Empty file used as checksum base did not get default value");
        }
    }
}
