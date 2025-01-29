namespace Olive;

using Newtonsoft.Json;
using Olive;
using Olive.BlobAzure;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

public class DocumentAnalyze
{
    string endpoint = AppConfig.AzureDocumentIntelligenceEndpoint;
    string apiKey = AppConfig.AzureDocumentIntelligenceApiKey;
    string apiVersion = AppConfig.APIVersion;
    readonly HttpClient httpClient = new HttpClient(); // Initialize HttpClient here;

    public async Task<BuildDocumentAnalyzerResponse> BuildDocumentAnalyzer(string modelId, string trainingFolderName, string description = null, string storageContainerName = null)
    {
        var containerUrl = new SASTokenGenerator().GenerateContainerSasUri(storageContainerName);
        var payload = new
        {
            modelId = modelId,
            description = description,
            buildMode = "template",
            azureBlobSource = new
            {
                containerUrl = containerUrl,
                prefix = $"{trainingFolderName}/"
            }
        };
        var requestUrl = $"{endpoint}/formrecognizer/documentModels:build?api-version={apiVersion}";
        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
        // Serialize the request body to JSON
        request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"); ;
        try
        {
            // Send the request and get the response
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                // Read the Operation-Location header
                if (response.Headers.TryGetValues("Operation-Location", out var operationLocation))
                {
                    var resultUrl = operationLocation.FirstOrDefault(); // Return the URL to check the operation status;
                    if (resultUrl is not null)
                    {
                        return await GetOperationStatusAsync(resultUrl);
                    }
                    else
                    {
                        throw new Exception("No operation location header found in response.");
                    }
                }
                else
                {
                    throw new Exception("Operation-Location header is missing in the response.");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to build document analyzer. Status Code: {response.StatusCode}, Details: {errorContent}");
            }
        }
        catch (HttpRequestException httpRequestException)
        {
            // Log and handle exception (e.g., rethrow, return a default value, etc.)
            Console.Error.WriteLine($"Request error: {httpRequestException.Message}");
            throw;  // Optionally rethrow the exception or return a custom error message
        }
        catch (Exception ex)
        {
            // Log and handle other exceptions
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
    }

    public async Task<DocumentAnalyzeModelResponse> ListDocumentAnalyzeModelsAsync(bool skipPrebuilt = true, bool useFormRecognizer = false)
    {
        var requestUrl = $"{endpoint}/{(useFormRecognizer ? "formrecognizer" : "documentintelligence")}/documentModels?api-version={apiVersion}";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);

        request.Content = new StringContent(string.Empty);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        try
        {
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();

            // Check for null or empty JSON response
            if (string.IsNullOrEmpty(jsonResponse))
            {
                Console.Error.WriteLine("The response from the API was null or empty.");
                return null; // or throw a custom exception if preferred
            }

            // Deserialize JSON response to DocumentModelResponse
            var documentModelResponse = JsonConvert.DeserializeObject<DocumentAnalyzeModelResponse>(jsonResponse);
            // Optionally filter out models with ModelId starting with "prebuilt"
            if (skipPrebuilt && documentModelResponse.Value.HasAny())
            {
                documentModelResponse.Value = documentModelResponse.Value
                    .Where(model => !model.ModelId.StartsWith("prebuilt", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            return documentModelResponse;
        }
        catch (HttpRequestException httpRequestException)
        {
            // Log and handle exception (e.g., rethrow, return a default value, etc.)
            Console.Error.WriteLine($"Request error: {httpRequestException.Message}");
            throw;  // Optionally rethrow the exception or return a custom error message
        }
        catch (Exception ex)
        {
            // Log and handle other exceptions
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }

    }

    public async Task<string> AnalyzeDocumentAsync(byte[] fileData, string modelId, bool useFormRecognizer = false)
    {
        var requestUrl = $"{endpoint}/{(useFormRecognizer ? "formrecognizer" : "documentintelligence")}/documentModels/{modelId}:analyze?api-version={apiVersion}";
        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

        // Convert the file to a Base64 string
        string base64String = Convert.ToBase64String(fileData);

        // Create the JSON payload
        var payload = new
        {
            base64Source = base64String
        };

        request.Content = new StringContent(JsonConvert.SerializeObject(payload));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        try
        {
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var resultUrl = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                if (resultUrl is not null)
                {
                    return resultUrl;
                }
                else
                {
                    throw new Exception("No operation location header found in response.");
                }
            }
            else
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to analyze document. Error Message: {message}");
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<DocumentAnalyzeResult> AnalyzeDocumentAsyncWithPoll(byte[] fileData, string modelId, bool useFormRecognizer = false)
    {
        var requestUrl = $"{endpoint}/{(useFormRecognizer ? "formrecognizer" : "documentintelligence")}/documentModels/{modelId}:analyze?api-version={apiVersion}";
        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

        // Convert the file to a Base64 string
        string base64String = Convert.ToBase64String(fileData);

        // Create the JSON payload
        var payload = new
        {
            base64Source = base64String
        };

        request.Content = new StringContent(JsonConvert.SerializeObject(payload));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        try
        {
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var resultUrl = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                if (resultUrl is not null)
                {
                    return await PollForResults(resultUrl);
                }
                else
                {
                    throw new Exception("No operation location header found in response.");
                }
            }
            else
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to analyze document. Error Message: {message}");
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    private async Task<DocumentAnalyzeResult> PollForResults(string resultUrl)
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

        while (true)
        {
            HttpResponseMessage response = await httpClient.GetAsync(resultUrl);
            string jsonResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<DocumentAnalyzeResult>(jsonResponse);

                // Check if the status is "succeeded"
                if (result?.Status == "succeeded")
                {
                    return result;
                }
                else if (result?.Status == "failed")
                {
                    throw new Exception("Document analysis failed.");
                }
            }
            else
            {
                throw new Exception("Failed to poll for results.");
            }
            // Wait before the next poll
            await Task.Delay(3000);
        }
    }

    public async Task<DocumentAnalyzeResult> GetPollResults(string resultUrl)
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

        HttpResponseMessage response = await httpClient.GetAsync(resultUrl);
        string jsonResponse = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<DocumentAnalyzeResult>(jsonResponse);
        }
        else
        {
            throw new Exception("Failed to poll for results.");
        }
    }

    async Task<BuildDocumentAnalyzerResponse> GetOperationStatusAsync(string operationUrl)
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

        while (true)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, operationUrl);
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<BuildDocumentAnalyzerResponse>(jsonResponse);
                if (result?.Status == "succeeded")
                {
                    return result; // Operation completed successfully
                }
                else if (result?.Status == "failed")
                {
                    throw new Exception("Document model build operation failed.");
                }
                else
                {
                    // Wait and continue polling
                    await Task.Delay(3000); // Adjust the delay as necessary
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get operation status. Status Code: {response.StatusCode}, Details: {errorContent}");
            }
        }
    }
}
