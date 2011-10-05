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
        private readonly object _sync = new object();
		
        protected FileStream FileStream;
		protected IFormatter Formatter;
	    protected ILog Logger;

		/// <summary>
		/// Reads the next item off of the stream, blocking if one is not available.
		/// </summary>
		/// <returns></returns>
		public virtual T Dequeue()
		{
            while (true)
            {
                if (FileStream.Position != FileStream.Length)
                {
                    lock (_sync) // Locking outside blocking can cause deadlocks!
                    {
                        var o = Formatter.Deserialize(FileStream);
                        return (T) o;
                    }
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
			
            Logger = LogManager.GetLogger(GetType());
			Formatter = formatter;
			var fi = new FileInfo(filename); 
			
            if (Logger.IsDebugEnabled)
			{
                Logger.Debug(string.Format("Opening Transaction File: {0}", filename));
			}
			
            FileStream = fi.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
		}
	}
}
