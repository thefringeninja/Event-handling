using System;
using System.IO;
using System.Runtime.Serialization;
using log4net;
using pvc.Adapters.TransactionFile.Checksums;

namespace pvc.Adapters.TransactionFile.Queues.TransactionFile
{
    /// <summary>
    /// Handles writing to the transaction file
    /// </summary>
    /// <typeparam name="T">The type of transactions to read, generally this will be a shared base class or
    /// interface but could also be 'object' in order to allow untyped access</typeparam>
    internal class TransactionFileWriter<T> : IDisposable
    {
        private static readonly object _sync = new object();
        private readonly IChecksum _checksum;
        private readonly IFormatter _formatter;
        private FileStream _fileStream;
		private readonly ILog _logger;

        public string ChecksumName
        {
            get { return _checksum.Name; }
        }

        public TransactionFileWriter(string filename, IFormatter formatter)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }

            if (formatter == null)
            {
                throw new ArgumentNullException("Formatter");
            }
            
			_logger = LogManager.GetLogger(GetType());
            var fi = new FileInfo(filename);
            _logger.DebugFormat("Opening file {0}", filename);

            _fileStream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            
            var filePath = string.Format(TransactionFile.WriteChecksumMask, fi.DirectoryName, fi.Name);
            _logger.DebugFormat("Opening Checksum: {0}", filename);

            _checksum = new FileChecksum(filePath);
            _formatter = formatter;
        }

        public void Enqueue(T value)
        {
            lock(_sync)
            {
                _fileStream.Seek(_checksum.GetValue(), SeekOrigin.Begin);
                _formatter.Serialize(_fileStream, value);
                _fileStream.Flush();
                _checksum.SetValue(_fileStream.Position);
            }
        }

        internal FileStream FileStream
        {
            get { return _fileStream; }
        }

        public void Dispose()
        {
            if(_fileStream != null)
            {
                _fileStream.Dispose();
                _fileStream = null;
            }
        }
    }
}
