namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    public struct BotCaptchaEventStruct : IEventStruct
    {
        public BotCaptchaEventStruct() { }
        
        public byte[] CaptchaUrl = [];
    }
}