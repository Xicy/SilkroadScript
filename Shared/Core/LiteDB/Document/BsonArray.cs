using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LiteDB
{
    public class BsonArray : BsonValue, IEnumerable<BsonValue>
    {
        public BsonArray()
            : base(new List<BsonValue>())
        {
        }

        public BsonArray(List<BsonValue> array)
            : base(array)
        {
            if (array == null) throw new ArgumentNullException("array");
        }

        public BsonArray(BsonValue[] array)
            : base(new List<BsonValue>(array))
        {
            if (array == null) throw new ArgumentNullException("array");
        }

        public BsonArray(IEnumerable<BsonValue> items)
            : this()
        {
            AddRange(items);
        }

        public BsonArray(IEnumerable<BsonArray> items)
            : this()
        {
            AddRange(items);
        }

        public BsonArray(IEnumerable<BsonDocument> items)
            : this()
        {
            AddRange(items);
        }

        public virtual BsonValue this[int index]
        {
            get { return RawValue.ElementAt(index); }
            set { RawValue[index] = value ?? Null; }
        }

        public virtual int Count
        {
            get { return RawValue.Count; }
        }

        public new List<BsonValue> RawValue
        {
            get { return (List<BsonValue>) base.RawValue; }
        }

        public virtual IEnumerator<BsonValue> GetEnumerator()
        {
            foreach (var value in RawValue)
            {
                yield return new BsonValue(value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void AddRange<T>(IEnumerable<T> array)
            where T : BsonValue
        {
            if (array == null) throw new ArgumentNullException("array");

            foreach (var item in array)
            {
                Add(item ?? Null);
            }
        }

        public virtual BsonArray Add(BsonValue value)
        {
            RawValue.Add(value ?? Null);

            return this;
        }

        public virtual void Remove(int index)
        {
            RawValue.RemoveAt(index);
        }

        public override int CompareTo(BsonValue other)
        {
            // if types are diferent, returns sort type order
            if (other.Type != BsonType.Document) return Type.CompareTo(other.Type);

            var otherArray = other.AsArray;

            var result = 0;
            var i = 0;
            var stop = Math.Min(Count, otherArray.Count);

            // compare each element
            for (; 0 == result && i < stop; i++)
                result = this[i].CompareTo(otherArray[i]);

            if (result != 0) return result;
            if (i == Count) return i == otherArray.Count ? 0 : -1;
            return 1;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, false, true);
        }
    }
}