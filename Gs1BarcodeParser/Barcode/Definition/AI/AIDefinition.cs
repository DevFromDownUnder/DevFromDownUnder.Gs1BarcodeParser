using Gs1BarcodeParser.Barcode.Definition.AI.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Gs1BarcodeParser.Barcode.Definition.AI
{
    public class AIDefinition
    {
        public static readonly AIDefinition Unknown = new AIDefinition(
            rawAi: "",
            aiType: AITypes.Unknown,
            flags: null,
            components: new List<DefinitionComponent>(),
            attributes: new List<KeyValuePair<string, string>>()
        );

        public AIDefinition(string rawAi, AITypes aiType, string flags, List<DefinitionComponent> components, List<KeyValuePair<string, string>> attributes)
        {
            RawAI = rawAi;
            AI = aiType;
            Flags = flags ?? string.Empty;
            Attributes = attributes ?? new List<KeyValuePair<string, string>>();
            Components = components;

            IsVariableLength = Components.Any(c => c.IsVariableLength);
            FixedDataLength = IsVariableLength ? 0 : Components.Sum(c => c.MaxLength);
            DoesNotRequireFnc1 = Flags.Contains('*');
            ExclusiveAIs = Attributes.Where(kv => kv.Key == "ex").Select(kv => kv.Value);
            IsDigitalLinkAttribute = Flags.Contains('?');
            IsDigitalLinkPrimaryKey = Attributes.Any(kv => kv.Key == "dlpkey");
            RequiredAIs = Attributes.Where(kv => kv.Key == "req").Select(kv => kv.Value);
            Title = AIMetadata.GetTitle(AI);
        }

        public AITypes AI { get; }
        public List<KeyValuePair<string, string>> Attributes { get; }
        public List<DefinitionComponent> Components { get; }
        public bool DoesNotRequireFnc1 { get; }
        public IEnumerable<string> ExclusiveAIs { get; }
        public int FixedDataLength { get; }
        public string Flags { get; }
        public bool IsDigitalLinkAttribute { get; }
        public bool IsDigitalLinkPrimaryKey { get; }
        public bool IsVariableLength { get; }
        public string RawAI { get; }
        public IEnumerable<string> RequiredAIs { get; }
        public string Title { get; }
    }
}