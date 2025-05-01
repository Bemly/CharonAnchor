namespace Lagrange.Core.Common.Entity;

public class BotStranger(long uin, string nickname, string uid) : BotContact
{
    public override long Uin { get; } = uin;
    
    public override string Nickname { get; } = nickname;
    
    public override string Uid { get; } = uid;
    
    public long Source { get; init; }
}