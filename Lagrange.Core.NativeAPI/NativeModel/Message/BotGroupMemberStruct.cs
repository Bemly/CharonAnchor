using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.NativeModel.Message
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupMemberStruct
    {
        public BotGroupMemberStruct() { }

        BotGroupStruct BotGroup = new();
        
        public long Uin = 0;

        public byte[] Uid = [];

        public byte[] Nickname = [];

        public int Age = 0;

        public int Gender = 0;

        public int Permission = 0;

        public int GroupLevel = 0;

        public byte[] MemberCard = [];

        public byte[] SpecialTitle = [];

        public byte[] JoinTime { get; } = [];

        public byte[] LastMsgTime { get; } = [];

        public byte[] ShutUpTimestamp { get; } = [];
    }
}
