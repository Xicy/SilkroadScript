using System;

namespace LiteDB
{
    /// <summary>
    ///     Represent a empty page (reused)
    /// </summary>
    internal class EmptyPage : BasePage
    {
        public EmptyPage(uint pageID)
            : base(pageID)
        {
            ItemCount = 0;
            FreeBytes = PAGE_AVAILABLE_BYTES;
        }

        public EmptyPage(BasePage page)
            : this(page.PageID)
        {
            // if page is not dirty but it´s changing to empty, lets copy disk content to add in journal
            if (!page.IsDirty && page.DiskData.Length > 0)
            {
                DiskData = new byte[PAGE_SIZE];
                Buffer.BlockCopy(page.DiskData, 0, DiskData, 0, PAGE_SIZE);
            }
        }

        /// <summary>
        ///     Page type = Empty
        /// </summary>
        public override PageType PageType
        {
            get { return PageType.Empty; }
        }

        /// <summary>
        ///     Update freebytes + items count
        /// </summary>
        public override void UpdateItemCount()
        {
            ItemCount = 0;
            FreeBytes = PAGE_AVAILABLE_BYTES;
        }

        #region Read/Write pages

        protected override void ReadContent(ByteReader reader)
        {
        }

        protected override void WriteContent(ByteWriter writer)
        {
        }

        #endregion Read/Write pages
    }
}