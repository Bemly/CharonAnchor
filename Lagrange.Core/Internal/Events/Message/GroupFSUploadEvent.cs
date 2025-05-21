namespace Lagrange.Core.Internal.Events.Message;

internal class GroupFSUploadEventReq(long groupUin, string fileName, Stream stream, string parentDirectory, byte[] fileMd5) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;
    
    public string FileName { get; } = fileName;
    
    public Stream Stream { get; } = stream;

    public string ParentDirectory { get; } = parentDirectory;

    public byte[] FileMd5 { get; } = fileMd5;
}

internal class GroupFSUploadEventResp(bool fileExist, string fileId, byte[] fileKey, byte[] checkKey, (string ip, uint uploadPort) addr) : ProtocolEvent
{
    public bool FileExist { get; } = fileExist;
    
    public string FileId { get; } = fileId;
    
    public byte[] FileKey { get; } = fileKey;
    
    public byte[] CheckKey { get; } = checkKey;
    
    public (string ip, uint uploadPort) Addr { get; } = addr;
}