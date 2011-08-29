using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using pvc.Adapters.TransactionFile.Checksums;

namespace pvc.Adapters.TransactionFile.Queues.TransactionFile
{
	/// <summary>
	/// Reads transactions from the transaction file updating a persistent checksum to allow for recovery
	/// from various problems such as application shutdown.
	/// </summary>
	/// <typeparam name="T">The type of the objects which are to be read from the transaction file.
	/// Usually this will be a shared base class or interface but for untyped access could just be object</typeparam>
	class CheckSummedTransactionFileReader<T> : TransactionFileReader<T>
	{
		private readonly FileChecksum _checksum;
		private readonly FileChecksum _writeChecksum;
       
	    /// <summary>
		/// Reads the next transaction from the transaction file and updates the checksum of the last known
		/// position.
		/// </summary>
		/// <remarks>
		/// TODO: This method needs to be changed to support calling back and awaiting completion from a caller 
		/// before checksumming. Without this there are two possible issues which can come up. If the checksum is done
		/// immediately after the read, it is possible that the application shuts down while the transaction
		/// is being processed in which case you may miss a transaction on a shutdown. In the case of doing it
		/// beforehand you may do a transaction twice.
		/// </remarks>
		/// <returns></returns>
		public override T Dequeue()
		{
			while (true)
			{
				if (_writeChecksum.GetValue() > _fileStream.Position)
				{
					var tmp = (T)_formatter.Deserialize(_fileStream);
					_checksum.SetValue(_fileStream.Position);
					return tmp;
				}
				Thread.Sleep(1);
			}
		}

        public CheckSummedTransactionFileReader(string filename, string checksumName, IFormatter formatter) : base(filename, formatter)
		{
			if (checksumName == null)
			{
				throw new ArgumentNullException("ChecksumName");
			}
			var fi = new FileInfo(filename);
			var checksumfile = string.Format("{0}\\{1}.chk", fi.DirectoryName, checksumName);
			var writeCheksumFilename = string.Format("{0}\\{1}.chk", fi.DirectoryName, fi.Name);

            if (_logger.IsDebugEnabled)
            {
                _logger.Debug(string.Format("Using checksum file: {0}", checksumfile));
            }
			
            _checksum = new FileChecksum(checksumfile);
			_writeChecksum = new FileChecksum(writeCheksumFilename);

            if (_logger.IsDebugEnabled)
            {
                _logger.Debug(string.Format("Setting initial position to: {0}", _checksum.GetValue()));
            }
			
            _fileStream.Seek(_checksum.GetValue(), SeekOrigin.Begin);
		}
	}
}
