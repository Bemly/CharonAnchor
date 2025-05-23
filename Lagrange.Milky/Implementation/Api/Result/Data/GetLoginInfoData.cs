namespace Lagrange.Milky.Implementation.Api.Result.Data;

public class GetLoginInfoData
{
    public required long Uin { get; init; }
    public required string Nickname { get; init; }
}