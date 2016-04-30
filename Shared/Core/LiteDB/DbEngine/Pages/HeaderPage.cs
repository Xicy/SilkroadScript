using System;
using System.Collections.Generic;

namespace LiteDB
{
    internal class HeaderPage : BasePage
    {
        /// <summary>
        ///     ChangeID in file position (can be calc?)
        /// </summary>
        public const int CHANGE_ID_POSITION = PAGE_HEADER_SIZE
                                              + 27 // HEADER_INFO
                                              + 1; // FILE_VERSION

        /// <summary>
        ///     Header info the validate that datafile is a LiteDB file (27 bytes)
        /// </summary>
        private const string HEADER_INFO = "** This is a LiteDB file **";

        /// <summary>
        ///     Datafile specification version
        /// </summary>
        private const byte FILE_VERSION = 5;

        /// <summary>
        ///     Get/Set the pageID that start sequenece with a complete empty pages (can be used as a new page)
        /// </summary>
        public uint FreeEmptyPageID;

        public HeaderPage()
            : base(0)
        {
            FreeEmptyPageID = uint.MaxValue;
            ChangeID = 0;
            LastPageID = 0;
            ItemCount = 1; // fixed for header
            FreeBytes = 0; // no free bytes on header
            DbParams = new DbParams();
            CollectionPages = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Page type = Header
        /// </summary>
        public override PageType PageType
        {
            get { return PageType.Header; }
        }

        /// <summary>
        ///     Get/Set the changeID of data. When a client read pages, all pages are in the same version. But when
        ///     OpenTransaction, we need validade that current changeID is the sabe that we have in cache
        /// </summary>
        public ushort ChangeID { get; set; }

        /// <summary>
        ///     Last created page - Used when there is no free page inside file
        /// </summary>
        public uint LastPageID { get; set; }

        /// <summary>
        ///     Database parameters stored in header page - Use 200 bytes fixed
        /// </summary>
        public DbParams DbParams { get; set; }

        /// <summary>
        ///     Get a dictionary with all collection pages with pageID link
        /// </summary>
        public Dictionary<string, uint> CollectionPages { get; set; }

        /// <summary>
        ///     Update freebytes + items count
        /// </summary>
        public override void UpdateItemCount()
        {
            ItemCount = 1; // fixed for header
            FreeBytes = 0; // no free bytes on header
        }

        #region Read/Write pages

        protected override void ReadContent(ByteReader reader)
        {
            var info = reader.ReadString(HEADER_INFO.Length);
            var ver = reader.ReadByte();

            if (info != HEADER_INFO) throw LiteException.InvalidDatabase();
            if (ver != FILE_VERSION) throw LiteException.InvalidDatabaseVersion(ver);

            ChangeID = reader.ReadUInt16();
            FreeEmptyPageID = reader.ReadUInt32();
            LastPageID = reader.ReadUInt32();
            DbParams.Read(reader);

            // read page collections references (position on end of page)
            var cols = reader.ReadByte();
            for (var i = 0; i < cols; i++)
            {
                CollectionPages.Add(reader.ReadString(), reader.ReadUInt32());
            }
        }

        protected override void WriteContent(ByteWriter writer)
        {
            writer.Write(HEADER_INFO, HEADER_INFO.Length);
            writer.Write(FILE_VERSION);
            writer.Write(ChangeID);
            writer.Write(FreeEmptyPageID);
            writer.Write(LastPageID);
            DbParams.Write(writer);

            writer.Write((byte) CollectionPages.Count);
            foreach (var key in CollectionPages.Keys)
            {
                writer.Write(key);
                writer.Write(CollectionPages[key]);
            }
        }

        #endregion Read/Write pages
    }
}