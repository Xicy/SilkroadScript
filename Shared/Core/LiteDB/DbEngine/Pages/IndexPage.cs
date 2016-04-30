using System.Collections.Generic;
using System.Linq;

namespace LiteDB
{
    internal class IndexPage : BasePage
    {
        /// <summary>
        ///     If a Index Page has less that this free space, it's considered full page for new items.
        /// </summary>
        public const int INDEX_RESERVED_BYTES = 100;

        public IndexPage(uint pageID)
            : base(pageID)
        {
            Nodes = new Dictionary<ushort, IndexNode>();
        }

        /// <summary>
        ///     Page type = Index
        /// </summary>
        public override PageType PageType
        {
            get { return PageType.Index; }
        }

        public Dictionary<ushort, IndexNode> Nodes { get; set; }

        /// <summary>
        ///     Update freebytes + items count
        /// </summary>
        public override void UpdateItemCount()
        {
            ItemCount = Nodes.Count;
            FreeBytes = PAGE_AVAILABLE_BYTES - Nodes.Sum(x => x.Value.Length);
        }

        #region Read/Write pages

        protected override void ReadContent(ByteReader reader)
        {
            Nodes = new Dictionary<ushort, IndexNode>(ItemCount);

            for (var i = 0; i < ItemCount; i++)
            {
                var index = reader.ReadUInt16();
                var levels = reader.ReadByte();

                var node = new IndexNode(levels);

                node.Page = this;
                node.Position = new PageAddress(PageID, index);
                node.KeyLength = reader.ReadUInt16();
                node.Key = reader.ReadBsonValue(node.KeyLength);
                node.DataBlock = reader.ReadPageAddress();

                for (var j = 0; j < node.Prev.Length; j++)
                {
                    node.Prev[j] = reader.ReadPageAddress();
                    node.Next[j] = reader.ReadPageAddress();
                }

                Nodes.Add(node.Position.Index, node);
            }
        }

        protected override void WriteContent(ByteWriter writer)
        {
            foreach (var node in Nodes.Values)
            {
                writer.Write(node.Position.Index); // node Index on this page
                writer.Write((byte) node.Prev.Length); // level length
                writer.Write(node.KeyLength); // valueLength
                writer.WriteBsonValue(node.Key, node.KeyLength); // value
                writer.Write(node.DataBlock); // data block reference

                for (var j = 0; j < node.Prev.Length; j++)
                {
                    writer.Write(node.Prev[j]);
                    writer.Write(node.Next[j]);
                }
            }
        }

        #endregion Read/Write pages
    }
}