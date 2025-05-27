using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Milky.Implementation.Entity;

namespace Lagrange.Milky.Implementation.Utility;

public partial class Converter
{
    public Friend Friend(BotFriend friend) => new()
    {
        UserId = friend.Uin,
        Qid = friend.Qid,
        Nickname = friend.Nickname,
        Sex = friend.Gender switch
        {
            BotGender.Male => "male",
            BotGender.Female => "female",
            BotGender.Unset or
                BotGender.Unknown => "unknown",
            _ => throw new NotSupportedException(),
        },
        Remark = friend.Remarks,
        Category = FriendCategory(friend.Category),
    };

    public FriendCategory FriendCategory(BotFriendCategory category) => new()
    {
        CategoryId = category.Id,
        CategoryName = category.Name,
    };

    public Group Group(BotGroup group) => new()
    {
        GroupId = group.Uin,
        Name = group.GroupName,
        MemberCount = group.MemberCount,
        MaxMemberCount = group.MaxMember,
    };

    public GroupMember GroupMember(BotGroupMember member) => new()
    {
        GroupId = member.Group.Uin,
        UserId = member.Uin,
        Nickname = member.Nickname,
        Card = member.MemberCard ?? string.Empty,
        Title = member.SpecialTitle,
        Sex = member.Gender switch
        {
            BotGender.Male => "male",
            BotGender.Female => "female",
            BotGender.Unset or
            BotGender.Unknown => "unknown",
            _ => throw new NotSupportedException(),
        },
        Level = member.GroupLevel,
        Role = member.Permission switch
        {
            GroupMemberPermission.Member => "member",
            GroupMemberPermission.Owner => "owner",
            GroupMemberPermission.Admin => "admin",
            _ => throw new NotImplementedException(),
        },
        JoinTime = new DateTimeOffset(member.JoinTime).ToUnixTimeSeconds(),
        LastSentTime = new DateTimeOffset(member.LastMsgTime).ToUnixTimeSeconds(),
    };
}