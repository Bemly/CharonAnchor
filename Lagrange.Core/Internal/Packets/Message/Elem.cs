using Lagrange.Proto;

#pragma warning disable CS8618 // Non-nullable field is uninitialized.
namespace Lagrange.Core.Internal.Packets.Message;

[ProtoPackable]
internal partial class Elem
{
    [ProtoMember(1)] public Text? Text { get; set; } 
    
    [ProtoMember(4)] public NotOnlineImage? NotOnlineImage { get; set; }
    
    [ProtoMember(53)] public CommonElem? CommonElem { get; set; }
}

[ProtoPackable]
internal partial class Text
{
    [ProtoMember(1)] public string TextMsg { get; set; }
    
    [ProtoMember(2)] public string Link { get; set; }
    
    [ProtoMember(3)] public byte[] Attr6Buf { get; set; }
    
    [ProtoMember(4)] public byte[] Attr7Buf { get; set; }
    
    [ProtoMember(11)] public byte[] Buf { get; set; }

    [ProtoMember(12)] public byte[] PbReserve { get; set; }
}

[ProtoPackable]
internal partial class NotOnlineImage
{
    [ProtoMember(1)] public byte[] FilePath { get; set; }

    [ProtoMember(2)] public uint FileLen { get; set; }

    [ProtoMember(3)] public byte[] DownloadPath { get; set; }

    [ProtoMember(4)] public byte[] OldVerSendFile { get; set; }

    [ProtoMember(5)] public uint ImgType { get; set; }

    [ProtoMember(6)] public byte[] PreviewsImage { get; set; }

    [ProtoMember(7)] public byte[] PicMd5 { get; set; }

    [ProtoMember(8)] public uint PicHeight { get; set; }

    [ProtoMember(9)] public uint PicWidth { get; set; }

    [ProtoMember(10)] public byte[] ResId { get; set; }

    [ProtoMember(11)] public byte[] Flag { get; set; }

    [ProtoMember(12)] public string ThumbUrl { get; set; }

    [ProtoMember(13)] public uint Original { get; set; }

    [ProtoMember(14)] public string BigUrl { get; set; }

    [ProtoMember(15)] public string OrigUrl { get; set; }

    [ProtoMember(16)] public uint BizType { get; set; }

    [ProtoMember(17)] public uint Result { get; set; }

    [ProtoMember(18)] public uint Index { get; set; }

    [ProtoMember(19)] public byte[] OpFaceBuf { get; set; }

    [ProtoMember(20)] public bool OldPicMd5 { get; set; }

    [ProtoMember(21)] public uint ThumbWidth { get; set; }

    [ProtoMember(22)] public uint ThumbHeight { get; set; }

    [ProtoMember(23)] public uint FileId { get; set; }

    [ProtoMember(24)] public uint ShowLen { get; set; }

    [ProtoMember(25)] public uint DownloadLen { get; set; }

    [ProtoMember(26)] public string Url400 { get; set; }

    [ProtoMember(27)] public uint Width400 { get; set; }

    [ProtoMember(28)] public uint Height400 { get; set; }

    [ProtoMember(29)] public byte[] PbReserve { get; set; }
}

[ProtoPackable]
internal partial class CommonElem
{
    [ProtoMember(1)] public uint ServiceType { get; set; }

    [ProtoMember(2)] public byte[] PbElem { get; set; }

    [ProtoMember(3)] public uint BusinessType { get; set; }
}