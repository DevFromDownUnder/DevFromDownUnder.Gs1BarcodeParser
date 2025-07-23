using DevFromDownUnder.Gs1BarcodeParser.Barcode;
using System.Collections.Generic;

namespace DevFromDownUnder.Gs1BarcodeParser
{
    public class ParserResult
    {
        public bool IsValid { get; internal set; } = true;
        public List<BarcodeIdentifier> Values { get; internal set; } = new List<BarcodeIdentifier>();
    }
}