using System;
using System.IO;
using System.Threading;

namespace pvc.Adapters.TransactionFile.Checksums
{
    internal class FileChecksum : IChecksum, IDisposable
    {
        private readonly string _filename;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly FileStream _fileStream;

        public FileChecksum(string filename)
        {
            _filename = filename;
            _fileStream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            _reader = new StreamReader(_fileStream);
            _writer = new StreamWriter(_fileStream);
            if (_fileStream.Length == 0)
            {
                Reset();
            }
        }

        public void SetValue(long value)
        {
            _fileStream.Seek(0, SeekOrigin.Begin);
            _writer.Write(value.ToString());
            _writer.Flush();
        }

        public long GetValue()
        {
            _fileStream.Seek(0, SeekOrigin.Begin);
            var val = _reader.ReadToEnd();
            return Convert.ToInt64(val);
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

            if(_fileStream != null)
            {
                _fileStream.Dispose();
            }
        }
    }
}