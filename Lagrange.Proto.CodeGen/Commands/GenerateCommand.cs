using Lagrange.Proto.CodeGen.Format;

namespace Lagrange.Proto.CodeGen.Commands;

public static class GenerateCommand
{
    public static async Task Invoke(string[]? path, string? folder, bool recursive)
    {
        if (path == null && folder == null)
        {
            Console.WriteLine("No path or folder specified.");
            return;
        }
        
        if (path != null)
        {
            foreach (string file in path)
            {
                if (File.Exists(file))
                {
                    await GenerateProtoFile(file);
                }
                else
                {
                    Console.WriteLine($"File not found: {file}");
                }
            }
        }
        
        if (folder != null)
        {
            if (Directory.Exists(folder))
            {
                var files = Directory.GetFiles(folder, "*.proto", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    await GenerateProtoFile(file);
                }
            }
            else
            {
                Console.WriteLine($"Folder not found: {folder}");
            }
        }
    }
    
    private static async Task GenerateProtoFile(string file)
    {
        Console.WriteLine($"Generating proto file for {file}...");
        
        string proto = await File.ReadAllTextAsync(file);
        var lexer = new ProtoLexer(proto);
        var tokens = lexer.Tokenize();
        var parser = new ProtoParser(tokens);
        var protoFile = parser.ParseProto();
        
        string outputFile = Path.ChangeExtension(file, ".cs");
    }
}