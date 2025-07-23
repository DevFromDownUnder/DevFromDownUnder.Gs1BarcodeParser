using DevFromDownUnder.Gs1BarcodeParser.Barcode.Definition.AI;
using System.Collections.Generic;

namespace DevFromDownUnder.Gs1BarcodeParser.Barcode
{
    public class BarcodeIdentifier
    {
        public BarcodeIdentifier(AIDefinition definition, string value)
        {
            Definition = definition;
            Value = value;
        }

        public AIDefinition Definition { get; set; }
        public List<string> ValidationErrors { get; } = new List<string>();
        public string Value { get; set; }

        public void AddValidationError(string error)
        {
            if (!ValidationErrors.Contains(error))
            {
                ValidationErrors.Add(error);
            }
        }

        public override string ToString()
        {
            return string.Format("({0}) {1}  - {2}", Definition.RawAI, Value, Definition.Title);
        }
    }
}