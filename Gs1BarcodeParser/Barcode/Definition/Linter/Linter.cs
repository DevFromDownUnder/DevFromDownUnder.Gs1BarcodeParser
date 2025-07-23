using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System;
using System.Text.RegularExpressions;

namespace Gs1BarcodeParser.Barcode.Definition.Linter
{
    public static partial class Linter
    {
        public const string CSET32 = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        public const string CSET39 = "#-/0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string CSET82 = "!\"%&'()*+,-./0123456789:;<=>?ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
        public const string DIGITS = "0123456789";
        public const string HEX = "0123456789ABCDEFabcdef";
        public const string IMPORTERIDX = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";

        private static readonly HashSet<string> Iso3166Alpha2Codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AD", "AE", "AF", "AG", "AI", "AL", "AM", "AO", "AQ", "AR", "AS", "AT", "AU", "AW", "AX", "AZ",
            "BA", "BB", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BL", "BM", "BN", "BO", "BQ", "BR", "BS",
            "BT", "BV", "BW", "BY", "BZ", "CA", "CC", "CD", "CF", "CG", "CH", "CI", "CK", "CL", "CM", "CN",
            "CO", "CR", "CU", "CV", "CW", "CX", "CY", "CZ", "DE", "DJ", "DK", "DM", "DO", "DZ", "EC", "EE",
            "EG", "EH", "ER", "ES", "ET", "FI", "FJ", "FK", "FM", "FO", "FR", "GA", "GB", "GD", "GE", "GF",
            "GG", "GH", "GI", "GL", "GM", "GN", "GP", "GQ", "GR", "GS", "GT", "GU", "GW", "GY", "HK", "HM",
            "HN", "HR", "HT", "HU", "ID", "IE", "IL", "IM", "IN", "IO", "IQ", "IR", "IS", "IT", "JE", "JM",
            "JO", "JP", "KE", "KG", "KH", "KI", "KM", "KN", "KP", "KR", "KW", "KY", "KZ", "LA", "LB", "LC",
            "LI", "LK", "LR", "LS", "LT", "LU", "LV", "LY", "MA", "MC", "MD", "ME", "MF", "MG", "MH", "MK",
            "ML", "MM", "MN", "MO", "MP", "MQ", "MR", "MS", "MT", "MU", "MV", "MW", "MX", "MY", "MZ", "NA",
            "NC", "NE", "NF", "NG", "NI", "NL", "NO", "NP", "NR", "NU", "NZ", "OM", "PA", "PE", "PF", "PG",
            "PH", "PK", "PL", "PM", "PN", "PR", "PS", "PT", "PW", "PY", "QA", "RE", "RO", "RS", "RU", "RW",
            "SA", "SB", "SC", "SD", "SE", "SG", "SH", "SI", "SJ", "SK", "SL", "SM", "SN", "SO", "SR", "SS",
            "ST", "SV", "SX", "SY", "SZ", "TC", "TD", "TF", "TG", "TH", "TJ", "TK", "TL", "TM", "TN", "TO",
            "TR", "TT", "TV", "TW", "TZ", "UA", "UG", "UM", "US", "UY", "UZ", "VA", "VC", "VE", "VG", "VI",
            "VN", "VU", "WF", "WS", "YE", "YT", "ZA", "ZM", "ZW"
        };

        private static readonly HashSet<string> Iso3166NumericCodes = new HashSet<string>
        {
            "004", "008", "010", "012", "016", "020", "024", "028", "031", "032", "036", "040", "044", "048", "050",
            "051", "052", "056", "060", "064", "068", "070", "072", "074", "076", "084", "086", "090", "092", "096",
            "100", "104", "108", "112", "116", "120", "124", "132", "136", "140", "144", "148", "152", "156", "158",
            "162", "166", "170", "174", "175", "178", "180", "184", "188", "191", "192", "196", "203", "204", "208",
            "212", "214", "218", "222", "226", "231", "232", "233", "234", "238", "239", "242", "246", "248", "250",
            "254", "258", "260", "262", "266", "268", "270", "275", "276", "288", "292", "296", "300", "304", "308",
            "312", "316", "320", "324", "328", "332", "334", "336", "340", "344", "348", "352", "356", "360", "364",
            "368", "372", "376", "380", "384", "388", "392", "398", "400", "404", "408", "410", "414", "417", "418",
            "422", "426", "428", "430", "434", "438", "440", "442", "446", "450", "454", "458", "462", "466", "470",
            "474", "478", "480", "484", "492", "496", "498", "499", "500", "504", "508", "512", "516", "520", "524",
            "528", "531", "533", "534", "535", "540", "548", "554", "558", "562", "566", "570", "574", "578", "580",
            "581", "583", "584", "585", "586", "591", "598", "600", "604", "608", "612", "616", "620", "624", "626",
            "630", "634", "638", "642", "643", "646", "652", "654", "659", "660", "662", "663", "666", "670", "674",
            "678", "682", "686", "688", "690", "694", "702", "703", "704", "705", "706", "710", "716", "724", "728",
            "729", "732", "736", "740", "744", "748", "752", "756", "760", "762", "764", "768", "772", "776", "780",
            "784", "788", "792", "795", "796", "798", "800", "804", "807", "818", "826", "831", "832", "833", "834",
            "840", "850", "854", "858", "860", "862", "876", "882", "887", "894"
        };

        private static readonly HashSet<string> Iso4217NumericCodes = new HashSet<string>
        {
            "008", "012", "032", "036", "044", "048", "050", "051", "052", "060", "064", "068", "072", "084", "090",
            "096", "104", "108", "116", "124", "132", "136", "144", "152", "156", "170", "174", "188", "191", "192",
            "203", "208", "214", "218", "222", "230", "232", "238", "242", "262", "270", "292", "320", "324", "328",
            "332", "340", "344", "348", "352", "356", "360", "364", "368", "376", "388", "392", "398", "400", "404",
            "408", "410", "414", "417", "418", "422", "426", "428", "430", "434", "440", "446", "454", "458", "462",
            "478", "480", "484", "496", "498", "504", "512", "516", "524", "532", "533", "548", "554", "558", "566",
            "578", "586", "590", "598", "600", "604", "608", "634", "643", "646", "654", "682", "690", "694", "702",
            "704", "706", "710", "728", "748", "752", "756", "760", "764", "776", "780", "784", "788", "800", "807",
            "818", "826", "834", "840", "858", "860", "882", "886", "894", "901", "925", "926", "927", "928", "929",
            "930", "931", "932", "933", "934", "936", "937", "938", "940", "941", "943", "944", "946", "947", "948",
            "949", "950", "951", "952", "953", "955", "956", "957", "958", "959", "960", "961", "962", "963", "964",
            "965", "967", "968", "969", "970", "971", "972", "973", "975", "976", "977", "978", "979", "980", "981",
            "984", "985", "986", "990", "994", "997", "999"
        };

        private static readonly HashSet<string> PackagingCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "1A", "1B", "1D", "1F", "1G", "1W", "200", "201", "202", "203", "204", "205", "206", "210", "211", "212", "2C", "3A", "3H", "43", "44", "4A", "4B", "4C", "4D", "4F", "4G", "4H", "5H", "5L", "5M", "6H", "6P", "7A", "7B", "8", "8A", "8B", "8C", "9",
            "AA", "AB", "AC", "AD", "AF", "AG", "AH", "AI", "AJ", "AL", "AM", "AP", "APE", "AT", "AV",
            "B4", "BB", "BC", "BD", "BE", "BF", "BG", "BGE", "BH", "BI", "BJ", "BK", "BL", "BM", "BME", "BN", "BO", "BP", "BQ", "BR", "BRI", "BS", "BT", "BU", "BV", "BW", "BX", "BY", "BZ",
            "CA", "CB", "CBL", "CC", "CCE", "CD", "CE", "CF", "CG", "CH", "CI", "CJ", "CK", "CL", "CM", "CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX", "CY", "CZ",
            "DA", "DB", "DC", "DG", "DH", "DI", "DJ", "DK", "DL", "DM", "DN", "DP", "DPE", "DR", "DS", "DT", "DU", "DV", "DW", "DX", "DY",
            "E1", "E2", "E3", "EC", "ED", "EE", "EF", "EG", "EH", "EI", "EN",
            "FB", "FC", "FD", "FE", "FI", "FL", "FO", "FOB", "FP", "FPE", "FR", "FT", "FW", "FX",
            "GB", "GI", "GL", "GR", "GU", "GY", "GZ",
            "HA", "HB", "HC", "HG", "HN", "HR",
            "IA", "IB", "IC", "ID", "IE", "IF", "IG", "IH", "IK", "IL", "IN", "IZ",
            "JB", "JC", "JG", "JR", "JT", "JY",
            "KG", "KI",
            "LAB", "LE", "LG", "LT", "LU", "LV", "LZ",
            "MA", "MB", "MC", "ME", "MPE", "MR", "MS", "MT", "MW", "MX",
            "NA", "NE", "NF", "NG", "NS", "NT", "NU", "NV",
            "OA", "OB", "OC", "OD", "OE", "OF", "OK", "OPE", "OT", "OU",
            "P2", "PA", "PAE", "PB", "PC", "PD", "PE", "PF", "PG", "PH", "PI", "PJ", "PK", "PL", "PLP", "PN", "PO", "POP", "PP", "PPE", "PR", "PT", "PU", "PUE", "PV", "PX", "PY", "PZ",
            "QA", "QB", "QC", "QD", "QF", "QG", "QH", "QJ", "QK", "QL", "QM", "QN", "QP", "QQ", "QR", "QS",
            "RB1", "RB2", "RB3", "RCB", "RD", "RG", "RJ", "RK", "RL", "RO", "RT", "RZ",
            "S1", "SA", "SB", "SC", "SD", "SE", "SEC", "SH", "SI", "SK", "SL", "SM", "SO", "SP", "SS", "ST", "STL", "SU", "SV", "SW", "SX", "SY", "SZ",
            "T1", "TB", "TC", "TD", "TE", "TEV", "TG", "THE", "TI", "TK", "TL", "TN", "TO", "TR", "TRE", "TS", "TT", "TTE", "TU", "TV", "TW", "TWE", "TY", "TZ",
            "UC", "UN", "UUE",
            "VA", "VG", "VI", "VK", "VL", "VN", "VO", "VP", "VQ", "VR", "VS", "VY",
            "WA", "WB", "WC", "WD", "WF", "WG", "WH", "WJ", "WK", "WL", "WM", "WN", "WP", "WQ", "WR", "WRP", "WS", "WT", "WU", "WV", "WW", "WX", "WY", "WZ",
            "X11", "X12", "X15", "X16", "X17", "X18", "X19", "X20", "X3", "XA", "XB", "XC", "XD", "XF", "XG", "XH", "XJ", "XK",
            "YA", "YB", "YC", "YD", "YF", "YG", "YH", "YJ", "YK", "YL", "YM", "YN", "YP", "YQ", "YR", "YS", "YT", "YV", "YW", "YX", "YY", "YZ",
            "ZA", "ZB", "ZC", "ZD", "ZF", "ZG", "ZH", "ZJ", "ZK", "ZL", "ZM", "ZN", "ZP", "ZQ", "ZR", "ZS", "ZT", "ZU", "ZV", "ZW", "ZX", "ZY", "ZZ"
        };

        private static readonly uint[] Primes = new uint[]
        {
            2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89,
            97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223,
            227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359,
            367, 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503,
            509
        };

        public static bool ValidateChecksum(ReadOnlySpan<char> dataWithChecksum)
        {
            if (dataWithChecksum.IsEmpty) return false;

            if (SpanContainsAnyExcept(dataWithChecksum, DIGITS))
            {
                return false;
            }

            int sum = 0;
            bool isOddPosition = true;

            for (int i = dataWithChecksum.Length - 2; i >= 0; i--)
            {
                int digit = dataWithChecksum[i] - '0';
                sum += isOddPosition ? digit * 3 : digit;
                isOddPosition = !isOddPosition;
            }

            int calculatedChecksum = (10 - sum % 10) % 10;
            int providedChecksum = dataWithChecksum[dataWithChecksum.Length - 1] - '0';

            return calculatedChecksum == providedChecksum;
        }

        public static bool ValidateCsumAlpha(ReadOnlySpan<char> data)
        {
            if (data.Length < 2) return false;
            if (data.Length - 2 > Primes.Length) return false;

            if (SpanContainsAnyExcept(data.Slice(0, data.Length - 2), CSET82))
            {
                return false;
            }

            if (SpanContainsAnyExcept(data.Slice(data.Length - 2), CSET32))
            {
                return false;
            }

            uint sum = 0;
            if (data.Length > 2)
            {
                for (int i = 0; i < data.Length - 2; i++)
                {
                    uint prime = Primes[data.Length - 3 - i];
                    sum += (uint)CSET82.IndexOf(data[i]) * prime;
                }
                sum %= 1021;
            }

            char expectedChar1 = CSET32[(int)(sum >> 5)];
            char expectedChar2 = CSET32[(int)(sum & 31)];

            return data[data.Length - 2] == expectedChar1 && data[data.Length - 1] == expectedChar2;
        }

        public static bool ValidateHasGs1Prefix(ReadOnlySpan<char> value)
        {
            if (value.Length < 4) return false;

            return !SpanContainsAnyExcept(value.Slice(0, 4), DIGITS);
        }

        public static bool ValidateHasGs1PrefixOffset1(ReadOnlySpan<char> value)
        {
            if (value.Length < 5) return false;

            return !SpanContainsAnyExcept(value.Slice(1, 4), DIGITS);
        }

        public static bool ValidateHasNonDigit(ReadOnlySpan<char> value)
        {
            return SpanContainsAnyExcept(value, DIGITS);
        }

        public static bool ValidateHexCharacters(ReadOnlySpan<char> value)
        {
            return !SpanContainsAnyExcept(value, HEX);
        }

        public static bool ValidateHyphen(ReadOnlySpan<char> value)
        {
            return !SpanContainsAnyExcept(value, "-");
        }

        public static bool ValidateIban(ReadOnlySpan<char> iban)
        {
            if (iban.IsWhiteSpace() || iban.Length < 5 || iban.Length > 34) return false;

            Span<char> rearrangedIban = stackalloc char[iban.Length];

            iban.Slice(4).CopyTo(rearrangedIban);
            iban.Slice(0, 4).CopyTo(rearrangedIban.Slice(iban.Length - 4));

            Span<char> numericIban = stackalloc char[iban.Length * 2];
            int currentPos = 0;

            foreach (char c in rearrangedIban)
            {
                if (char.IsLetter(c))
                {
                    if ((c - 'A' + 10).ToString().AsSpan().TryCopyTo(numericIban.Slice(currentPos)))
                    {
                        currentPos += 2;
                    }
                }
                else if (char.IsDigit(c))
                {
                    numericIban[currentPos++] = c;
                }
                else
                {
                    return false;
                }
            }

            return BigInteger.Parse(numericIban.Slice(0, currentPos).ToString()) % 97 == 1;
        }

        public static bool ValidateImporterIdx(ReadOnlySpan<char> value)
        {
            return value.Length == 1 && !SpanContainsAnyExcept(value, IMPORTERIDX);
        }

        public static bool ValidateIso3166(ReadOnlySpan<char> code, bool allow999 = false)
        {
            if (code.Length != 3 || !IsAllDigits(code)) return false;

            var codeStr = code.ToString();
            if (allow999 && codeStr == "999") return true;

            return Iso3166NumericCodes.Contains(codeStr);
        }

        public static bool ValidateIso3166Alpha2(ReadOnlySpan<char> code)
        {
            return code.Length == 2 && Iso3166Alpha2Codes.Contains(code.ToString());
        }

        public static bool ValidateIso4217(ReadOnlySpan<char> code)
        {
            if (code.Length != 3 || !IsAllDigits(code)) return false;

            return Iso4217NumericCodes.Contains(code.ToString());
        }

        public static bool ValidateIso5218(ReadOnlySpan<char> value)
        {
            return value.Length == 1 && "0129".IndexOf(value[0]) != -1;
        }

        public static bool ValidateLatLong(ReadOnlySpan<char> value)
        {
            return value.Length == 10 && IsAllDigits(value) && long.Parse(value.ToString()) <= 1800000000;
        }

        public static bool ValidateMediaType(ReadOnlySpan<char> value)
        {
            return value.Length == 2 && IsAllDigits(value);
        }

        public static bool ValidateNonZero(ReadOnlySpan<char> value)
        {
            if (!IsAllDigits(value)) return false;

            foreach (char c in value)
            {
                if (c != '0') return true;
            }
            return false;
        }

        public static bool ValidateNoZeroPrefix(ReadOnlySpan<char> value)
        {
            return !value.StartsWith("0".AsSpan());
        }

        public static bool ValidatePackageType(ReadOnlySpan<char> value)
        {
            return PackagingCodes.Contains(value.ToString());
        }

        public static bool ValidatePcEnc(ReadOnlySpan<char> value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '%')
                {
                    if (i + 2 >= value.Length || !ValidateHexCharacters(value.Slice(i + 1, 2)))
                    {
                        return false;
                    }
                    i += 2;
                }
            }
            return true;
        }

        public static bool ValidatePieceOfTotal(ReadOnlySpan<char> value)
        {
            if (value.Length != 4 || !IsAllDigits(value)) return false;

            int piece = int.Parse(value.Slice(0, 2).ToString());
            int total = int.Parse(value.Slice(2).ToString());

            return piece > 0 && total > 0 && piece <= total;
        }

        public static bool ValidatePosInSeqSlash(ReadOnlySpan<char> value)
        {
            int slashIndex = value.IndexOf('/');
            if (slashIndex == -1) return false;

            var posSpan = value.Slice(0, slashIndex);
            var endSpan = value.Slice(slashIndex + 1);

            if (int.TryParse(posSpan.ToString(), out int pos) && int.TryParse(endSpan.ToString(), out int end))
            {
                return pos > 0 && end > 0 && pos <= end;
            }

            return false;
        }

        public static bool ValidateWinding(ReadOnlySpan<char> value)
        {
            return value.Length == 1 && !SpanContainsAnyExcept(value, "019");
        }

        public static bool ValidateYesNo(ReadOnlySpan<char> value)
        {
            return value.Length == 1 && (value[0] == '0' || value[0] == '1');
        }

        public static bool ValidateYYMMD0(ReadOnlySpan<char> date)
        {
            if (date.Length == 6 && date.EndsWith("00".AsSpan()))
            {
                return DateTime.TryParseExact(date.Slice(0, 4).ToString(), "yyMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
            }

            return DateTime.TryParseExact(date.ToString(), "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        private static bool IsAllDigits(ReadOnlySpan<char> value)
        {
            return !SpanContainsAnyExcept(value, DIGITS);
        }

        private static bool SpanContainsAnyExcept(ReadOnlySpan<char> span, string validChars)
        {
            foreach (char c in span)
            {
                if (validChars.IndexOf(c) == -1)
                {
                    return true;
                }
            }
            return false;
        }
    }
}