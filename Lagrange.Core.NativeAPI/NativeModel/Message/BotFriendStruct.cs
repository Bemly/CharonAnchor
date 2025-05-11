using System.Runtime.InteropServices;
using Lagrange.Core.NativeAPI.NativeModel.Common;

namespace Lagrange.Core.NativeAPI.NativeModel.Message
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotFriendStruct
    {
        public BotFriendStruct() { }

        public long Uin = 0;

        public byte[] Nickname = [];

        public byte[] Uid = [];

        public int Age = 0;

        public int Gender = 0;

        public byte[] Remarks = [];

        public byte[] PersonalSign = [];

        public byte[] Qid = [];

        public BotFriendCategoryStruct Category = new();
    }
}
