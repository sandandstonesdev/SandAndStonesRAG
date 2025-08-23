using Microsoft.Net.Http.Headers;

namespace PdfEmbeddingsFuncApp
{

    public static class MultipartRequestHelper
    {
        public static string GetBoundary(string contentType)
        {
            var elements = contentType.Split(';');
            var boundary = elements
                .Select(element => element.Trim())
                .FirstOrDefault(element => element.StartsWith("boundary=", StringComparison.OrdinalIgnoreCase));
            if (boundary == null)
                throw new InvalidDataException("Missing content-type boundary.");
            return HeaderUtilities.RemoveQuotes(boundary.Substring("boundary=".Length)).Value;
        }
    }
}
