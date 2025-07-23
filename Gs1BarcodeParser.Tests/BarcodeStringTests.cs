using System.Text;
using Gs1BarcodeParser;

namespace Gs1BarcodeParser.Tests
{
    [TestClass]
    public sealed class BarcodeStringTests
    {
        /// <summary>
        /// Verifies that parsed barcodes match the expected validity and contain the correct Application Identifier (AI) data.
        /// This method covers standard GS1 element strings, including various data formats, edge cases, and error conditions.
        /// </summary>
        [DataTestMethod]
        // --- Basic Valid Combinations ---
        [DataRow("Valid [GTIN, BATCH]", "010950110153000310BATCH123<FNC1>", true, "01", "09501101530003", "10", "BATCH123")]
        [DataRow("Valid [GTIN, SERIAL]", "010950110153000321SERIAL98765<FNC1>", true, "01", "09501101530003", "21", "SERIAL98765")]
        [DataRow("Valid [GTIN, PROD_DATE, BEST_BEFORE_DATE]", "01095011015300031124010115241231", true, "01", "09501101530003", "11", "240101", "15", "241231")]
        [DataRow("Valid [GTIN, EXPIRY, BATCH]", "01095011015300031725123110ABC-123<FNC1>", true, "01", "09501101530003", "17", "251231", "10", "ABC-123")]
        [DataRow("Valid [SSCC]", "00123456789012345675", true, "00", "123456789012345675")]
        [DataRow("Valid [EXPIRY] (Day is 00)", "010950110153000317251200", true, "01", "09501101530003", "17", "251200")]

        // --- Variable Length AI Tests ---
        [DataRow("Valid [GTIN] followed by two variable length AIs", "010950110153000310BATCH123<FNC1>21SERIAL123<FNC1>", true, "01", "09501101530003", "10", "BATCH123", "21", "SERIAL123")]
        [DataRow("Valid variable length AI at the end (no FNC1 needed)", "010950110153000310BATCH-XYZ", true, "01", "09501101530003", "10", "BATCH-XYZ")]
        [DataRow("Valid - Long batch and serial numbers", "010950110153000310VERY-LONG-BATCH-NO-1<FNC1>21VERY-LONG-SERIAL-NO-1<FNC1>", true, "01", "09501101530003", "10", "VERY-LONG-BATCH-NO-1", "21", "VERY-LONG-SERIAL-NO-1")]

        // --- Date Error Tests ---
        [DataRow("Invalid Expiry Date (Month > 12)", "010950110153000317251301", false)]
        [DataRow("Invalid Expiry Date (Day > 31)", "010950110153000317251232", false)]
        [DataRow("Invalid Production Date (Invalid month)", "010950110153000311240015", false)]

        // --- Length and Format Error Tests ---
        [DataRow("Invalid GTIN (too short)", "01095011015300", false)]
        [DataRow("Invalid Date (too short)", "01095011015300031725123", false)]
        [DataRow("Invalid Character in Numeric AI (GTIN)", "01ABCDEFGH123456", false)]
        [DataRow("Invalid Missing FNC1 between variable length AIs", "010950110153000310BATCH121SERIAL123", true, "01", "09501101530003", "10", "BATCH121SERIAL123")]

        // --- Edge Case Tests ---
        [DataRow("Empty Barcode", "", true)]
        [DataRow("Barcode with only FNC1", "<FNC1>", true)]
        [DataRow("AI with no data before next AI", "010950110153000310<FNC1>17251231", false)]
        [DataRow("AI with no data at the end", "010950110153000310", false)]

        // --- AI with Implied Decimal Point ---
        [DataRow("Valid Net Weight in kg (310n)", "01095011015300033102001250", true, "01", "09501101530003", "3102", "001250")]
        [DataRow("Valid Net Weight in lbs (320n)", "01095011015300033203000550", true, "01", "09501101530003", "3203", "000550")]

        // --- More Complex/Miscellaneous Tests ---
        [DataRow("Unrecognized AI", "ABCD12345", false)]
        [DataRow("Invalid data in a numeric-only AI", "010950110153000330ABC", false)]
        public void Parse_BarcodeStrings_ShouldGiveExpectedResult(string displayName, string barcode, bool expectedIsValid, params object[] expectedAiValuePairs)
        {
            // Replace placeholder with the actual FNC1 character for parsing.
            var finalBarcode = barcode.Replace("<FNC1>", BarcodeParser.FNC1.ToString());

            var result = BarcodeParser.Parse(finalBarcode);

            Assert.AreEqual(expectedIsValid, result.IsValid, $"'{displayName}': Validity check failed.");

            // Only check AI/value pairs if the test case expects them.
            if (expectedAiValuePairs?.Length > 0)
            {
                Assert.AreEqual(expectedAiValuePairs.Length / 2, result.Values.Count, $"'{displayName}': Incorrect number of AIs parsed.");

                for (int i = 0; i < expectedAiValuePairs.Length; i += 2)
                {
                    var expectedAi = (string)expectedAiValuePairs[i];
                    var expectedValue = (string)expectedAiValuePairs[i + 1];

                    var item = result.Values.FirstOrDefault(v => v.Definition.RawAI == expectedAi);
                    Assert.IsNotNull(item, $"'{displayName}': Expected AI '{expectedAi}' not found.");
                    Assert.AreEqual(expectedValue, item.Value, $"'{displayName}': Value for AI '{expectedAi}' did not match.");
                }
            }
        }
    }
}
