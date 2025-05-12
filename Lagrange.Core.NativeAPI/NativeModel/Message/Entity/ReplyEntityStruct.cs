using System.Runtime.InteropServices;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.NativeAPI.NativeModel.Message.Entity
{
    [StructLayout(LayoutKind.Sequential)]
    public class ReplyEntityStruct
    {
        public ReplyEntityStruct() { }
        
        public static implicit operator ReplyEntityStruct(ReplyEntity entity)
        {
            return new ReplyEntityStruct() { };
            throw new NotImplementedException();
        }
    }
}