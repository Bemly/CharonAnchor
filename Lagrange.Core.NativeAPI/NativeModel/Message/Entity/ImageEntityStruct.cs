using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.NativeModel.Message.Entity
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ImageEntityStruct : IEntityStruct
    {
        public ImageEntityStruct() { }
        
        public byte[] FileUrl = [];
        
        public byte[] FileName = [];
        
        public byte[] FileSha1 = [];
        
        public byte[] FileSize = [];
        
        public byte[] FileMd5 = [];
        
        public float[] ImageSize = [];
        
        public int SubType = 0;
        
        public byte[] Summary = [];

        public uint RecordLength = 0;
    }
}