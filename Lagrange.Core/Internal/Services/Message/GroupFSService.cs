using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility.Cryptography;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<GroupFSUploadEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x6d6_0")]
internal class GroupFSUploadService : OidbService<GroupFSUploadEventReq, GroupFSUploadEventResp, D6D6ReqBody, D6D6RspBody>
{
    private protected override uint Command => 0x6d6;

    private protected override uint Service => 0;
    
    private protected override Task<D6D6ReqBody> ProcessRequest(GroupFSUploadEventReq request, BotContext context)
    {
        return Task.FromResult(new D6D6ReqBody
        {
            UploadFileReq = new UploadFileReqBody
            {
                Uint64GroupCode = (ulong)request.GroupUin,
                Uint32AppId = 7,
                Uint32BusId = 102,
                Uint32Entrance = 6,
                StrParentFolderId = request.ParentDirectory,
                StrFileName = request.FileName,
                StrLocalPath = $"/{request.FileName}",
                Uint64FileSize = (ulong)request.Stream.Length,
                BytesSha = request.Stream.Sha1(),
                BytesSha3 = TriSha1Provider.CalculateTriSha1(request.Stream),
                BytesMd5 = request.FileMd5,
            }
        });
    }

    private protected override Task<GroupFSUploadEventResp> ProcessResponse(D6D6RspBody response, BotContext context)
    {
        var upload = response.UploadFileRsp;
        if (upload.Int32RetCode != 0) throw new OperationException(upload.Int32RetCode, upload.StrRetMsg);
        
        return Task.FromResult(new GroupFSUploadEventResp(upload.BoolFileExist, upload.StrFileId, upload.BytesFileKey, upload.BytesCheckKey, (upload.StrUploadIp, upload.Uint32UploadPort)));
    }
}

[EventSubscribe<GroupFSDownloadEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x6d6_2")]
internal class GroupFSDownloadService : OidbService<GroupFSDownloadEventReq, GroupFSDownloadEventResp, D6D6ReqBody, D6D6RspBody>
{
    private protected override uint Command => 0x6d6;

    private protected override uint Service => 2;

    private protected override Task<D6D6ReqBody> ProcessRequest(GroupFSDownloadEventReq request, BotContext context)
    {
        return Task.FromResult(new D6D6ReqBody
        {
            DownloadFileReq = new DownloadFileReqBody
            {
                Uint64GroupCode = (ulong)request.GroupUin,
                Uint32BusId = 102,
                Uint32AppId = 7,
                StrFileId = request.FileId
            }
        });
    }

    private protected override Task<GroupFSDownloadEventResp> ProcessResponse(D6D6RspBody response, BotContext context)
    {
        var download = response.DownloadFileRsp;
        if (download.Int32RetCode != 0) throw new OperationException(download.Int32RetCode, download.StrRetMsg);
        
        string url = $"https://{download.StrDownloadDns}/ftn_handler/{Convert.ToHexString(download.BytesDownloadUrl)}/?fname=";
        return Task.FromResult(new GroupFSDownloadEventResp(url));
    }
}


[EventSubscribe<GroupFSDeleteEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x6d6_3")]
internal class
    GroupFSDeleteService : OidbService<GroupFSDeleteEventReq, GroupFSDeleteEventResp, D6D6ReqBody, D6D6RspBody>
{
    private protected override uint Command => 0x6d6;

    private protected override uint Service => 3;

    private protected override Task<D6D6ReqBody> ProcessRequest(GroupFSDeleteEventReq request, BotContext context)
    {
        return Task.FromResult(new D6D6ReqBody
        {
            DeleteFileReq = new DeleteFileReqBody
            {
                Uint64GroupCode = (ulong)request.GroupUin,
                Uint32AppId = 7,
                Uint32BusId = 102,
                StrFileId = request.FileId
            }
        });
    }

    private protected override Task<GroupFSDeleteEventResp> ProcessResponse(D6D6RspBody response, BotContext context)
    {
        var delete = response.DeleteFileRsp;
        if (delete.Int32RetCode != 0) throw new OperationException(delete.Int32RetCode, delete.StrRetMsg);

        return Task.FromResult(new GroupFSDeleteEventResp());
    }
}

[EventSubscribe<GroupFSMoveEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x6d6_5")]
internal class GroupFSMoveService : OidbService<GroupFSMoveEventReq, GroupFSMoveEventResp, D6D6ReqBody, D6D6RspBody>
{
    private protected override uint Command => 0x6d6;

    private protected override uint Service => 5;

    private protected override Task<D6D6ReqBody> ProcessRequest(GroupFSMoveEventReq request, BotContext context)
    {
        return Task.FromResult(new D6D6ReqBody
        {
            MoveFileReq = new MoveFileReqBody
            {
                Uint64GroupCode = (ulong)request.GroupUin,
                Uint32AppId = 7,
                Uint32BusId = 102,
                StrFileId = request.FileId,
                StrParentFolderId = request.ParentDirectory
            }
        });
    }

    private protected override Task<GroupFSMoveEventResp> ProcessResponse(D6D6RspBody response, BotContext context)
    {
        var move = response.MoveFileRsp;
        if (move.Int32RetCode != 0) throw new OperationException(move.Int32RetCode, move.StrRetMsg);

        return Task.FromResult(new GroupFSMoveEventResp());
    }
}