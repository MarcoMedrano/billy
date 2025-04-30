using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Billy.Function.AzureContentUnderstanding;

public class AzureContentUnderstandingClient
(
    ILogger _logger,
        string _endpoint,
        string _apiVersion,
        string? _subscriptionKey = null,
        string? _apiToken = null,
        string _xMsUserAgent = "cu-sample-code"
        )
{

    private readonly Dictionary<string, string> _headers = GetHeaders(_subscriptionKey, _apiToken, _xMsUserAgent);
    private readonly HttpClient _httpClient = new HttpClient();

    /*
    Begins the analysis of a file or URL using the specified analyzer.

    Args:
        analyzerId (string): The ID of the analyzer to use.
        fileLocation (string): The path to the file or the URL to analyze.

    Returns:
        HttpResponseMessage: The response from the analysis request.
    */
    public async Task<HttpResponseMessage> BeginAnalyzeAsync(string analyzerId, string fileLocation)
    {
        HttpResponseMessage response;
        var requestHeaders = new Dictionary<string, string>(_headers);

        if (File.Exists(fileLocation))
        {
            var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(fileLocation));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetAnalyzeUrl(_endpoint, _apiVersion, analyzerId));
            foreach (var header in requestHeaders)
            {
                requestMessage.Headers.Add(header.Key, header.Value);
            }

            requestMessage.Content = fileContent;
            response = await _httpClient.SendAsync(requestMessage);
        }
        else if (fileLocation.StartsWith("https://") || fileLocation.StartsWith("http://"))
        {
            var jsonContent = JsonSerializer.Serialize(new { url = fileLocation });
            var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetAnalyzeUrl(_endpoint, _apiVersion, analyzerId));
            foreach (var header in requestHeaders)
            {
                requestMessage.Headers.Add(header.Key, header.Value);
            }

            requestMessage.Content = stringContent;
            response = await _httpClient.SendAsync(requestMessage);
        }
        else
        {
            throw new ArgumentException("File location must be a valid path or URL.");
        }

        response.EnsureSuccessStatusCode();
        _logger.LogInformation($"Analyzing file {fileLocation} with analyzer: {analyzerId}");

        return response;
    }

    /*
    Polls the result of an asynchronous operation until it completes or times out.
    Args:
        response (HttpResponseMessage): The initial response object containing the operation location.
        timeoutSeconds (int, optional): The maximum number of seconds to wait for the operation to complete. Defaults to 120.
        pollingIntervalSeconds (int, optional): The number of seconds to wait between polling attempts. Defaults to 2.

    Returns:
        JsonDocument: The JSON response of the completed operation if it succeeds.
    */
    public async Task<JsonDocument> PollResultAsync(HttpResponseMessage response, int timeoutSeconds = 120, int pollingIntervalSeconds = 2)
    {

        if (!response.Headers.TryGetValues("operation-location", out var operationLocationValues))
        {
            throw new ArgumentException("Operation location not found in response headers.");
        }

        string operationLocation = operationLocationValues.FirstOrDefault();
        if (string.IsNullOrEmpty(operationLocation))
        {
            throw new ArgumentException("Operation location not found in response headers.");
        }

        var requestHeaders = new Dictionary<string, string>(_headers);

        var startTime = DateTime.Now;
        while (true)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, operationLocation);
            foreach (var header in requestHeaders)
            {
                requestMessage.Headers.Add(header.Key, header.Value);
            }

            var elapsedTime = (DateTime.Now - startTime).TotalSeconds;
            _logger.LogInformation($"Waiting for service response (elapsed: {elapsedTime:F2}s)");

            if (elapsedTime > timeoutSeconds)
            {
                throw new TimeoutException($"Operation timed out after {timeoutSeconds:F2} seconds.");
            }

            var pollResponse = await _httpClient.SendAsync(requestMessage);
            pollResponse.EnsureSuccessStatusCode();

            var jsonResponse = await JsonDocument.ParseAsync(await pollResponse.Content.ReadAsStreamAsync());
            var status = string.Empty;

            if (jsonResponse.RootElement.TryGetProperty("status", out var statusElement))
            {
                status = statusElement.GetString()?.ToLower() ?? string.Empty;
            }

            if (status == "succeeded")
            {
                _logger.LogInformation($"Request result is ready after {elapsedTime:F2} seconds.");
                return jsonResponse;
            }
            else if (status == "failed")
            {
                _logger.LogError($"Request failed. Reason: {JsonSerializer.Serialize(jsonResponse)}");
                throw new InvalidOperationException("Request failed.");
            }
            else
            {
                string operationId = operationLocation.Split('/').Last().Split('?').First();
                _logger.LogInformation($"Request {operationId} in progress ...");
            }

            await Task.Delay(pollingIntervalSeconds * 1000);
        }
    }

    private string GetAnalyzeUrl(string endpoint, string apiVersion, string analyzerId)
    {
        return $"{endpoint}/contentunderstanding/analyzers/{analyzerId}:analyze?api-version={apiVersion}";
    }

    private static Dictionary<string, string> GetHeaders(string? subscriptionKey, string? apiToken, string xMsUserAgent)
    {
        if (string.IsNullOrEmpty(subscriptionKey) && string.IsNullOrEmpty(apiToken))
        {
            throw new ArgumentException("Either subscription key or API token must be provided.");
        }

        var headers = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(subscriptionKey))
        {
            headers["Ocp-Apim-Subscription-Key"] = subscriptionKey;
        }
        else
        {
            headers["Authorization"] = $"Bearer {apiToken}";
        }

        headers["x-ms-useragent"] = xMsUserAgent;
        return headers;
    }
}