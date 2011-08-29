using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using log4net;
using pvc.Adapters.TransactionFile.Checksums;

namespace pvc.Adapters.TransactionFile.Queues.TransactionFile
{
    /// <summary>
    /// Handles writing to the transaction file
    /// </summary>
    /// <typeparam name="T">The type of transactions to read, generally this will be a shared base class or
    /// interface but could also be 'object' in order to allow untyped access</typeparam>
    public class TransactionFileWriter<T>
    {
        private readonly IChecksum _writeChecksum;
        private readonly IFormatter _formatter;
        private readonly FileStream _fileStream;
        private readonly Mutex _fileProtectMutex;
		private readonly ILog _logger;

        public TransactionFileWriter(string filename, IFormatter formatter): this(filename, true, formatter)
        {

        }
        
        public TransactionFileWriter(string filename, bool useMutex, IFormatter formatter)
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
			if (_logger.IsDebugEnabled)
			{
                _logger.Debug(string.Format("Opening file {0}", filename));
			}

            _fileStream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            
            var filePath = string.Format("{0}\\{1}.chk", fi.DirectoryName, fi.Name);
			if (_logger.IsDebugEnabled)
			{
			    _logger.Debug(string.Format("Opening Checksum: {0}", filename));
			}

            _writeChecksum = new FileChecksum(filePath);
            _formatter = formatter;
            if (useMutex)
            {
                bool creatednew;
                _fileProtectMutex = new Mutex(false, fi.Name, out creatednew);
                if (creatednew)
                {
					if (_logger.IsDebugEnabled)
					{
					    _logger.Debug(string.Format("Created new mutex of name {0}", fi.Name));
					}
                }
            }
            else
            {
                _fileProtectMutex = null;
            }
        }

        public void Enqueue(T value)
        {
            if (_fileProtectMutex != null)
            {
                var obtained = _fileProtectMutex.WaitOne(5000, false);
                if (!obtained)
                {
                    throw new Exception("Unable to acquire mutex");
                }
            }
            try
            {
                _fileStream.Seek(_writeChecksum.GetValue(), SeekOrigin.Begin);
                _formatter.Serialize(_fileStream, value);
                _fileStream.Flush();
                _writeChecksum.SetValue(_fileStream.Position);
            }
            finally
            {
                if (_fileProtectMutex != null)
                {
                    _fileProtectMutex.ReleaseMutex();
                }
            }
        }
    }
}
