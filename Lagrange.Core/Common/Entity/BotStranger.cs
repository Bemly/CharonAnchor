namespace Lagrange.Core.Common.Entity;

public class BotStranger(long uin, string nickname, string uid) : BotContact
{
    public override long Uin { get; } = uin;
    
    public override string Nickname { get; } = nickname ?? string.Empty;
    
    public override string Uid { get; } = uid ?? string.Empty;
    
    public long Source { get; init; }
}