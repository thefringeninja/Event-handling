using System;
using System.IO;

namespace pvc.Adapters.TransactionFile.Checksums
{
    public class FileChecksum : IChecksum, IDisposable
    {
        private static readonly object _sync = new object();

        private readonly string _filename;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly FileStream _writeStream;
        private readonly FileStream _readStream;
        
        public FileChecksum(string filename)
        {
            _filename = filename;
            _writeStream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            _readStream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);

            _reader = new StreamReader(_readStream);
            _writer = new StreamWriter(_writeStream);

            if (_writeStream.Length == 0)
            {
                Reset();
            }
        }

        public void SetValue(long value)
        {
            lock(_sync)
            {
                _writeStream.Seek(0, SeekOrigin.Begin);
                _writer.Write(value.ToString());
                _writer.Flush();
            }
        }

        public long GetValue()
        {
            lock(_sync)
            {
                _readStream.Seek(0, SeekOrigin.Begin);
                var val = _reader.ReadToEnd();
                return Convert.ToInt64(val);
            }
        }

        public void Reset()
        {
            SetValue(0);
        }

        public string Name { get { return _filename; } }

        public void Dispose()
        {
            if(_reader != null)
            {
                _reader.Dispose();
            }

            if(_writer != null)
            {
                _writer.Dispose();
            }

            if(_writeStream != null)
            {
                _writeStream.Dispose();
            }
        }
    }
}