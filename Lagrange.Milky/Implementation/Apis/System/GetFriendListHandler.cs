using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Apis.Params;
using Lagrange.Milky.Implementation.Apis.Results;
using Lagrange.Milky.Implementation.Entities;

namespace Lagrange.Milky.Implementation.Apis.System;

[Api("get_friend_list")]
public class GetFriendListHandler(BotContext bot) : IApiHandler<CachedParam>
{
    public async ValueTask<IApiResult> HandleAsync(CachedParam param, CancellationToken token)
    {
        var firends = (await bot.FetchFriends(param.NoCache ?? false))
            .Select(friend => new Friend
            {
                UserId = friend.Uin,
                Qid = friend.Qid,
                Nickname = friend.Nickname,
                Remark = friend.Remarks,
                Category = new FriendCategory
                {
                    CategoryId = friend.Category.Id,
                    CategoryName = friend.Category.Name,
                },
            });

        return new ApiOkResult<IEnumerable<Friend>>
        {
            Data = firends,
        };
    }
}