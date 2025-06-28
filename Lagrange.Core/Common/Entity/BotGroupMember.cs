namespace Lagrange.Core.Common.Entity;

public class BotGroupMember(
    BotGroup group,
    long uin,
    string uid,
    string nickname,
    GroupMemberPermission permission,
    int groupLevel,
    string? memberCard,
    string? specialTitle,
    DateTime joinTime,
    DateTime lastMsgTime,
    DateTime shutUpTimestamp) : BotContact
{
    public BotGroup Group { get; } = group;
    
    public override long Uin { get; } = uin;

    public override string Uid { get; } = uid ?? string.Empty;

    public override string Nickname { get; } = nickname ?? string.Empty;
    
    public int Age { get; init; }
    
    public BotGender Gender { get; init; }

    public GroupMemberPermission Permission { get; } = permission;

    public int GroupLevel { get; } = groupLevel;

    public string? MemberCard { get; } = memberCard;

    public string? SpecialTitle { get; } = specialTitle;

    public DateTime JoinTime { get; } = joinTime;

    public DateTime LastMsgTime { get; } = lastMsgTime;

    public DateTime ShutUpTimestamp { get; } = shutUpTimestamp;
}

public enum GroupMemberPermission
{
    Member,
    Owner,
    Admin,
}