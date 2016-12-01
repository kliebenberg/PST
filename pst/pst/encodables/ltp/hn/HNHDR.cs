﻿namespace pst.encodables.ltp.hn
{
    class HNHDR
    {
        ///2
        public int PageMapOffset { get; }

        ///1
        public int BlockSignature { get; }

        ///1
        public int ClientSignature { get; }

        ///4
        public HID UserRoot { get; }

        ///4
        public int FillLevel { get; }

        public HNHDR(int pageMapOffset, int blockSignature, int clientSignature, HID userRoot, int fillLevel)
        {
            PageMapOffset = pageMapOffset;
            BlockSignature = blockSignature;
            ClientSignature = clientSignature;
            UserRoot = userRoot;
            FillLevel = fillLevel;
        }
    }
}
