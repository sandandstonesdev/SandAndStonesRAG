using Azure.Storage.Blobs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using PdfEmbeddingsFuncApp;
using System.Net;

public class UploadFileFunction
{
    private readonly ILogger<UploadFileFunction> _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public UploadFileFunction(ILoggerFactory loggerFactory, BlobServiceClient blobServiceClient)
    {
        _logger = loggerFactory.CreateLogger<UploadFileFunction>();
        _blobServiceClient = blobServiceClient;
    }

    [Function("UploadFile")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "upload")] HttpRequestData req)
    {
        var response = req.CreateResponse();

        if (!req.Headers.TryGetValues("Content-Type", out var contentTypes) ||
            !contentTypes.Any(ct => ct.Contains("multipart/form-data")))
        {
            _logger.LogWarning("Invalid Content-Type");
            response.StatusCode = HttpStatusCode.BadRequest;
            await response.WriteStringAsync("Content-Type must be multipart/form-data");
            return response;
        }

        var boundary = MultipartRequestHelper.GetBoundary(req.Headers.GetValues("Content-Type").First());
        var reader = new MultipartReader(boundary, req.Body);

        string fileName = null;
        byte[] fileBytes = null;

        MultipartSection section;
        while ((section = await reader.ReadNextSectionAsync()) != null)
        {
            if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition) &&
                contentDisposition.DispositionType.Equals("form-data") &&
                !string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                fileName = contentDisposition.FileName.Value;
                using var ms = new MemoryStream();
                await section.Body.CopyToAsync(ms);
                fileBytes = ms.ToArray();
                break;
            }
        }

        if (fileBytes != null && fileName != null)
        {
            var container = _blobServiceClient.GetBlobContainerClient("documents");
            await container.CreateIfNotExistsAsync();
            var blobClient = container.GetBlobClient(fileName);

            using var ms = new MemoryStream(fileBytes);
            await blobClient.UploadAsync(ms, overwrite: true);

            _logger.LogInformation("File uploaded: {fileName}", fileName);

            var blobUrl = blobClient.Uri.ToString();

            response.StatusCode = HttpStatusCode.OK;
            await response.WriteAsJsonAsync(new
            {
                message = "File uploaded successfully.",
                fileName = fileName,
                blobUrl = blobUrl
            });
            return response;
        }

        _logger.LogWarning("No file found in the request.");
        response.StatusCode = HttpStatusCode.BadRequest;
        await response.WriteStringAsync("No file found in the request.");
        return response;
    }
}