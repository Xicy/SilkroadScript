using System;

namespace LiteDB
{
    internal class LazyLoad<T>
        where T : class
    {
        private readonly Action _after = () => { };
        private readonly Action _before = () => { };
        private readonly Func<T> _factory;
        private readonly object _locker = new object();
        private T _value;

        public LazyLoad(Func<T> factory, Action before, Action after)
        {
            _factory = factory;
            _before = before;
            _after = after;
        }

        public bool IsValueCreated
        {
            get { return _value != null; }
        }

        public T Value
        {
            get
            {
                lock (_locker)
                {
                    if (_value == null)
                    {
                        _before();
                        _value = _factory();
                        _after();
                    }
                }

                return _value;
            }
        }
    }
}