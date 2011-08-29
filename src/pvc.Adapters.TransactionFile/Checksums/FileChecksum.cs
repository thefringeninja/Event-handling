using System;
using System.IO;

namespace pvc.Adapters.TransactionFile.Checksums
{
    public class FileChecksum : IChecksum
    {
        private readonly string _filename;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly FileStream _fileStream;

        public FileChecksum(string filename)
        {
            _filename = filename;
            var newFile = false;
            if (File.Exists(filename))
            {
                _fileStream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            else
            {
                _fileStream = File.Open(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                newFile = true;
            }
            _reader = new StreamReader(_fileStream);
            _writer = new StreamWriter(_fileStream);
            if (newFile || _fileStream.Length == 0)
            {
                Reset();
            }
        }

        #region IChecksum Members

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

        #endregion
    }
}
