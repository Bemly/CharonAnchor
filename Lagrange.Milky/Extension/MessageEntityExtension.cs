using Lagrange.Core.Message.Entities;

namespace Lagrange.Milky.Extension;

public static class MessageEntityExtension
{
    public static string ToDebugString(this IMessageEntity entity) => entity switch
    {
        ImageEntity image => $"Image {{ Url: {image.FileUrl} }}",
        MentionEntity mention => $"Mention {{ Uin: {mention.Uin} }}",
        MultiMsgEntity multi => $"MultiMsg {{ Id: {multi.ResId} }}",
        RecordEntity record => $"Record {{ Url: {record.FileUrl} }}",
        ReplyEntity reply => $"Reply {{ Sequence: {reply.SrcSequence} }}",
        TextEntity text => $"Text {{ Text: {text.Text} }}",
        VideoEntity video => $"Video {{ Url: {video.FileUrl} }}",
        _ => entity.GetType().Name,
    };
}