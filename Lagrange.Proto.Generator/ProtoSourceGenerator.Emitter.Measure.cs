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
        private const string ConstantVarName = "constant";
        private const string LengthVarName = "length";

        private const string ProtoHelperTypeRef = "global::Lagrange.Proto.Utility.ProtoHelper";
        private const string ProtoResolvableUtilityTypeRef = "global::Lagrange.Proto.Primitives.ProtoResolvableUtility";

        private const string GetVarIntLengthMethodRef = $"{ProtoHelperTypeRef}.GetVarIntLength";
        private const string CountStringMethodRef = $"{ProtoHelperTypeRef}.CountString";
        private const string CountBytesMethodRef = $"{ProtoHelperTypeRef}.CountBytes";
        private const string MeasureMethodRef = $"{ProtoResolvableUtilityTypeRef}.Measure";

        private void EmitMeasureMethod(SourceWriter source)
        {
            source.WriteLine($"public static int MeasureHandler({_fullQualifiedName} {ObjectVarName})");
            source.WriteLine("{");
            source.Indentation++;

            EmitConstant(source);
            source.WriteLine($"int {LengthVarName} = {ConstantVarName};");
            source.WriteLine();

            foreach (var kv in parser.Fields)
            {
                int field = kv.Key;
                var info = kv.Value;

                EmitLengthStatement(source, field, info);
            }

            source.WriteLine();
            source.WriteLine($"return {LengthVarName};");

            source.Indentation--;
            source.WriteLine("}");
        }

        private void EmitConstant(SourceWriter source)
        {
            if (parser.IgnoreDefaultFields)
            {
                source.WriteLine($"const int {ConstantVarName} = 0;");
                return;
            }

            int constant = 0;
            foreach (var kv in parser.Fields)
            {
                int field = kv.Key;
                var info = kv.Value;

                var tag = ProtoHelper.EncodeVarInt(field << 3 | (byte)info.WireType);
                if (info.TypeSymbol.IsValueType && !info.TypeSymbol.IsNullable()) constant += tag.Length;
            }

            source.WriteLine($"const int {ConstantVarName} = {constant};");
        }

        private void EmitLengthStatement(SourceWriter source, int field, ProtoFieldInfo info)
        {
            var tag = ProtoHelper.EncodeVarInt(field << 3 | (byte)info.WireType);
            string lengthMember = info.TypeSymbol.IsValueType && info.TypeSymbol.IsNullable()
                ? GenerateLengthMember(field, info, $"{ObjectVarName}.{info.Symbol.Name}.Value")
                : GenerateLengthMember(field, info, $"{ObjectVarName}.{info.Symbol.Name}");
            string expression;

            if (parser.IgnoreDefaultFields)
            {
                expression = info.TypeSymbol.IsValueType
                    ? GenerateIfNotDefaultExpression($"{ObjectVarName}.{info.Symbol.Name}", $"{tag.Length} + {lengthMember}", "0")
                    : GenerateIfNotNullExpression($"{ObjectVarName}.{info.Symbol.Name}", $"{tag.Length} + {lengthMember}", "0"); // check with default
            }
            else
            {
                if (info.TypeSymbol.IsValueType)
                {
                    expression = info.TypeSymbol.IsNullable()
                        ? GenerateIfNotNullExpression($"{ObjectVarName}.{info.Symbol.Name}", $"{tag.Length} + {lengthMember}", "0")
                        : lengthMember;
                }
                else
                {
                    expression = GenerateIfNotNullExpression($"{ObjectVarName}.{info.Symbol.Name}", $"{tag.Length} + {lengthMember}", "0");
                }
            }

            source.WriteLine($"{LengthVarName} += {expression};");
        }

        private static string GenerateLengthMember(int field, ProtoFieldInfo info, string memberName)
        {
            return info.WireType switch
            {
                WireType.VarInt when info.TypeSymbol.IsIntegerType() => $"{GetVarIntLengthMethodRef}({memberName})",
                WireType.Fixed32 => "4",
                WireType.Fixed64 => "8",
                WireType.LengthDelimited when info.TypeSymbol.SpecialType == SpecialType.System_String => $"{CountStringMethodRef}({memberName})",
                WireType.LengthDelimited when info.TypeSymbol is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Byte } => $"{CountBytesMethodRef}({memberName})",
                WireType.LengthDelimited when info.ExtraTypeInfo.Count is not 0 => $"{TypeInfoPropertyName}.Fields[{field << 3 | (byte)info.WireType}].Measure({memberName})",
                _ => $"{MeasureMethodRef}({field}, {WireTypeTypeRef}.{info.WireType}, {memberName})"
            };
        }
    }

    private static string GenerateIfNotNullExpression(string variableName, string left, string right) => $"({variableName} != null ? {left} : {right})";
        
    private static string GenerateIfNotDefaultExpression(string variableName, string left, string right) => $"({variableName} != default ? {left} : {right})";
}