using Lagrange.Core.Common;
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
        return Task.FromResult(new GroupFSUploadEventResp(upload.BoolFileExist, upload.StrFileId, upload.BytesFileKey, upload.BytesCheckKey, (upload.StrUploadIp, upload.Uint32UploadPort)));
    }
}