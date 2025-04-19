using Lagrange.Proto.Generator.Entity;
using Lagrange.Proto.Generator.Utility;
using Lagrange.Proto.Generator.Utility.Extension;
using Lagrange.Proto.Serialization;
using Microsoft.CodeAnalysis;

namespace Lagrange.Proto.Generator;

public partial class ProtoSourceGenerator
{
    private partial class Emitter
    {
        private const string ProtoWriterTypeRef = "global::Lagrange.Proto.Primitives.ProtoWriter";

        private const string WriterVarName = "writer";
        private const string ObjectVarName = "obj";
        private const string ValueVarName = "value";
        
        private const string WriteRawByteMethodName = "WriteRawByte";
        private const string EncodeVarIntMethodName = "EncodeVarInt";
        private const string EncodeFixed32MethodName = "EncodeFixed32";
        private const string EncodeFixed64MethodName = "EncodeFixed64";
        private const string ZigZagEncodeMethodName = "ZigZagEncode";

        private const string EncodeStringMethodName = "EncodeString";
        private const string EncodeBytesMethodName = "EncodeBytes";
        private const string EncodeResolvableMethodName = "EncodeResolvable";

        private void EmitSerializeMethod(SourceWriter source)
        {
            source.WriteLine($"public static void SerializeHandler({_fullQualifiedName} {ObjectVarName}, {ProtoWriterTypeRef} {WriterVarName})");
            source.WriteLine("{");
            source.Indentation++;

            foreach (var kv in parser.Fields)
            {
                int field = kv.Key;
                var info = kv.Value;
                
                EmitMembers(source, field, info);
                source.WriteLine();
            }
            
            source.Indentation--;
            source.WriteLine("}");
        }

        private void EmitMembers(SourceWriter source, int field, ProtoFieldInfo info)
        {
            var tag = ProtoHelper.EncodeVarInt(field << 3 | (byte)info.WireType);
            
            string memberName =  info.TypeSymbol.IsValueType && info.TypeSymbol.IsNullable() 
                ? $"{ObjectVarName}.{info.Symbol.Name}.Value"
                : $"{ObjectVarName}.{info.Symbol.Name}";
            if (parser.IgnoreDefaultFields)
            {
                if (info.TypeSymbol.IsValueType) // check with default
                {
                    EmitIfNotDefaultStatement(source, $"{ObjectVarName}.{info.Symbol.Name}", writer =>
                    {
                        EmitRawTags(writer, tag);
                        EmitMember(writer, field, info, memberName);
                    });
                }
                else
                {
                    EmitIfNotNullStatement(source, memberName, writer =>
                    {
                        EmitRawTags(writer, tag);
                        EmitMember(writer, field, info, memberName);
                    });
                }
            }
            else
            {
                if (info.TypeSymbol.IsValueType)
                {
                    if (info.TypeSymbol.IsNullable()) // write xxxx.Value
                    {
                        EmitIfNotNullStatement(source, $"{ObjectVarName}.{info.Symbol.Name}", writer =>
                        {
                            EmitRawTags(writer, tag);
                            EmitMember(writer, field, info, memberName);
                        });
                    }
                    else // write directly
                    {
                        EmitRawTags(source, tag);
                        EmitMember(source, field, info, memberName);
                    }
                }
                else
                {
                    EmitIfNotNullStatement(source, memberName, writer =>
                    {
                        EmitRawTags(writer, tag);
                        EmitMember(writer, field, info, memberName);
                    });
                }
            }
        }
        
        private static void EmitRawTags(SourceWriter source, byte[] tag)
        {
            foreach (byte i in tag) source.WriteLine($"{WriterVarName}.{WriteRawByteMethodName}({i});");
        }

        private void EmitMember(SourceWriter source, int field, ProtoFieldInfo info, string memberName)
        {
            if (!SymbolResolver.IsRepeatedType(info.TypeSymbol, out _))
            {
                string? special = info.WireType switch
                {
                    WireType.VarInt when info.TypeSymbol.IsIntegerType() && info.IsSigned => $"{WriterVarName}.{EncodeVarIntMethodName}({ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));",
                    WireType.VarInt when info.TypeSymbol.IsIntegerType() => $"{WriterVarName}.{EncodeVarIntMethodName}({memberName});",
                    WireType.Fixed32 when info.IsSigned => $"{WriterVarName}.{EncodeFixed32MethodName}({ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));",
                    WireType.Fixed64 when info.IsSigned => $"{WriterVarName}.{EncodeFixed64MethodName}({ProtoHelperTypeRef}.{ZigZagEncodeMethodName}({memberName}));",
                    WireType.Fixed32 => $"{WriterVarName}.{EncodeFixed32MethodName}({memberName});",
                    WireType.Fixed64 => $"{WriterVarName}.{EncodeFixed64MethodName}({memberName});",
                    WireType.LengthDelimited when info.TypeSymbol.SpecialType == SpecialType.System_String => $"{WriterVarName}.{EncodeStringMethodName}({memberName});",
                    WireType.LengthDelimited when info.TypeSymbol is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Byte } => $"{WriterVarName}.{EncodeBytesMethodName}({memberName});",
                    _ => null
                };

                if (special != null)
                {
                    source.WriteLine(special);
                    return;
                }
            }
            
            source.WriteLine(info.IsSigned
                ? $"{WriterVarName}.{EncodeResolvableMethodName}({field}, {WireTypeTypeRef}.{info.WireType}, {memberName}, {ProtoNumberHandlingTypeRef}.{(info.IsSigned ? "Signed" : "Default")});"
                : $"{WriterVarName}.{EncodeResolvableMethodName}({field}, {WireTypeTypeRef}.{info.WireType}, {memberName});");
        }
    }
}