using System.Runtime.InteropServices;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.NativeAPI.NativeModel.Message.Entity
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ReplyEntityStruct
    {
        public ReplyEntityStruct() { }

        public ulong SrcUid;
        
        public int SrcSequence;
        
        public IntPtr Source;

        public int SourceType;
        
        public static implicit operator ReplyEntityStruct(ReplyEntity entity)
        {
            var type = entity.Source switch
            {
                BotFriend => 1,
                BotGroupMember => 2,
                BotStranger => 3,
                _ => 0
            };
            
            var sourcePtr = type switch
            {
                1 => Marshal.AllocHGlobal(Marshal.SizeOf<BotFriend>()),
                2 => Marshal.AllocHGlobal(Marshal.SizeOf<BotGroupMember>()),
                3 => Marshal.AllocHGlobal(Marshal.SizeOf<BotStranger>()),
                _ => IntPtr.Zero
            };
            
            return new ReplyEntityStruct
            {
                SrcUid = entity.SrcUid,
                SrcSequence = entity.SrcSequence,
                Source = sourcePtr,
                SourceType = type
            };
        }
    }
}