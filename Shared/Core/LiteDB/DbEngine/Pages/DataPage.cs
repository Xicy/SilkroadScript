using System.Collections.Generic;
using System.Linq;

namespace LiteDB
{
    /// <summary>
    ///     The DataPage thats stores object data.
    /// </summary>
    internal class DataPage : BasePage
    {
        /// <summary>
        ///     If a Data Page has less that free space, it's considered full page for new items. Can be used only for update
        ///     (DataPage) ~ 50% PAGE_AVAILABLE_BYTES
        ///     This value is used for minimize
        /// </summary>
        public const int DATA_RESERVED_BYTES = PAGE_AVAILABLE_BYTES/2;

        public DataPage(uint pageID)
            : base(pageID)
        {
            DataBlocks = new Dictionary<ushort, DataBlock>();
        }

        /// <summary>
        ///     Page type = Extend
        /// </summary>
        public override PageType PageType
        {
            get { return PageType.Data; }
        }

        /// <summary>
        ///     Returns all data blocks - Each block has one object
        /// </summary>
        public Dictionary<ushort, DataBlock> DataBlocks { get; set; }

        /// <summary>
        ///     Update freebytes + items count
        /// </summary>
        public override void UpdateItemCount()
        {
            ItemCount = (ushort) DataBlocks.Count;
            FreeBytes = PAGE_AVAILABLE_BYTES - DataBlocks.Sum(x => x.Value.Length);
        }

        #region Read/Write pages

        protected override void ReadContent(ByteReader reader)
        {
            DataBlocks = new Dictionary<ushort, DataBlock>(ItemCount);

            for (var i = 0; i < ItemCount; i++)
            {
                var block = new DataBlock();

                block.Page = this;
                block.Position = new PageAddress(PageID, reader.ReadUInt16());
                block.ExtendPageID = reader.ReadUInt32();

                for (var j = 0; j < CollectionIndex.INDEX_PER_COLLECTION; j++)
                {
                    block.IndexRef[j] = reader.ReadPageAddress();
                }

                var size = reader.ReadUInt16();
                block.Data = reader.ReadBytes(size);

                DataBlocks.Add(block.Position.Index, block);
            }
        }

        protected override void WriteContent(ByteWriter writer)
        {
            foreach (var block in DataBlocks.Values)
            {
                writer.Write(block.Position.Index);
                writer.Write(block.ExtendPageID);
                foreach (var idx in block.IndexRef)
                {
                    writer.Write(idx);
                }
                writer.Write((ushort) block.Data.Length);
                writer.Write(block.Data);
            }
        }

        #endregion Read/Write pages
    }
}