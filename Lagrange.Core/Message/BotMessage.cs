using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Message;

public partial class BotMessage
{
    public BotContact Contact { get; }
    
    public MessageType Type => Contact switch
    {
        BotGroupMember _ => MessageType.Group,
        BotFriend _ => MessageType.Private,
        BotStranger _ => MessageType.Temp,
        _ => throw new ArgumentOutOfRangeException(nameof(Contact))
    };
    
    public DateTime Time { get; set; } = DateTime.Now;

    public List<IMessageEntity> Entities { get; } = [];
    
    internal ulong MessageId { get; set; } = 0;
    
    internal int Random { get; set; } = System.Random.Shared.Next();
    
    internal int Sequence { get; set; } = 0;
    
    internal int ClientSequence { get; set; } = new Random().Next(100000, 999999);
    
    internal BotMessage(BotContact contact)
    {
        Contact = contact;
    }
}