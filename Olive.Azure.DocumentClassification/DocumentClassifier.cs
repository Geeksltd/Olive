using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Newtonsoft.Json;
using Olive;
using Azure.AI.DocumentIntelligence;

namespace Olive;

public class DocumentClassifier
{
    string endpoint = AppConfig.AzureDocumentIntelligenceEndpoint;
    string apiKey = AppConfig.AzureDocumentIntelligenceApiKey;
    string apiVersion = AppConfig.APIVersion;


    //public async Task<DocumentClassifierDetails> BuildDocumentClassifier(string classifierId , List<string> documentTypes , string storageContainerName = null)
    //{
    //    // Ensure the documentTypes list is provided
    //    if (documentTypes == null || documentTypes.Count == 0)
    //    {
    //        throw new ArgumentException("documentTypes must be provided and cannot be empty.");
    //    }
    //    Uri trainingFilesUri = new SASTokenGenerator().GenerateContainerSasUri(storageContainerName);

    //    //Uri trainingFilesUri = new Uri("<trainingFilesUri>");
    //    var client = new DocumentIntelligenceClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

    //    // Dictionary to hold the document types and their associated BlobContentSource
    //    var documentTypeDetails = new Dictionary<string, ClassifierDocumentTypeDetails>();

    //    // Iterate through the document types and create the BlobContentSource and ClassifierDocumentTypeDetails dynamically
    //    foreach (var documentType in documentTypes)
    //    {
    //        // Generate prefix based on the document type (assuming prefix is simply the document type name)
    //        var prefix = $"{documentType}/";

    //        // Create BlobContentSource using the generated prefix
    //        var source = new BlobContentSource(trainingFilesUri) { Prefix = prefix };

    //        // Add to the dictionary
    //        documentTypeDetails.Add(documentType, new ClassifierDocumentTypeDetails(source));
    //    }

    //    BuildDocumentClassifierOperation operation = await client.BuildDocumentClassifierAsync(WaitUntil.Completed, documentTypeDetails, classifierId);
    //    DocumentClassifierDetails classifier = operation.Value;

    //    return classifier;
    //    //Console.WriteLine($"  Classifier Id: {classifier.ClassifierId}");
    //    //Console.WriteLine($"  Created on: {classifier.CreatedOn}");

    //    //Console.WriteLine("  Document types the classifier can recognize:");
    //    //foreach (KeyValuePair<string, ClassifierDocumentTypeDetails> documentType in classifier.DocumentTypes)
    //    //{
    //    //    Console.WriteLine($"    {documentType.Key}");
    //    //}
    //}

    public async Task<DocumentClassifierResponse> ListDocumentClassifiersAsync(bool useFormRecognizer = false)
    {
        using (var client = new HttpClient())
        {
            var requestUrl = $"{endpoint}/{(useFormRecognizer ? "formrecognizer" : "documentintelligence")}/documentClassifiers?api-version={apiVersion}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);

            request.Content = new StringContent(string.Empty);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Check for null or empty JSON response
                if (string.IsNullOrEmpty(jsonResponse))
                {
                    Console.Error.WriteLine("The response from the API was null or empty.");
                    return null; // or throw a custom exception if preferred
                }
                // Deserialize JSON response to DocumentClassifierResponse
                var documentClassifierResponse = JsonConvert.DeserializeObject<DocumentClassifierResponse>(jsonResponse);
                return documentClassifierResponse;
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
    }


    public async Task<DocumentClassifierResult> ClassifyDocumentAsync(byte[] fileData, string modelId)
    {

        // Initialize the DocumentAnalysisClient
        var client = new DocumentIntelligenceClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

        // Determine if the file path is a URL or a local file path
        // If it's a local file, classify using the stream method
        using var stream = new MemoryStream(fileData);
        return await ClassifyDocumentFromStreamAsync(client, modelId, stream);
    }

    public async Task<DocumentClassifierResult> ClassifyDocumentAsync(string filePathOrURI, string modelId)
    {
        // Validate the file path
        if (string.IsNullOrEmpty(filePathOrURI))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePathOrURI));
        }

        var classifierId = modelId.Or(AppConfig.AzureDocumentIntelligenceModelId);

        // Initialize the DocumentAnalysisClient
        var client = new DocumentIntelligenceClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

        // Determine if the file path is a URL or a local file path
        if (Uri.IsWellFormedUriString(filePathOrURI, UriKind.Absolute))
        {
            // If it's a URL, classify using the URI method
            Uri fileUri = new Uri(filePathOrURI);
            return await ClassifyDocumentFromUriAsync(client, classifierId, fileUri);
        }
        else if (File.Exists(filePathOrURI))
        {
            // If it's a local file, classify using the stream method
            using var stream = new FileStream(filePathOrURI, FileMode.Open, FileAccess.Read);
            return await ClassifyDocumentFromStreamAsync(client, classifierId, stream);
        }
        else
        {
            throw new FileNotFoundException("The specified file was not found or the URI is invalid.", filePathOrURI);
        }
    }

    private async Task<DocumentClassifierResult> ClassifyDocumentFromUriAsync(DocumentIntelligenceClient client, string classifierId, Uri fileUri)
    {
        // Classify the document from the URI
        var operation = await client.ClassifyDocumentAsync(WaitUntil.Completed, new ClassifyDocumentOptions(classifierId, fileUri));
        return GenerateResult(operation.Value);
    }

    private async Task<DocumentClassifierResult> ClassifyDocumentFromStreamAsync(DocumentIntelligenceClient client, string classifierId, Stream stream)
    {
        // Classify the document from the file stream
        var operation = await client.ClassifyDocumentAsync(WaitUntil.Completed, new ClassifyDocumentOptions(classifierId, BinaryData.FromStream(stream)));
        return GenerateResult(operation.Value);
    }

    private DocumentClassifierResult GenerateResult(AnalyzeResult result)
    {
        // Use StringBuilder to build the output string
        var output = new StringBuilder();
        var response = new DocumentClassifierResult();

        if (result == null)
            throw new ArgumentNullException(nameof(result));

        if (result.Documents.HasAny())
        {
            response = new DocumentClassifierResult
            {
                DocumentType = result.Documents[0].DocumentType,
                Confidence = result.Documents[0].Confidence
            };
        }

        return response;
    }
}
