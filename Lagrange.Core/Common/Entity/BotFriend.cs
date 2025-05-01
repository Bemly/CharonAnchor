namespace Lagrange.Core.Common.Entity;

public class BotFriend(long uin, string nickname, string uid, string remarks, string personalSign, string qid, BotFriendCategory category) : BotContact
{
    public override long Uin { get; } = uin;

    public override string Nickname { get; } = nickname;

    public override string Uid { get; } = uid;

    public string Remarks { get; } = remarks;

    public string PersonalSign { get; } = personalSign;

    public string Qid { get; } = qid;
    
    public BotFriendCategory Category { get; } = category;
}