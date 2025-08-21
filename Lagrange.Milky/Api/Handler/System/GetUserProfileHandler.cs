using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.System;

[Api("get_user_profile")]
public class GetUserProfileHandler(BotContext bot) : IApiHandler<GetUserProfileParameter, GetUserProfileResult>
{
    private readonly BotContext _bot = bot;

    public async Task<GetUserProfileResult> HandleAsync(GetUserProfileParameter parameter, CancellationToken token)
    {
        var stranger = await _bot.FetchStranger(parameter.UserId);
        return new GetUserProfileResult(
            stranger.Nickname,
            stranger.QID,
            (int)stranger.Age,
            stranger.Gender switch
            {
                Lagrange.Core.Common.BotGender.Male => "male",
                Lagrange.Core.Common.BotGender.Female => "female",
                _ => "unknown",
            },
            stranger.Remark,
            stranger.PersonalSign,
            (int)stranger.Level,
            stranger.Country,
            stranger.City,
            stranger.School ?? string.Empty
        );
    }
}

public class GetUserProfileParameter(long userId)
{
    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;
}

public class GetUserProfileResult(string nickname, string qid, int age, string sex, string remark, string bio, int level, string country, string city, string school)
{
    [JsonPropertyName("nickname")]
    public string Nickname { get; } = nickname;

    [JsonPropertyName("qid")]
    public string Qid { get; } = qid;

    [JsonPropertyName("age")]
    public int Age { get; } = age;

    [JsonPropertyName("sex")]
    public string Sex { get; } = sex;

    [JsonPropertyName("remark")]
    public string Remark { get; } = remark;

    [JsonPropertyName("bio")]
    public string Bio { get; } = bio;

    [JsonPropertyName("level")]
    public int Level { get; } = level;

    [JsonPropertyName("country")]
    public string Country { get; } = country;

    [JsonPropertyName("city")]
    public string City { get; } = city;

    [JsonPropertyName("school")]
    public string School { get; } = school;
}