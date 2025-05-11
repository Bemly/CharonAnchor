using System.Diagnostics;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Message.Entities;

public abstract class RichMediaEntityBase : IMessageEntity
{
    private MsgInfo? _msgInfo;

    protected byte[]? _compat;

    private protected bool IsGroup;
    
    internal MsgInfo? MsgInfo
    {
        get => _msgInfo;
        private protected set
        {
            Debug.Assert(value != null);
            
            var fileInfo = value.MsgInfoBody[0].Index.Info;
            FileMd5 = fileInfo.FileHash;
            FileSha1 = fileInfo.FileSha1;
            FileSize = fileInfo.FileSize;
            FileName = fileInfo.FileName;
            _msgInfo = value;
        } 
    }
    
    internal abstract Lazy<Stream>? Stream { get; }
    
    public string FileUrl { get; internal set; } = string.Empty;

    public string FileName { get; internal set; } = string.Empty;
    
    public string FileSha1 { get; internal set; } = string.Empty;
    
    public uint FileSize { get; internal set; }

    public string FileMd5 { get; internal set; } = string.Empty;
    
    public abstract Task Preprocess(BotContext context, BotMessage message);

    public abstract Task Postprocess(BotContext context, BotMessage message);
    
    internal abstract Elem[] Build();
    
    internal abstract IMessageEntity? Parse(List<Elem> elements, Elem target);
    
    Elem[] IMessageEntity.Build() => Build();

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target) => Parse(elements, target);
}