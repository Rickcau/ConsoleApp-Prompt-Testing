using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using System.Linq;
using ConsoleApp_Prompt_Testing.Models;
using System.Net.Http.Json;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string outputDirectory = Path.Combine(baseDirectory, "OutputLogs");
    private static readonly string outputFile = Path.Combine(outputDirectory, $"chat_responses_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
    private static readonly string filesDirectory = Path.Combine(baseDirectory, "files");
    private static readonly string groundTruthFile = Path.Combine(filesDirectory, "ground_truth_queries.json");
    private static readonly string apiUrl = "https://localhost:7097/Chat";
    private static readonly string apiKey = "1234";

    //public class ChatRequest
    //{
    //    public string? SessionId { get; set; }
    //    public string? UserId { get; set; }
    //    public string? Prompt { get; set; }
    //}

    //public class ChatProviderResponse
    //{
    //    [JsonPropertyName("chatResponse")]
    //    public string? ChatResponse { get; set; }
    //    public string? Query { get; set; }
    //}

    static async Task Main(string[] args)
    {
        // Create output directory if it doesn't exist
        Directory.CreateDirectory(outputDirectory);

        // Ensure files directory exists
        if (!Directory.Exists(filesDirectory))
        {
            throw new DirectoryNotFoundException($"Files directory not found at: {filesDirectory}");
        }

        // Ensure ground truth file exists
        if (!File.Exists(groundTruthFile))
        {
            throw new FileNotFoundException($"Ground truth queries file not found at: {groundTruthFile}");
        }

        Console.WriteLine($"Output file will be created at: {outputFile}");
        Console.WriteLine($"Reading ground truth queries from: {groundTruthFile}");

        // Configure HttpClient with timeout
        client.BaseAddress = new Uri("https://localhost:7097/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("api-key", apiKey);

        try
        {
            // Read and parse the JSON file using JObject
            string jsonContent = await File.ReadAllTextAsync(groundTruthFile);
            var jsonObject = JObject.Parse(jsonContent);

            // Extract user questions into a list
            var prompts = jsonObject["user_question_answer"]?
                .Select(item => item["user_question"]?.ToString())
                .Where(question => !string.IsNullOrEmpty(question))
                .ToList();

            if (prompts == null || !prompts.Any())
            {
                throw new Exception("No valid user questions found in the JSON file.");
            }

            // Write start time to file
            await File.WriteAllTextAsync(outputFile, $"Process started at: {DateTime.Now}\n\n");

            // Process all prompts
            Console.WriteLine($"Processing {prompts.Count} prompts...");
            await ProcessPromptsSequentially(prompts);

            // Write completion time to file
            await File.AppendAllTextAsync(outputFile, $"\nProcess completed at: {DateTime.Now}");

            Console.WriteLine($"\nAll prompts have been processed. Check the output file at:\n{outputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            await File.AppendAllTextAsync(outputFile, $"\nError occurred at {DateTime.Now}: {ex.Message}");
            throw;
        }
    }

    static async Task ProcessPromptsSequentially(List<string> prompts)
    {
        for (int i = 0; i < prompts.Count; i++)
        {
            Console.WriteLine($"Processing prompt {i + 1} of {prompts.Count}...");

            var request = new ChatProviderRequest
            {
                SessionId = "12345",
                UserId = "test",
                Prompt = prompts[i],
                RunValidation = true,
            };

            try
            {
                // Send request and wait for the complete response
                var response = await SendChatRequest(request);
                await LogResponseToFile(i,prompts[i], response);
                Console.WriteLine($"Prompt {i + 1} processed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing prompt {i + 1}: {ex.Message}");
                await LogResponseToFile(i, prompts[i], new ChatProviderValidationResponse(), ex.Message);
            }
        }
    }

    static async Task<ChatProviderValidationResponse> SendChatRequest(ChatProviderRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync("Chat", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var chatValidationResponse = JsonSerializer.Deserialize<ChatProviderValidationResponse>(responseContent);

            if (chatValidationResponse?.ChatResponse == null)
            {
                throw new Exception("Received empty response from server");
            }

            return chatValidationResponse;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"HTTP Request failed: {ex.Message}\nStatus Code: {ex.StatusCode}", ex);
        }
    }

    static async Task LogResponseToFile(int count, string prompt, ChatProviderValidationResponse response, string? errormsg = null)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logEntry = $"{count}.\n[{timestamp}]\nPrompt: {prompt}";

        if (!string.IsNullOrEmpty(errormsg))
        {
            logEntry += $"\nError: {errormsg}";
        }
        else if (response?.ValidationResponse != null)
        {
            var jsonObject = JObject.Parse(response.ValidationResponse);
            var questionAnswer = jsonObject["user_question_answer"]?.FirstOrDefault();
            string validationResponse = "";

            if (questionAnswer != null)
            {
                string? userQuestion = questionAnswer["user_question"]?.ToString();
                string? generatedSql = questionAnswer["generated_sql"]?.ToString();
                string? sqlResults = questionAnswer["sql_results"]?.ToString();
                string? thoughts = questionAnswer["thoughts"]?.ToString();
                int rating = questionAnswer["rating"]?.Value<int>() ?? 0;

                validationResponse = $"Question: {userQuestion}\n" +
                                   $"SQL: {generatedSql}\n\n" +
                                   $"Results: {sqlResults}\n\n" +
                                   $"Thoughts: {thoughts}\n\n" +
                                   $"Rating: {rating}\n\n";
            }

            logEntry += $"\nResponse: {response.ChatResponse}\n\nQuery: {response.Query}\n\n# Validation Response #\n{validationResponse}";
        }

        await File.AppendAllTextAsync(outputFile, logEntry);
    }
}