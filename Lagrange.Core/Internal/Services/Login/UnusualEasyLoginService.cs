using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Struct;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<UnusualEasyLoginEventReq>(Protocols.PC)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginEasyLoginUnusualDevice", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class UnusualEasyLoginService : BaseService<UnusualEasyLoginEventReq, UnusualEasyLoginEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(UnusualEasyLoginEventReq input, BotContext context)
    {
        if (context.Keystore.WLoginSigs.A1 is not { Length: > 0 } a1) throw new InvalidOperationException("A1 is not set");

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.Encode(context, a1, null));
    }

    protected override ValueTask<UnusualEasyLoginEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var state = NTLoginCommon.Decode(context, input, out var info, out var resp);
    
        return new ValueTask<UnusualEasyLoginEventResp>(state switch
        {
            NTLoginCommon.State.LOGIN_ERROR_SUCCESS => new UnusualEasyLoginEventResp(state, null),
            _ when info is not null => new UnusualEasyLoginEventResp(state, (info.TipsTitle, info.TipsContent)),
            _ => new UnusualEasyLoginEventResp(state, null)
        });
    }
}