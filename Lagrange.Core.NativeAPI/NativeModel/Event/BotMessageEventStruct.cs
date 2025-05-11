using Lagrange.Core.NativeAPI.NativeModel.Common;
using Lagrange.Core.NativeAPI.NativeModel.Message;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    public struct BotMessageEventStruct : IEventStruct
    {
        public BotMessageEventStruct() { }
        
        public BotMessageStruct Message = new();
        public byte[] RawMessage = [];
    }
}