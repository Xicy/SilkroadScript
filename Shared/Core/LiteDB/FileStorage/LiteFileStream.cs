using System;
using System.IO;
using System.Linq;

namespace LiteDB
{
    public class LiteFileStream : Stream
    {
        private readonly DbEngine _engine;
        private byte[] _currentChunkData;

        private int _currentChunkIndex;
        private int _positionInChunk;

        private long _streamPosition;

        internal LiteFileStream(DbEngine engine, LiteFileInfo file)
        {
            _engine = engine;
            FileInfo = file;

            if (file.Length == 0)
            {
                throw LiteException.FileCorrupted(file);
            }

            _positionInChunk = 0;
            _currentChunkIndex = 0;
            _currentChunkData = GetChunkData(_currentChunkIndex);
        }

        /// <summary>
        ///     Get file information
        /// </summary>
        public LiteFileInfo FileInfo { get; }

        public override long Length { get; } = 0;

        public override bool CanRead
        {
            get { return true; }
        }

        public override long Position
        {
            get { return _streamPosition; }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesLeft = count;

            while (_currentChunkData != null && bytesLeft > 0)
            {
                var bytesToCopy = Math.Min(bytesLeft, _currentChunkData.Length - _positionInChunk);

                Buffer.BlockCopy(_currentChunkData, _positionInChunk, buffer, offset, bytesToCopy);

                _positionInChunk += bytesToCopy;
                bytesLeft -= bytesToCopy;
                offset += bytesToCopy;
                _streamPosition += bytesToCopy;

                if (_positionInChunk >= _currentChunkData.Length)
                {
                    _positionInChunk = 0;

                    _currentChunkData = GetChunkData(++_currentChunkIndex);
                }
            }

            return count - bytesLeft;
        }

        private byte[] GetChunkData(int index)
        {
            // check if there is no more chunks in this file
            var chunk = _engine
                .Find(LiteFileStorage.CHUNKS, Query.EQ("_id", LiteFileInfo.GetChunckId(FileInfo.Id, index)))
                .FirstOrDefault();

            // if chunk is null there is no more chunks
            return chunk == null ? null : chunk["data"].AsBinary;
        }

        #region Not supported operations

        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        #endregion Not supported operations
    }
}