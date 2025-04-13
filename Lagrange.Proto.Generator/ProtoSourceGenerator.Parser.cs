using Lagrange.Proto.Generator.Entity;
using Lagrange.Proto.Generator.Utility;
using Lagrange.Proto.Generator.Utility.Extension;
using Lagrange.Proto.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Lagrange.Proto.Generator.DiagnosticDescriptors;

namespace Lagrange.Proto.Generator;

public partial class ProtoSourceGenerator
{
    private const string ProtoPackableAttributeFullName = "Lagrange.Proto.ProtoPackableAttribute";
    
    private class Parser(ClassDeclarationSyntax context, SemanticModel model)
    {
        public SemanticModel Model { get; } = model;
        
        public TypesResolver Resolver { get; } = new(model);
        
        public List<Diagnostic> Diagnostics { get; } = [];
        
        public string? Namespace { get; private set; }
        
        public string Identifier { get; private set; } = string.Empty;
        
        public bool IgnoreDefaultFields { get; private set; }
        
        public Dictionary<int, ProtoFieldInfo> Fields { get; } = new();
        
        public void Parse(CancellationToken token = default)
        {
            Namespace = context.GetNamespace()?.ToString();
            Identifier = context.Identifier.Text;

            if (Model.GetDeclaredSymbol(context) is not INamedTypeSymbol { } classSymbol)
            {
                ReportDiagnostics(UnableToGetSymbol, context.GetLocation(), context.Identifier.Text);
                return;
            }
            
            foreach (var argument in classSymbol.GetAttributes().SelectMany(x => x.NamedArguments))
            {
                switch (argument.Key)
                {
                    case "IgnoreDefaultFields":
                    { 
                        IgnoreDefaultFields = (bool)(argument.Value.Value ?? false);
                        break;
                    }
                }
            }
            
            if (!context.IsPartial())
            {
                ReportDiagnostics(MustBePartialClass, context.GetLocation(), context.Identifier.Text);
                return;
            }

            var members = context.ChildNodes()
                .Where(x => x is FieldDeclarationSyntax or PropertyDeclarationSyntax)
                .Cast<MemberDeclarationSyntax>()
                .Where(x => x.ContainsAttribute("ProtoMember"));
            
            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();
                
                var symbol = Model.GetDeclaredSymbol(member) ?? throw new InvalidOperationException("Unable to get symbol.");
                var attribute = symbol.GetAttributes().First();
                int field = (int)(attribute.ConstructorArguments[0].Value ?? throw new InvalidOperationException("Unable to get field number."));
                var type = member switch
                {
                    FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Type,
                    PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Type,
                    _ => throw new InvalidOperationException("Unsupported member type.")
                };
                string name = member switch
                {
                    FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Variables[0].Identifier.ToString(),
                    PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Identifier.ToString(),
                    _ => throw new InvalidOperationException("Unsupported member type.")
                };
                var typeSymbol = symbol switch
                {
                    IPropertySymbol propertySymbol => propertySymbol.Type,
                    IFieldSymbol fieldSymbol => fieldSymbol.Type,
                    _ => throw new InvalidOperationException("Unsupported member type.")
                };
                var wireType = type.GetWireType();
                bool signed = false;

                if (wireType == WireType.LengthDelimited && typeSymbol.IsUserDefinedType())
                {
                    var typeAttribute = typeSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == ProtoPackableAttributeFullName);
                    if (typeAttribute == null)
                    { 
                        ReportDiagnostics(NestedTypeMustBeProtoPackable, member.GetLocation(), typeSymbol.Name, Identifier);
                        continue;
                    }
                }
                
                foreach (var argument in attribute.NamedArguments)
                {
                    switch (argument.Key)
                    {
                        case "NumberHandling":
                        {
                            if (wireType != WireType.VarInt)
                            {
                                ReportDiagnostics(InvalidNumberHandling, member.GetLocation(), field, Identifier);
                                continue;
                            }
                            
                            var value = (ProtoNumberHandling)(argument.Value.Value ?? throw new InvalidOperationException("Unable to get number handling."));
                            if (value.HasFlag(ProtoNumberHandling.Signed)) signed = true;
                            if (value.HasFlag(ProtoNumberHandling.Fixed32)) wireType = WireType.Fixed32;
                            if (value.HasFlag(ProtoNumberHandling.Fixed64)) wireType = WireType.Fixed64;
                            break;
                        }
                    }
                }
                
                if (Fields.ContainsKey(field))
                {
                    ReportDiagnostics(DuplicateFieldNumber, member.GetLocation(), field, Identifier);
                    continue;
                }
                
                Fields[field] = new ProtoFieldInfo(member, name, type, wireType, signed);
            }
        }
        
        private void ReportDiagnostics(DiagnosticDescriptor descriptor, Location location, params object[] args)
        {
            var diagnostic = Diagnostic.Create(descriptor, location, args);
            Diagnostics.Add(diagnostic);
        }
    }
}