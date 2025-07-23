using Gs1BarcodeParser.Barcode.Definition.Enums;
using Gs1BarcodeParser.Barcode.Definition.Linter.Enums;
using System.Collections.Generic;

namespace Gs1BarcodeParser.Barcode.Definition
{
    public class DefinitionComponent
    {
        public DefinitionComponent(ComponentDataType dataType, int minLength, int maxLength, bool isOptional, List<LinterTypes> linters)
        {
            DataType = dataType;
            MinLength = minLength;
            MaxLength = maxLength;
            IsOptional = isOptional;
            Linters = linters;
            IsVariableLength = MinLength != MaxLength && MaxLength != 0;
        }

        public ComponentDataType DataType { get; }
        public bool IsOptional { get; }
        public bool IsVariableLength { get; }
        public List<LinterTypes> Linters { get; }
        public int MaxLength { get; }
        public int MinLength { get; }
    }
}