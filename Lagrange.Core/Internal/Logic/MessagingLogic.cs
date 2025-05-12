using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;

namespace Lagrange.Core.Internal.Logic;

internal class MessagingLogic(BotContext context) : ILogic
{
    private readonly MessagePacker _packer = new(context);
    
    public Task<BotMessage> Parse(byte[] data) => _packer.Parse(data);
    
    public Task<BotMessage> Parse(MsgPush msg) => _packer.Parse(msg);
    
    public async Task<BotMessage> SendGroupMessage(MessageChain chain, long groupUin)
    {
        var (group, member) = await context.CacheContext.ResolveMember(groupUin, context.BotUin) ?? throw new InvalidTargetException(context.BotUin, groupUin);
        var message = await BuildMessage(chain, member, group);
        var result = await context.EventContext.SendEvent<SendMessageEventResp>(new SendMessageEventReq(message));
        
        if (result == null) throw new InvalidOperationException();
        if (result.Result != 0) throw new OperationException(result.Result);
        
        message.Sequence = result.Sequence;
        message.Time = DateTimeOffset.FromUnixTimeSeconds(result.SendTime).DateTime;
        
        return message;
    }
    
    public async Task<BotMessage> SendFriendMessage(MessageChain chain, long friendUin)
    {
        var friend = await context.CacheContext.ResolveFriend(friendUin) ?? throw new InvalidTargetException(friendUin);
        var message = await BuildMessage(chain, friend, null);
        var result = await context.EventContext.SendEvent<SendMessageEventResp>(new SendMessageEventReq(message));

        if (result == null) throw new InvalidOperationException();
        if (result.Result != 0) throw new OperationException(result.Result);
        
        message.Sequence = result.Sequence;
        message.Time = DateTimeOffset.FromUnixTimeSeconds(result.SendTime).DateTime;
        
        return message;
    }

    private async Task<BotMessage> BuildMessage(MessageChain chain, BotContact contact, BotGroup? group)
    {
        uint random = (uint)Random.Shared.Next();
        var message = new BotMessage(chain, contact, group)
        {
            Random = random,
            MessageId = (0x10000000ul << 32) | random
        };
        
        foreach (var entity in chain)
        {
            await entity.Preprocess(context, message);
        }
        
        return message;
    }
}