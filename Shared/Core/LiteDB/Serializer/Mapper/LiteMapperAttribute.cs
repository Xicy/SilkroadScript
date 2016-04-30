using System;

namespace LiteDB
{
    /// <summary>
    ///     Set a name to this property in BsonDocument
    /// </summary>
    public class LiteMapperAttribute : Attribute
    {
        public string FieldName { get; set; }
        public AutoID AutoID { get; set; } = AutoID.Null;
        public bool Ignore { get; set; } = false;
        public bool Unique { get; set; } = false;
        public bool IgnoreCase { get; set; } = true;
        public bool EmptyStringToNull { get; set; } = true;
        public bool RemoveAccents { get; set; } = true;
        public bool TrimWhitespace { get; set; } = true;

        public IndexOptions Indexes => new IndexOptions
        {
            EmptyStringToNull = EmptyStringToNull,
            Unique = Unique,
            IgnoreCase = IgnoreCase,
            TrimWhitespace = TrimWhitespace,
            RemoveAccents = RemoveAccents
        };
    }

    public enum AutoID
    {
        False,
        Null,
        True
    }
}