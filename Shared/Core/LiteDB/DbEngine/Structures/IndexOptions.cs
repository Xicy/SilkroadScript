using System;

namespace LiteDB
{
    /// <summary>
    ///     A class that represent all index options used on a index creation
    /// </summary>
    public class IndexOptions : IEquatable<IndexOptions>
    {
        public IndexOptions()
        {
            Unique = false;
            IgnoreCase = true;
            TrimWhitespace = true;
            EmptyStringToNull = true;
            RemoveAccents = true;
        }

        /// <summary>
        ///     Unique keys?
        /// </summary>
        public bool Unique { get; set; }

        /// <summary>
        ///     Ignore case? (convert all strings to lowercase)
        /// </summary>
        public bool IgnoreCase { get; set; }

        /// <summary>
        ///     Remove all whitespace on start/end string?
        /// </summary>
        public bool TrimWhitespace { get; set; }

        /// <summary>
        ///     Convert all empty string to null?
        /// </summary>
        public bool EmptyStringToNull { get; set; }

        /// <summary>
        ///     Removing accents on string?
        /// </summary>
        public bool RemoveAccents { get; set; }

        public bool Equals(IndexOptions other)
        {
            return Unique == other.Unique &&
                   IgnoreCase == other.IgnoreCase &&
                   TrimWhitespace == other.TrimWhitespace &&
                   EmptyStringToNull == other.EmptyStringToNull &&
                   RemoveAccents == other.RemoveAccents;
        }

        public IndexOptions Clone()
        {
            return new IndexOptions
            {
                Unique = Unique,
                IgnoreCase = IgnoreCase,
                TrimWhitespace = TrimWhitespace,
                EmptyStringToNull = EmptyStringToNull,
                RemoveAccents = RemoveAccents
            };
        }
    }
}