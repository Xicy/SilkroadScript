namespace Shared.Core.Security
{
    internal class TransferBuffer
    {
        private byte[] _mBuffer;
        private readonly object _mLock;
        private int _mOffset;
        private int _mSize;

        public TransferBuffer(TransferBuffer rhs)
        {
            lock (rhs._mLock)
            {
                _mBuffer = new byte[rhs._mBuffer.Length];
                System.Buffer.BlockCopy(rhs._mBuffer, 0, _mBuffer, 0, _mBuffer.Length);
                _mOffset = rhs._mOffset;
                _mSize = rhs._mSize;
                _mLock = new object();
            }
        }

        public TransferBuffer()
        {
            _mBuffer = null;
            _mOffset = 0;
            _mSize = 0;
            _mLock = new object();
        }

        public TransferBuffer(int length, int offset, int size)
        {
            _mBuffer = new byte[length];
            _mOffset = offset;
            _mSize = size;
            _mLock = new object();
        }

        public TransferBuffer(int length)
        {
            _mBuffer = new byte[length];
            _mOffset = 0;
            _mSize = 0;
            _mLock = new object();
        }

        public TransferBuffer(byte[] buffer, int offset, int size, bool assign)
        {
            if (assign)
            {
                _mBuffer = buffer;
            }
            else
            {
                _mBuffer = new byte[buffer.Length];
                System.Buffer.BlockCopy(buffer, 0, _mBuffer, 0, buffer.Length);
            }
            _mOffset = offset;
            _mSize = size;
            _mLock = new object();
        }

        public byte[] Buffer
        {
            get { return _mBuffer; }
            set
            {
                lock (_mLock)
                {
                    _mBuffer = value;
                }
            }
        }

        public int Offset
        {
            get { return _mOffset; }
            set
            {
                lock (_mLock)
                {
                    _mOffset = value;
                }
            }
        }

        public int Size
        {
            get { return _mSize; }
            set
            {
                lock (_mLock)
                {
                    _mSize = value;
                }
            }
        }
    }
}