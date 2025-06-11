using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Milky.Extension;
using Lagrange.Milky.Implementation.Entity;

namespace Lagrange.Milky.Implementation.Utility;

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

    public GroupMember GroupMember(BotGroupMember member) => new(
        member.Group.GroupUin,
        member.Uin,
        member.Nickname,
        member.MemberCard ?? string.Empty,
        member.SpecialTitle,
        member.Gender switch
        {
            BotGender.Male => "male",
            BotGender.Female => "female",
            BotGender.Unset or
            BotGender.Unknown => "unknown",
            _ => throw new NotSupportedException(),
        },
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