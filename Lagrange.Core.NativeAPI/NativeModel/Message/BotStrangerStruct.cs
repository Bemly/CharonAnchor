using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.NativeAPI.NativeModel.Common;

namespace Lagrange.Core.NativeAPI.NativeModel.Message
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotStrangerStruct
    {
        public BotStrangerStruct() { }
        public long Uin = 0;
        public ByteArrayNative Nickname = new();
        public ByteArrayNative Uid = new();
        public long Source = 0;
        
        public static implicit operator BotStranger(BotStrangerStruct stranger)
        {
            return new BotStranger(
                stranger.Uin,
                Encoding.UTF8.GetString(stranger.Nickname),
                Encoding.UTF8.GetString(stranger.Uid)
            );
        }
        
        public static implicit operator BotStrangerStruct(BotStranger stranger)
        {
            return new BotStrangerStruct()
            {
                Uin = stranger.Uin,
                Nickname = Encoding.UTF8.GetBytes(stranger.Nickname),
                Uid = Encoding.UTF8.GetBytes(stranger.Uid),
                Source = stranger.Source
            };
        }
    }
}