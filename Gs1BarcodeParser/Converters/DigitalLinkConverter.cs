using System;
using System.Text;
using System.Web;

namespace Gs1BarcodeParser.Converters
{
    public static class DigitalLinkConverter
    {
        public static string ToBarcodeString(string barcodeUri)
        {
            if (barcodeUri.Contains("#"))
            {
                // Remove the fragment part of the URI if it exists
                barcodeUri = barcodeUri.Substring(0, barcodeUri.IndexOf('#'));
            }

            if (!Uri.TryCreate(barcodeUri, UriKind.Absolute, out var uri))
            {
                return string.Empty;
            }

            var barcode = new StringBuilder();
            var pathSegments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < pathSegments.Length; i += 2)
            {
                if (i + 1 < pathSegments.Length)
                {
                    string ai = pathSegments[i];
                    string value = Uri.UnescapeDataString(pathSegments[i + 1]);
                    barcode.Append(ai).Append(value).Append(BarcodeParser.FNC1);
                }
            }

            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            foreach (string key in queryParams.AllKeys)
            {
                if (key == null) continue;

                string unescapedValue = queryParams[key];
                if (unescapedValue == null) continue;

                string value = Uri.UnescapeDataString(unescapedValue);
                barcode.Append(key).Append(value).Append(BarcodeParser.FNC1);
            }

            if (barcode.Length > 0)
            {
                barcode.Length--;
            }

            return barcode.ToString();
        }
    }
}