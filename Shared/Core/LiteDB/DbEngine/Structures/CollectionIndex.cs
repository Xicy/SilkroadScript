using System.Text.RegularExpressions;

namespace LiteDB
{
    internal class CollectionIndex
    {
        /// <summary>
        ///     Total indexes per collection - it's fixed because I will used fixed arrays allocations
        /// </summary>
        public const int INDEX_PER_COLLECTION = 16;

        public static Regex IndexPattern = new Regex(@"[\w-$\.]+$");

        /// <summary>
        ///     Get a reference for the free list index page - its private list per collection/index (must be a Field to be used as
        ///     reference parameter)
        /// </summary>
        public uint FreeIndexPageID;

        public CollectionIndex()
        {
            Clear();
        }

        /// <summary>
        ///     Represent slot position on index array on dataBlock/collection indexes - non-persistable
        /// </summary>
        public int Slot { get; set; }

        /// <summary>
        ///     Field name
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        ///     Index options like unique and ignore case
        /// </summary>
        public IndexOptions Options { get; set; }

        /// <summary>
        ///     Head page address for this index
        /// </summary>
        public PageAddress HeadNode { get; set; }

        /// <summary>
        ///     A link pointer to tail node
        /// </summary>
        public PageAddress TailNode { get; set; }

        /// <summary>
        ///     Returns if this index slot is empty and can be used as new index
        /// </summary>
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(Field); }
        }

        /// <summary>
        ///     Get a reference for page
        /// </summary>
        public CollectionPage Page { get; set; }

        /// <summary>
        ///     Clear all index information
        /// </summary>
        public void Clear()
        {
            Field = string.Empty;
            Options = new IndexOptions();
            HeadNode = PageAddress.Empty;
            FreeIndexPageID = uint.MaxValue;
        }
    }
}