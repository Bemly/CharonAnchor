using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.NativeModel.Message
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupStruct
    {
        public BotGroupStruct() { }

        public long GroupUin = 0;

        public byte[] GroupName = [];

        public int MemberCount = 0;
        
        public int MaxMember = 0;

        public long CreateTime = 0;

        public byte[] Description = [];

        public byte[] Question = [];

        public byte[] Announcement = [];
    }
}