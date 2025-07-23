using DevFromDownUnder.Gs1BarcodeParser.Barcode;
using DevFromDownUnder.Gs1BarcodeParser.Barcode.Definition;
using DevFromDownUnder.Gs1BarcodeParser.Barcode.Definition.AI;
using DevFromDownUnder.Gs1BarcodeParser.Barcode.Definition.Enums;
using DevFromDownUnder.Gs1BarcodeParser.Barcode.Definition.Linter;
using DevFromDownUnder.Gs1BarcodeParser.Barcode.Definition.Linter.Enums;
using DevFromDownUnder.Gs1BarcodeParser.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevFromDownUnder.Gs1BarcodeParser
{
    public class BarcodeParser
    {
        public const char FNC1 = (char)29;

        public static ParserResult Parse(string data)
        {
            ReadOnlySpan<char> span = data.AsSpan().TrimStart(FNC1);

            bool isDigitalLink = span.StartsWith("https://".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                                 span.StartsWith("http://".AsSpan(), StringComparison.OrdinalIgnoreCase);

            if (isDigitalLink)
            {
                span = DigitalLinkConverter.ToBarcodeString(data).AsSpan();
            }

            return Validate(ParseBarcodeString(span), isDigitalLink);
        }

        private static AIDefinition FindDefinition(ReadOnlySpan<char> data)
        {
            foreach (var definition in DefinitionDictionary.All)
            {
                if (data.StartsWith(definition.RawAI.AsSpan()))
                {
                    return definition;
                }
            }

            return null;
        }

        private static int GetDataLength(AIDefinition definition, ReadOnlySpan<char> data)
        {
            if (definition.IsVariableLength)
            {
                int fnc1Position = data.IndexOf(FNC1);
                return fnc1Position == -1 ? data.Length : fnc1Position;
            }

            return definition.FixedDataLength;
        }

        private static bool IsValidBase64String(ReadOnlySpan<char> value)
        {
            if (value.IsEmpty) return true;
            try
            {
                Convert.FromBase64String(value.ToString());
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static List<BarcodeIdentifier> ParseBarcodeString(ReadOnlySpan<char> barcodeData)
        {
            var values = new List<BarcodeIdentifier>();
            var remainingData = barcodeData;

            while (!remainingData.IsEmpty)
            {
                var definition = FindDefinition(remainingData);
                if (definition == null)
                {
                    int fnc1Index = remainingData.IndexOf(FNC1);
                    var data = fnc1Index != -1 ? remainingData.Slice(0, fnc1Index) : remainingData;

                    var unknown = new BarcodeIdentifier(AIDefinition.Unknown, data.ToString());
                    unknown.AddValidationError("Unrecognized Application Identifier or data");
                    values.Add(unknown);

                    remainingData = remainingData.Slice(data.Length);

                    if (!remainingData.IsEmpty && remainingData[0] == FNC1)
                    {
                        remainingData = remainingData.Slice(1);
                    }

                    continue;
                }

                remainingData = remainingData.Slice(definition.RawAI.Length);

                int dataLength = GetDataLength(definition, remainingData);
                if (dataLength > remainingData.Length)
                {
                    dataLength = remainingData.Length;
                }

                var valueSpan = remainingData.Slice(0, dataLength);
                values.Add(new BarcodeIdentifier(definition, valueSpan.ToString()));

                remainingData = remainingData.Slice(dataLength);

                if (!remainingData.IsEmpty && remainingData[0] == FNC1)
                {
                    remainingData = remainingData.Slice(1);
                }
            }

            return values;
        }

        private static ParserResult Validate(List<BarcodeIdentifier> parsedItems, bool isDigitalLink)
        {
            var result = new ParserResult();
            var presentAis = new HashSet<string>(parsedItems.Select(p => p.Definition.RawAI));

            foreach (var item in parsedItems)
            {
                ValidateSpecification(item);
            }

            ValidateCompanionRules(parsedItems, presentAis);

            if (isDigitalLink)
            {
                ValidateDigitalLinkPrimaryKey(parsedItems);
            }

            if (parsedItems.Any(p => p.ValidationErrors.Count != 0))
            {
                result.IsValid = false;
            }

            result.Values = parsedItems;

            return result;
        }

        private static void ValidateCompanionRules(List<BarcodeIdentifier> parsedItems, HashSet<string> presentAis)
        {
            foreach (var item in parsedItems)
            {
                foreach (var exclusionSet in item.Definition.ExclusiveAIs)
                {
                    var excluded = exclusionSet.Split(',');
                    foreach (var exAi in excluded.Where(presentAis.Contains))
                    {
                        item.AddValidationError(string.Format("AI ({0}) cannot be used with AI ({1}).", item.Definition.RawAI, exAi));
                    }
                }

                foreach (var requirementSet in item.Definition.RequiredAIs)
                {
                    var orGroups = requirementSet.Split(',');
                    bool requirementMet = orGroups.Any(group => group.Split('+').All(presentAis.Contains));
                    if (!requirementMet)
                    {
                        item.AddValidationError(string.Format("AI ({0}) is missing a required companion. Requires one of: [{1}].", item.Definition.RawAI, requirementSet));
                    }
                }
            }
        }

        private static void ValidateDigitalLinkPrimaryKey(List<BarcodeIdentifier> parsedItems)
        {
            if (parsedItems.Count == 0) return;

            var primaryAi = parsedItems.First();
            if (primaryAi.Definition.IsDigitalLinkPrimaryKey)
            {
                var qualifierAis = parsedItems.Skip(1).Select(p => p.Definition.RawAI).ToList();
                var dlpkeyAttr = primaryAi.Definition.Attributes.FirstOrDefault(kvp => kvp.Key == "dlpkey");

                if (dlpkeyAttr.Key != null && string.IsNullOrEmpty(dlpkeyAttr.Value))
                {
                    if (qualifierAis.Count != 0)
                    {
                        primaryAi.AddValidationError(string.Format("Digital Link Primary Key AI ({0}) does not permit any qualifiers, but found ({1}).", primaryAi.Definition.RawAI, string.Join(",", qualifierAis.ToArray())));
                    }
                }
                else if (dlpkeyAttr.Key != null)
                {
                    var allowedSequencesRaw = dlpkeyAttr.Value;
                    var allowedSequences = allowedSequencesRaw.Split('|').Select(seq => seq.Split(',').ToList()).ToList();

                    bool isSequenceValid = false;
                    foreach (var allowedSeq in allowedSequences)
                    {
                        int lastFoundIndex = -1;
                        bool currentSequenceMatch = true;
                        foreach (var actualAi in qualifierAis)
                        {
                            int currentIndex = allowedSeq.IndexOf(actualAi);
                            if (currentIndex > lastFoundIndex)
                            {
                                lastFoundIndex = currentIndex;
                            }
                            else
                            {
                                currentSequenceMatch = false;
                                break;
                            }
                        }
                        if (currentSequenceMatch)
                        {
                            isSequenceValid = true;
                            break;
                        }
                    }

                    if (!isSequenceValid)
                    {
                        primaryAi.AddValidationError(string.Format("Invalid qualifier order for Digital Link Primary Key AI ({0}). Found ({1}), which does not follow an allowed sequence: [{2}].", primaryAi.Definition.RawAI, string.Join(",", qualifierAis.ToArray()), allowedSequencesRaw.Replace('|', ' ')));
                    }
                }
            }
        }

        private static void ValidateSpecification(BarcodeIdentifier item)
        {
            var value = item.Value.AsSpan();
            int currentPosition = 0;

            foreach (var component in item.Definition.Components)
            {
                string componentDesc = string.Format("{0}({1}..{2})", component.DataType, component.MinLength, component.MaxLength);

                if (currentPosition >= value.Length)
                {
                    if (!component.IsOptional)
                    {
                        item.AddValidationError(string.Format("Missing mandatory data for spec component '{0}'.", componentDesc));
                    }
                    break;
                }

                int segmentLength = component.IsVariableLength ? value.Length - currentPosition : component.MaxLength;
                if (currentPosition + segmentLength > value.Length)
                {
                    item.AddValidationError(string.Format("Insufficient data for spec component '{0}'.", componentDesc));
                    break;
                }

                var segment = value.Slice(currentPosition, segmentLength);

                if (component.DataType == ComponentDataType.Numeric && segment.ToString().Any(c => !char.IsDigit(c)))
                {
                    item.AddValidationError(string.Format("Segment '{0}' for spec '{1}' must be numeric.", segment.ToString(), componentDesc));
                }
                else if (component.DataType == ComponentDataType.AlphanumericX && segment.ToString().Any(c => Linter.CSET82.IndexOf(c) == -1))
                {
                    item.AddValidationError(string.Format("Segment '{0}' for spec '{1}' must be Cset82 alphanumeric.", segment.ToString(), componentDesc));
                }
                else if (component.DataType == ComponentDataType.AlphanumericY && !IsValidBase64String(segment))
                {
                    item.AddValidationError(string.Format("Segment '{0}' for spec '{1}' must be valid base 64.", segment.ToString(), componentDesc));
                }

                foreach (var linter in component.Linters)
                {
                    var valueForLinter = (linter == LinterTypes.Csum || linter == LinterTypes.Key || linter == LinterTypes.Keyoff1 || linter == LinterTypes.CsumAlpha || linter == LinterTypes.Iso5218) ? value : segment;

                    switch (linter)
                    {
                        case LinterTypes.Csum:
                            if (!Linter.ValidateChecksum(valueForLinter))
                                item.AddValidationError(string.Format("Invalid checksum for value '{0}'.", item.Value));
                            break;

                        case LinterTypes.Key:
                            if (!Linter.ValidateHasGs1Prefix(valueForLinter))
                                item.AddValidationError(string.Format("Value '{0}' fails prefix check (must be >= 4 chars and start with 4 digits).", item.Value));
                            break;

                        case LinterTypes.Keyoff1:
                            if (!Linter.ValidateHasGs1PrefixOffset1(valueForLinter))
                                item.AddValidationError(string.Format("Value '{0}' fails offset prefix check (must be >= 5 chars and have 4 digits after offset).", item.Value));
                            break;

                        case LinterTypes.CsumAlpha:
                            if (!Linter.ValidateCsumAlpha(valueForLinter))
                                item.AddValidationError(string.Format("Invalid alphanumeric checksum for value '{0}'.", item.Value));
                            break;

                        case LinterTypes.YYMMD0:
                            if (!Linter.ValidateYYMMD0(valueForLinter))
                                item.AddValidationError(string.Format("Malformed date '{0}'. Expected YYMMDD format.", valueForLinter.ToString()));
                            break;

                        case LinterTypes.Iso4217:
                            if (!Linter.ValidateIso4217(valueForLinter))
                                item.AddValidationError(string.Format("Invalid ISO 4217 currency code '{0}'.", valueForLinter.ToString()));
                            break;

                        case LinterTypes.Iso3166:
                            if (!Linter.ValidateIso3166(valueForLinter))
                                item.AddValidationError(string.Format("Invalid ISO 3166 country code '{0}'.", valueForLinter.ToString()));
                            break;

                        case LinterTypes.Iso3166Alpha2:
                            if (!Linter.ValidateIso3166Alpha2(valueForLinter))
                                item.AddValidationError(string.Format("Invalid ISO 3166 alpha-2 country code '{0}'.", valueForLinter.ToString()));
                            break;

                        case LinterTypes.Iso3166999:
                            if (!Linter.ValidateIso3166(valueForLinter, allow999: true))
                                item.AddValidationError(string.Format("Invalid ISO 3166 country code '{0}'.", valueForLinter.ToString()));
                            break;

                        case LinterTypes.Iso5218:
                            if (!Linter.ValidateIso5218(valueForLinter))
                                item.AddValidationError(string.Format("Invalid ISO 5218 biological sex code '{0}'.", item.Value));
                            break;

                        case LinterTypes.Iban:
                            if (!Linter.ValidateIban(valueForLinter))
                                item.AddValidationError(string.Format("Invalid IBAN '{0}'.", valueForLinter.ToString()));
                            break;

                        case LinterTypes.NoZeroPrefix:
                            if (!Linter.ValidateNoZeroPrefix(valueForLinter))
                                item.AddValidationError(string.Format("Value '{0}' cannot have a leading zero.", valueForLinter.ToString()));
                            break;

                        case LinterTypes.Latitude:
                        case LinterTypes.Longitude:
                            if (!Linter.ValidateLatLong(valueForLinter))
                                item.AddValidationError(string.Format("Invalid {0} format '{1}'.", linter, valueForLinter.ToString()));
                            break;

                        case LinterTypes.PcEnc:
                            if (!Linter.ValidatePcEnc(valueForLinter))
                                item.AddValidationError(string.Format("Invalid characters in percent-encoded value '{0}'.", valueForLinter.ToString()));
                            break;

                        case LinterTypes.YesNo:
                            if (!Linter.ValidateYesNo(valueForLinter))
                                item.AddValidationError(string.Format("Requires a '0' or '1' but got '{0}'.", valueForLinter.ToString()));
                            break;

                        case LinterTypes.Hyphen:
                            if (!Linter.ValidateHyphen(valueForLinter))
                                item.AddValidationError($"Component must be a hyphen but got '{valueForLinter.ToString()}'.");
                            break;

                        case LinterTypes.ImporterIdx:
                            if (!Linter.ValidateImporterIdx(valueForLinter))
                                item.AddValidationError($"Invalid importer index '{valueForLinter.ToString()}'.");
                            break;

                        case LinterTypes.PackageType:
                            if (!Linter.ValidatePackageType(valueForLinter))
                                item.AddValidationError($"Invalid package type format '{valueForLinter.ToString()}'.");
                            break;

                        case LinterTypes.MediaType:
                            if (!Linter.ValidateMediaType(valueForLinter))
                                item.AddValidationError($"Invalid media type format '{valueForLinter.ToString()}'.");
                            break;

                        case LinterTypes.PosInSeqSlash:
                            if (!Linter.ValidatePosInSeqSlash(valueForLinter))
                                item.AddValidationError($"Invalid position-in-sequence format '{valueForLinter.ToString()}'.");
                            break;

                        case LinterTypes.NonZero:
                            if (!Linter.ValidateNonZero(valueForLinter))
                                item.AddValidationError($"Component cannot be zero but got '{valueForLinter.ToString()}'.");
                            break;

                        case LinterTypes.Winding:
                            if (!Linter.ValidateWinding(valueForLinter))
                                item.AddValidationError($"Invalid winding direction '{valueForLinter.ToString()}'.");
                            break;

                        case LinterTypes.PieceOfTotal:
                            if (!Linter.ValidatePieceOfTotal(valueForLinter))
                                item.AddValidationError($"Invalid piece-of-total format '{valueForLinter.ToString()}'.");
                            break;

                        case LinterTypes.HasNonDigit:
                            if (!Linter.ValidateHasNonDigit(valueForLinter))
                                item.AddValidationError($"Must contain at least one non-digit character in value '{valueForLinter.ToString()}'.");
                            break;

                        case LinterTypes.CouponCode:
                        case LinterTypes.CouponPosOffer:
                            //Not going to validate these for now
                            break;
                    }
                }
                currentPosition += segmentLength;
            }
        }
    }
}