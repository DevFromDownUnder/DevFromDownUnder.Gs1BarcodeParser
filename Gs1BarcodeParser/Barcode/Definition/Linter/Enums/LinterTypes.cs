namespace Gs1BarcodeParser.Barcode.Definition.Linter.Enums
{
    /// <summary>
    /// Defines the types of linters (validators) used for checking GS1 data components.
    /// See more at https://gs1.github.io/gs1-syntax-dictionary/
    /// </summary>
    public enum LinterTypes
    {
        CouponCode,
        CouponPosOffer,
        Csum,
        CsumAlpha,
        HasNonDigit,
        Hyphen,
        Iban,
        ImporterIdx,
        Iso3166,
        Iso3166999,
        Iso3166Alpha2,
        Iso4217,
        Iso5218,
        Key,
        Keyoff1,
        Latitude,
        Longitude,
        MediaType,
        NoZeroPrefix,
        NonZero,
        PackageType,
        PcEnc,
        PieceOfTotal,
        PosInSeqSlash,
        Winding,
        YYMMD0,
        YesNo
    }
}