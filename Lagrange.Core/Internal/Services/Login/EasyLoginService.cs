using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Struct;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<EasyLoginEventReq>(Protocols.PC)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginEasyLogin", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class EasyLoginService : BaseService<EasyLoginEventReq, EasyLoginEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(EasyLoginEventReq input, BotContext context)
    {
        if (context.Keystore.WLoginSigs.A1 is not { Length: > 0 } a1) throw new InvalidOperationException("A1 is not set");

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.Encode(context, a1, null));
    }

    protected override ValueTask<EasyLoginEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var state = NTLoginCommon.Decode(context, input, out var info, out var resp);
        
        return new ValueTask<EasyLoginEventResp>(state switch
        {
            NTLoginCommon.State.LOGIN_ERROR_SUCCESS => new EasyLoginEventResp(state, null, null),
            NTLoginCommon.State.LOGIN_ERROR_UNUSUAL_DEVICE => new EasyLoginEventResp(state, null, resp.Unusual.Sig),
            _ when info is not null => new EasyLoginEventResp(state, (info.TipsTitle, info.TipsContent), null),
            _ => new EasyLoginEventResp(state, null, null)
        });
    }
}