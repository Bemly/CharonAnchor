using Lagrange.Core.NativeAPI.NativeModel.Common;

namespace Lagrange.Core.NativeAPI.NativeModel.Context
{
    public struct BotKeystoreStruct
    {
        public BotKeystoreStruct() { }

        public long Uin { get; set; } = 0;

        public byte[] Uid { get; set; } = [];

        public byte[] Guid { get; set; } = [];

        public byte[] AndroidId { get; set; } = [];

        public byte[] Qimei { get; set; } = [];

        public byte[] DeviceName { get; set; } = [];

        // WLoginSigs

        public byte[] A2 { get; set; } = [];

        public byte[] A2Key { get; set; } = new byte[16];

        public byte[] D2 { get; set; } = [];

        public byte[] D2Key { get; set; } = new byte[16];

        public byte[] A1 { get; set; } = [];

        public byte[] A1Key { get; set; } = new byte[16];

        public byte[] NoPicSig { get; set; } = [];

        public byte[] TgtgtKey { get; set; } = [];

        public byte[] Ksid { get; set; } = [];

        public byte[] SuperKey { get; set; } = [];

        public byte[] StKey { get; set; } = [];

        public byte[] StWeb { get; set; } = [];

        public byte[] St { get; set; } = [];

        public byte[] WtSessionTicket { get; set; } = [];

        public byte[] WtSessionTicketKey { get; set; } = [];

        public byte[] RandomKey { get; set; } = new byte[16];

        public byte[] SKey { get; set; } = [];

        public KeyValuePairNative<byte[], byte[]>[] PsKey { get; set; } = [];
    }
}
