using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Internal.Packets.Service;

internal class NTV2RichMedia
{
    public static NTV2RichMediaReq BuildDownloadReq(BotMessage @struct, RichMediaEntityBase entity, DownloadExt ext)
    {
        if (entity.MsgInfo == null) throw new InvalidOperationException("MsgInfo is null");
        
        return new NTV2RichMediaReq
        {
            ReqHead = BuildHead(@struct, entity, 200),
            Download = new DownloadReq
            {
                Node = entity.MsgInfo.MsgInfoBody[0].Index,
                Download = ext
            }
        };
    }
    
    public static NTV2RichMediaReq BuildUploadReq(
        BotMessage @struct,
        RichMediaEntityBase entity, ExtBizInfo ext,
        params (uint, RichMediaEntityBase)[] subFileInfos)
    {
        if (entity.Stream == null) throw new InvalidOperationException("Stream is null");
        
        return new NTV2RichMediaReq
        {
            ReqHead = BuildHead(@struct, entity, 201),
            Upload = new UploadReq
            {
                UploadInfo = 
                [
                    new UploadInfo
                    {
                        FileInfo = BuildFileInfo(entity),
                        SubFileType = 0
                    }, 
                    ..subFileInfos.Select(x => new UploadInfo
                    { 
                        FileInfo = BuildFileInfo(x.Item2), 
                        SubFileType = x.Item1
                    })
                ],
                TryFastUploadCompleted = true,
                SrvSendMsg = false,
                ClientRandomId = (ulong)Random.Shared.Next(),
                CompatQMsgSceneType = 1,
                ClientSeq = 10,
                ExtBizInfo = ext,
                NoNeedCompatMsg = false
            }
        };
    }

    private static MultiMediaReqHead BuildHead(BotMessage @struct, RichMediaEntityBase entity, uint cmd)
    {
        var (request, business) = entity switch
        {
            ImageEntity => (2u, 1u),
            RecordEntity => (2u, 3u),
            VideoEntity => (2u, 2u),
            _ => throw new ArgumentOutOfRangeException(nameof(entity))
        };

        return new MultiMediaReqHead
        {
            Common = new CommonHead
            {
                RequestId = 1,
                Command = cmd
            },
            Scene = BuildSceneInfo(@struct.Contact, request, business),
            Client = new ClientMeta
            {
                AgentType = 2
            }
        };
    }
    
    private static SceneInfo BuildSceneInfo(BotContact contact, uint requestType, uint businessType)
    {
        var sceneInfo = new SceneInfo
        {
            RequestType = requestType,
            BusinessType = businessType,
        };
        
        if (contact is BotGroupMember member)
        {
            sceneInfo.SceneType = 2;
            sceneInfo.Group = new GroupInfo
            {
                GroupUin = member.Group.GroupUin,
            };
        }
        else
        {
            sceneInfo.SceneType = 1;
            sceneInfo.C2C = new C2CUserInfo
            {
                TargetUid = contact.Uid,
                AccountType = 2
            };
        }
        return sceneInfo;
    }

    private static FileInfo BuildFileInfo(RichMediaEntityBase entity)
    {
        if (entity.Stream == null) throw new InvalidOperationException("MsgInfo is null");
        
        var stream = entity.Stream.Value;
        string md5 = Convert.ToHexString(entity.Stream.Value.Md5());
        string sha1 = Convert.ToHexString(entity.Stream.Value.Sha1());
        var info = new FileInfo
        {
            FileSize = (uint)stream.Length,
            FileHash = md5,
            FileSha1 = sha1
        };

        switch (entity)
        {
            case ImageEntity image:
            {
                break;
            }
            case RecordEntity record:
            {
                break;
            }
            case VideoEntity video:
            {
                break;
            }
        }
        
        return info;
    }
}