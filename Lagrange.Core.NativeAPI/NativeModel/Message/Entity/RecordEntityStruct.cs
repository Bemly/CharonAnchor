using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.NativeModel.Message.Entity
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RecordEntityStruct : IEntityStruct
    {
        public RecordEntityStruct() { }
        public byte[] FileUrl = [];

        public byte[] FileName = [];

        public byte[] FileSha1 = [];

        public byte[] FileSize = [];

        public byte[] FileMd5 = [];
    }
}