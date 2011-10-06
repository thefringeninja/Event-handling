using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using pvc.Adapters.TransactionFile.Checksums;

namespace pvc.Adapters.TransactionFile.Queues.TransactionFile
{
	/// <summary>
	/// Handles reading transactions from a transaction file, updating a persistent checksum to allow for recovery
	/// from various problems such as application shutdown.
	/// <remarks>
	///     - These processes will rely on an internal checksum from a paired writer to 'see' outside writes
	///     - The specified checksum is to manage an interrupted read, and is not related to writes
	/// </remarks>
	/// </summary>
	/// <typeparam name="T">The type of the objects which are to be read from the transaction file.
    /// Usually this will be a shared base class or interface but for untyped access, it could just be <code>object</code></typeparam>
	internal class CheckSummedTransactionFileReader<T> : TransactionFileReader<T>
	{
	    private readonly object _sync = new object();
		private readonly FileChecksum _readSum;
	    private readonly FileChecksum _writeSum;

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
	            var value = _writeSum.GetValue();
	            if (value > FileStream.Position)
	            {
	                lock (_sync) // Locking outside blocking can cause deadlocks!
	                {
	                    var item = (T) Formatter.Deserialize(FileStream);
	                    _readSum.SetValue(FileStream.Position);
	                    return item;
	                }
	            }
	            Thread.Sleep(1);
	        }
	    }

	    public CheckSummedTransactionFileReader(string filename, string checksumName, IFormatter formatter) : base(filename, formatter)
		{
			if (checksumName == null)
			{
				throw new ArgumentNullException("checksumName");
			}

			var fi = new FileInfo(filename);
			var readFile = string.Format(TransactionFile.ReadChecksumMask, fi.DirectoryName, checksumName);
            Logger.DebugFormat("Using read checksum file: {0}", readFile);
			_readSum = new FileChecksum(readFile);

            var writeFile = string.Format(TransactionFile.WriteChecksumMask, fi.DirectoryName, fi.Name);
            Logger.DebugFormat("Using write checksum file: {0}", writeFile);
            _writeSum = new FileChecksum(writeFile);
            
            SetInitialPosition();
		}

	    private void SetInitialPosition()
	    {
	        var position = _readSum.GetValue();
            Logger.DebugFormat("Setting initial read position to: {0}", position);
	        FileStream.Seek(position, SeekOrigin.Begin);
	    }
	}
}
