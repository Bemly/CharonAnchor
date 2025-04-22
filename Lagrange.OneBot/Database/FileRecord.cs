namespace Lagrange.OneBot.Database;

[Serializable]
public class FileRecord
{
    public const string CreateStatement =  """
                                             CREATE TABLE IF NOT EXISTS `FileRecord` (
                                                        `SelfUin` INTEGER NOT NULL,
                                                        `File` TEXT NOT NULL,
                                                        `Data` BLOB NOT NULL,
                                                        PRIMARY KEY (`SelfUin`, `File`)
                                                 );
                                             """;
    public long SelfUin { get; set; }
    
    public string File { get; set; } = string.Empty;
    
    public byte[] Data { get; set; } = [];
}