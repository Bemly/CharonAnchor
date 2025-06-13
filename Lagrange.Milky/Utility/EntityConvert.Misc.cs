using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Entity;
using Lagrange.Milky.Extension;

namespace Lagrange.Milky.Utility;

public partial class EntityConvert
{
    public Friend Friend(BotFriend friend) => new(
        friend.Uin,
        friend.Qid,
        friend.Nickname,
        friend.Gender switch
        {
            BotGender.Male => "male",
            BotGender.Female => "female",
            BotGender.Unset or
            BotGender.Unknown => "unknown",
            _ => throw new NotSupportedException(),
        },
        friend.Remarks,
        FriendCategory(friend.Category)
    );

    private FriendCategory FriendCategory(BotFriendCategory category) => new(category.Id, category.Name);

    public Group Group(BotGroup group) => new(group.GroupUin, group.GroupName, group.MemberCount, group.MaxMember);

    public async Task<GroupMember> GroupMemberAsync(BotGroupMember member, CancellationToken token)
    {
        var user = (await _bot.FetchFriends().WaitAsync(token)).FirstOrDefault(friend => friend.Uin == member.Uin);
        if (user == null)
        {
            // TODO: Fallback to the API for getting stranger information
            // friend = ...
        }

        return new(
            member.Uin,
            user?.Qid ?? null,
            member.Nickname,
            member.Gender switch
            {
                BotGender.Male => "male",
                BotGender.Female => "female",
                BotGender.Unset or
                BotGender.Unknown => "unknown",
                _ => throw new NotSupportedException(),
            },
            member.Group.Uin,
            member.MemberCard ?? string.Empty,
            member.SpecialTitle,
            member.GroupLevel,
            member.Permission switch
            {
                GroupMemberPermission.Member => "member",
                GroupMemberPermission.Owner => "owner",
                GroupMemberPermission.Admin => "admin",
                _ => throw new NotSupportedException(),
            },
            member.JoinTime.ToUnixTimeSeconds(),
            member.LastMsgTime.ToUnixTimeSeconds()
        );
    }
}