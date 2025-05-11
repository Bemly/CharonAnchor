using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.NativeModel.Message
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotStrangerStruct
    {
        public BotStrangerStruct() { }
        public long Uin = 0;
        public byte[] Nickname = [];
        public byte[] Uid = [];
        public int Source = 0;
    }
}