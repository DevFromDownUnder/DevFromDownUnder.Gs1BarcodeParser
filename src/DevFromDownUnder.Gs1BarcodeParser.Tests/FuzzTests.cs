using System.Text;
using DevFromDownUnder.Gs1BarcodeParser;

namespace DevFromDownUnder.Gs1BarcodeParser.Tests
{
    [TestClass]
    public sealed class FuzzTests
    {
        /// <summary>
        /// Runs a fuzz test by generating a large number of random strings to check for unhandled exceptions.
        /// An unhandled exception will automatically fail the test.
        /// </summary>
        [TestMethod]
        [DataRow(100000, 100)] // 100,000 iterations with max string length of 100
        public void RunFuzzTests(int iterations, int maxStringLength)
        {
            var random = new Random();
            const string numberChars = "0123456789";
            const string alphaChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-._~";
            const string specialChars = "!*'();:@&=+$,/?%#[]";

            var otherCharList = (alphaChars + specialChars).ToList();
            otherCharList.Add(BarcodeParser.FNC1);
            var otherChars = otherCharList.ToArray();

            const double numberWeight = 0.85;

            for (int i = 0; i < iterations; i++)
            {
                int length = random.Next(1, maxStringLength);
                var builder = new StringBuilder(length);
                for (int j = 0; j < length; j++)
                {
                    if (random.NextDouble() < numberWeight)
                    {
                        builder.Append(numberChars[random.Next(numberChars.Length)]);
                    }
                    else
                    {
                        builder.Append(otherChars[random.Next(otherChars.Length)]);
                    }
                }
                string fuzzInput = builder.ToString();

                // Randomly prepend a URL prefix to test Digital Link handling.
                if (random.Next(10) > 6)
                {
                    fuzzInput = "https://id.gs1.org/" + fuzzInput;
                }

                try
                {
                    BarcodeParser.Parse(fuzzInput);
                }
                catch (Exception ex)
                {
                    // If any unexpected exception occurs, fail the test with a detailed message.
                    var cleanInput = fuzzInput.Replace(BarcodeParser.FNC1.ToString(), "<FNC1>");
                    Assert.Fail($"Fuzz test failed on iteration {i + 1} with input '{cleanInput}'. Exception: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}
