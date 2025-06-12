using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;
using Lagrange.Milky.Extension;
using Lagrange.Milky.Implementation.Entity.Message;

namespace Lagrange.Milky.Implementation.Utility;

public partial class EntityConvert
{
    public MessageBase MessageBase(BotMessage message) => message.Type switch
    {
        MessageType.Group => GroupMessage(message),
        MessageType.Private => FriendMessage(message),
        MessageType.Temp => TempMessage(message),
        _ => throw new NotSupportedException(),
    };

    public FriendMessage FriendMessage(BotMessage message) => new(
        message.Contact.Uin == _bot.BotUin ? message.Contact.Uin : message.Receiver.Uin,
        message.Sequence,
        message.Contact.Uin,
        message.Time.ToUnixTimeSeconds(),
        Segments(message.Entities),
        Friend((BotFriend)message.Contact),
        message.ClientSequence
    );

    public GroupMessage GroupMessage(BotMessage message) => new(
        ((BotGroupMember)message.Contact).Group.Uin,
        message.Sequence,
        message.Contact.Uin,
        message.Time.ToUnixTimeSeconds(),
        Segments(message.Entities),
        Group(((BotGroupMember)message.Contact).Group),
        GroupMember((BotGroupMember)message.Contact)
    );

    public TempMessage TempMessage(BotMessage message) => new(
        message.Contact.Uin == _bot.BotUin ? message.Contact.Uin : message.Receiver.Uin,
        message.Sequence,
        message.Contact.Uin,
        message.Time.ToUnixTimeSeconds(),
        Segments(message.Entities),
        null
    );
}