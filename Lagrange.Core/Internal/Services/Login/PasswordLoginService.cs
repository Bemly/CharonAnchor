using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Login;
using Lagrange.Core.Internal.Packets.Struct;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<PasswordLoginEventReq>(Protocols.PC)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLogin", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class PasswordLoginService : BaseService<PasswordLoginEventReq, PasswordLoginEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(PasswordLoginEventReq input, BotContext context)
    {
        var clientA1 = Tlv.GenerateClientA1(context.Keystore, context.AppInfo, input.Password);
        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.Encode(context, clientA1, input.Captcha));
    }

    protected override ValueTask<PasswordLoginEventResp?> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var state = NTLoginCommon.Decode(context, input, out var info, out var resp);
        
        return new ValueTask<PasswordLoginEventResp?>(state switch
        {
            NTLoginCommon.State.LOGIN_ERROR_SUCCESS => new PasswordLoginEventResp(state, null, null),
            NTLoginCommon.State.LOGIN_ERROR_PROOFWATER => new PasswordLoginEventResp(state, null, resp.Captcha.Url),
            _ when info is not null => new PasswordLoginEventResp(state, (info.TipsTitle, info.TipsContent), info.JumpUrl),
            _ => new PasswordLoginEventResp(state, null, null)
        });
    }
}