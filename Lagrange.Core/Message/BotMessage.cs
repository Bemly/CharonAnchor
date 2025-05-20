using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Message;

public partial class BotMessage
{
    internal BotMessage(BotContact contact, BotContact receiver)
    {
        Contact = contact;
        Receiver = receiver;
    }
    
    internal BotMessage(MessageChain chain, BotContact contact, BotContact receiver)
    {
        Entities = chain;
        Contact = contact;
        Receiver = receiver;
    }
    
    public BotContact Contact { get; }
    
    public BotContact Receiver { get; }
    
    public MessageType Type => Contact switch
    {
        BotGroupMember _ => MessageType.Group,
        BotFriend _ => MessageType.Private,
        BotStranger _ => MessageType.Temp,
        _ => throw new ArgumentOutOfRangeException(nameof(Contact))
    };
    
    public DateTime Time { get; set; } = DateTime.Now;

    public MessageChain Entities { get; } = [];
    
    internal ulong MessageId { get; set; }
    
    internal uint Random { get; init; }
    
    internal int Sequence { get; set; }
    
    internal int ClientSequence { get; init; } = new Random().Next(10000, 99999);
}