using Lagrange.Core.Common.Entity;
using Lagrange.Milky.Implementation.Entity;

namespace Lagrange.Milky.Implementation.Utility;

public class EntityConvert
{
    public Friend Friend(BotFriend friend) => new()
    {
        UserId = friend.Uin,
        Qid = friend.Qid,
        Nickname = friend.Nickname,
        Remark = friend.Remarks,
        Category = FriendCategory(friend.Category),
    };

    public FriendCategory FriendCategory(BotFriendCategory category) => new()
    {
        CategoryId = category.Id,
        CategoryName = category.Name,
    };
}