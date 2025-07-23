namespace Gs1BarcodeParser.Barcode.Definition.Enums
{
    public enum ComponentDataType
    {
        /// <summary>
        /// Numeric digits (0-9).
        /// </summary>
        Numeric,

        /// <summary>
        /// Alphanumeric characters from GS1 character set 82.
        /// </summary>
        AlphanumericX,

        /// <summary>
        /// Alphanumeric characters that can be Base64 encoded.
        /// </summary>
        AlphanumericY,

        /// <summary>
        /// Data that can be digitally signed.
        /// </summary>
        AlphanumericZ
    }
}