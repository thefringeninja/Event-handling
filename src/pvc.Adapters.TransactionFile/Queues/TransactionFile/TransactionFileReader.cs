using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using log4net;

namespace pvc.Adapters.TransactionFile.Queues.TransactionFile
{
	/// <summary>
	/// Reads transactions from the transaction file
	/// </summary>
	/// <typeparam name="T">The type of transactions to read, generally this will be a shared base class or interface but could also be 'object' in order to allow untyped access</typeparam>
	internal class TransactionFileReader<T>
	{
		protected FileStream _fileStream;
		protected IFormatter _formatter;
	    protected ILog _logger;

		/// <summary>
		/// Reads the next item off of the stream, blocking if one is not available.
		/// </summary>
		/// <returns></returns>
		public virtual T Dequeue()
		{
			while (true)
			{
				if (_fileStream.Position != _fileStream.Length)
				{
					var o = _formatter.Deserialize(_fileStream);
					return (T)o;
				}
				Thread.Sleep(1);
			}
		}

        public TransactionFileReader(string filename, IFormatter formatter)
		{
			if (filename == null)
			{
                throw new ArgumentNullException("filename");
			}
			if (formatter == null)
			{
                throw new ArgumentNullException("formatter");
			}
			
            _logger = LogManager.GetLogger(GetType());
			_formatter = formatter;
			var fi = new FileInfo(filename); 
			
            if (_logger.IsDebugEnabled)
			{
                _logger.Debug(string.Format("Opening Transaction File: {0}", filename));
			}
			
            _fileStream = fi.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
		}
	}
}
