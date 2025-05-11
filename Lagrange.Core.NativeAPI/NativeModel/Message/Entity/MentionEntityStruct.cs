using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.NativeModel.Message.Entity
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MentionEntityStruct : IEntityStruct
    {
        public MentionEntityStruct() { }
        public long Uin = 0;
        public byte[] Display = [];
    }
}