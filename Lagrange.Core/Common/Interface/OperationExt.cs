using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Logic;

namespace Lagrange.Core.Common.Interface;

public static class OperationExt
{
    public static Task<BotQrCodeInfo?> FetchQrCodeInfo(this BotContext context, byte[] k) => 
        context.EventContext.GetLogic<WtExchangeLogic>().FetchQrCodeInfo(k);
    
    public static Task<(bool Success, string Message)> CloseQrCode(this BotContext context, byte[] k, bool confirm) =>
        context.EventContext.GetLogic<WtExchangeLogic>().CloseQrCode(k, confirm);

    public static Task<List<BotFriend>> FetchFriends(this BotContext context, bool refresh = false) =>
        context.CacheContext.GetFriendList(refresh);

    public static Task<List<BotGroup>> FetchGroups(this BotContext context, bool refresh = false) =>
        context.CacheContext.GetGroupList(refresh);

    public static Task<List<BotGroupMember>> FetchMembers(this BotContext context, long groupUin, bool refresh = false) =>
        context.CacheContext.GetMemberList(groupUin, refresh);
}