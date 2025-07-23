namespace Gs1BarcodeParser.Tests
{
    [TestClass]
    public sealed class DigitalLinkTests
    {
        /// <summary>
        /// Verifies that GS1 Digital Link URIs are parsed correctly according to qualifier ordering rules.
        /// </summary>
        [DataTestMethod]
        [DataRow("DL - Valid qualifier order", "https://id.gs1.org/01/09501101530003/22/CPV123/10/BATCH1", true, "01", "09501101530003", "22", "CPV123", "10", "BATCH1")]
        [DataRow("DL - Valid alternative qualifier", "https://id.gs1.org/01/09501101530003/235/TPX456", true, "01", "09501101530003", "235", "TPX456")]
        [DataRow("DL - Valid primary key with no qualifiers", "https://id.gs1.org/00/123456789012345675", true, "00", "123456789012345675")]
        [DataRow("DL - Valid with query parameters", "https://id.gs1.org/01/09501101530003?22=CPV1&10=BATCH1", true, "01", "09501101530003", "22", "CPV1", "10", "BATCH1")]
        [DataRow("DL - Valid with path and query qualifiers", "https://id.gs1.org/01/09501101530003/22/CPV1?10=BATCH1", true, "01", "09501101530003", "22", "CPV1", "10", "BATCH1")]
        [DataRow("DL - Invalid qualifier order (path)", "https://id.gs1.org/01/09501101530003/10/BATCH1/22/CPV123", false)]
        [DataRow("DL - Invalid mix of alternative qualifiers", "https://id.gs1.org/01/09501101530003/22/CPV123/235/TPX456", false)]
        [DataRow("DL - Invalid - qualifier not allowed", "https://id.gs1.org/00/123456789012345675/10/EXTRA", false)]
        [DataRow("DL - Invalid with query parameters in wrong order", "https://id.gs1.org/01/09501101530003?10=BATCH1&22=CPV1", false)]
        public void Parse_DigitalLinkUris_ShouldGiveExpectedResult(string displayName, string uri, bool expectedIsValid, params object[] expectedAiValuePairs)
        {
            // This method has the same logic as the one above but is separated for clarity between element strings and DL URIs.
            (new BarcodeStringTests()).Parse_BarcodeStrings_ShouldGiveExpectedResult(displayName, uri, expectedIsValid, expectedAiValuePairs);
        }
    }
}
