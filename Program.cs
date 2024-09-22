using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Azure;

// Add Azure OpenAI package (Add code here)
using Azure.AI.OpenAI;


namespace OpenAI_Chat
{
    class Program
    {
        static IConfiguration configuration;
        public static void Main()
        {
            try
            {
                Utils.InitLog();

                IConfiguration config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
                string oaiEndpoint = config["AzureOAIEndpoint"] ?? "";
                string oaiKey = config["AzureOAIKey"] ?? "";
                string oaiModelName = config["AzureOAIModelName"] ?? "";

                // Initialize the Azure OpenAI client (Add code here)
                // This line should look like:
                //      CORRECT_TYPE client = new CORRECT_TYPE( ... );
                OpenAIClient client = new OpenAIClient(new Uri(oaiEndpoint), new AzureKeyCredential(oaiKey));


                var functions = new Dictionary<int, Action<OpenAIClient, string>> {
                    { 1, function1 },
                    { 2, function2 },
                    { 3, function3 },
                    { 4, function4 }
                };

                while (true) {
                    Console.WriteLine("1: Validate PoC\n" +
                        "2: Company chatbot\n" +
                        "3: Developer tasks\n" +
                        "4: Use company data\n" +
                        "\'quit\' to exit the program\n");
                    string userInput = (Console.ReadLine() ?? "").Trim().ToLower();

                    if (userInput == "quit") {
                        break;
                    }

                    int inputKey = int.Parse(userInput);

                    if (functions.ContainsKey(inputKey)) {
                        functions[inputKey](client, oaiModelName);
                    }
                    else {
                        Console.WriteLine("Invalid input. Please enter number 1,2,3, or 4.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // Task 1: Validate PoC
        static void function1(OpenAIClient client, string oaiModelName) {
            var promptInput = Utils.GetPromptInput("Task 1: Validate PoC\n", "sample-text.txt");

            // Build completion options object (Add code here)
            // This line should look like:
            //      CORRECT_TYPE options = new CORRECT_TYPE() { ... };

            ChatCompletionsOptions options =  new ChatCompletionsOptions()
            {
                Messages = {
                    new ChatRequestUserMessage(promptInput)
                },
                MaxTokens = 1200,
                Temperature = 0.7f,
                DeploymentName = oaiModelName
            };

            Utils.WriteLog("API Parameters: ", options);

            // Send request to Azure OpenAI model (Add code here)
            // This line should look like:
            //      CORRECT_TYPE response = client.CORRECT_METHOD(...);
            ChatCompletions response = client.GetChatCompletions(options);

            Utils.WriteLog("Response:\n", response);
            Console.WriteLine("Response: " + response.Choices[0].Message.Content + "\n");
        }

        // Task 2: Company chatbot
        static void function2(OpenAIClient client, string oaiModelName) {

            var promptInput = Utils.GetPromptInput("Task 2: Company chatbot\n", "sample-text.txt");

            // Build completion options object (Add code here)
            // This line should look like:
            //      CORRECT_TYPE options = new CORRECT_TYPE() { ... };
            var systemMessage ="You are an AI assistant. Please respond in a casual tone and end every response with 'Hope that helps! Thanks for using Contoso, Ltd.'. Also respond in both english and spanish language";

            ChatCompletionsOptions options = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatRequestSystemMessage(systemMessage),
                    new ChatRequestUserMessage("Where can I find the company phone number?"),
                    new ChatRequestAssistantMessage("You can find it on the footer of every page on our website. Hope that helps! Thanks for using Contoso, Ltd."),
                    new ChatRequestUserMessage(promptInput)
                },
                MaxTokens = 1000,
                Temperature = 0.5f,
                DeploymentName = oaiModelName
            };

            Utils.WriteLog("API Parameters: ", options);

            // Send request to Azure OpenAI model (Add code here)
            // This line should look like:
            //      CORRECT_TYPE response = client.CORRECT_METHOD(...);
            ChatCompletions response = client.GetChatCompletions(options);

            Utils.WriteLog("Response:\n", response);
            Console.WriteLine("Response: " + response.Choices[0].Message.Content + "\n");

        }

        // Task 3: Developer tasks
        static async void function3(OpenAIClient client, string oaiModelName) {

            var promptInput = Utils.GetPromptInput("Task 3: Developer tasks\n", "sample-text.txt");

            // Build completion options object (Add code here)
            // This line should look like:
            //      CORRECT_TYPE options = new CORRECT_TYPE() { ... };
            var systemMessage ="You are an AI assistant.";
            var tasks = new[]
            {
                new { file = "legacyCode.py", task = "Add comments and generate documentation for the following Python code." },
                new { file = "fibonacci.py", task = "Generate five unit tests for the following Python function." }
            };

            foreach (var task in tasks) {
                var codeContent = await File.ReadAllTextAsync($@"C:\files\{task.file}");
                var promptContent = $"{task.task}\n\n```python\n{codeContent}\n```";
            ChatCompletionsOptions options = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatRequestSystemMessage(systemMessage),
                    new ChatRequestUserMessage(promptContent)
                },
                Temperature = 0.7f,
                MaxTokens = 1200,
                DeploymentName = oaiModelName
            };

            Utils.WriteLog("API Parameters: ", options);

            // Send request to Azure OpenAI model (Add code here)
            // This line should look like:
            //      CORRECT_TYPE response = client.CORRECT_METHOD(...);
            ChatCompletions response = client.GetChatCompletions(options);

            Utils.WriteLog("Response:\n", response);
            Console.WriteLine("Response: " + response.Choices[0].Message.Content + "\n");
            }
        }

        // Task 4: Use company data
        static void function4(OpenAIClient client, string oaiModelName) {

            string promptInput = Utils.GetPromptInput("Task 4: Use company data\n", "sample-text.txt");
            configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
            string azureSearchEndpoint = configuration["AzureSearchEndpoint"] ?? "";
            string azureSearchKey = configuration["AzureSearchKey"] ?? "";
            string azureSearchIndex = configuration["AzureSearchIndexName"] ?? "";
            AzureCognitiveSearchChatExtensionConfiguration ownDataConfig = new()
            {
                    SearchEndpoint = new Uri(azureSearchEndpoint),
                    Authentication = new OnYourDataApiKeyAuthenticationOptions(azureSearchKey),
                    IndexName = azureSearchIndex
            };
            // Build completion options object (Add code here)
            // This section should include something like:
            //      CORRECT_TYPE options = new CORRECT_TYPE() { ... };
            var systemMessage ="You are a helpful travel agent.";
            ChatCompletionsOptions options = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatRequestSystemMessage(systemMessage),
                    new ChatRequestUserMessage(promptInput)
                },
                MaxTokens = 600,
                Temperature = 0.9f,
                DeploymentName = oaiModelName,
                // Specify extension options
                AzureExtensionsOptions = new AzureChatExtensionsOptions()
                {
                    Extensions = {ownDataConfig}
                }
            };

            Utils.WriteLog("API Parameters: ", options);

            // Send request to Azure OpenAI model (Add code here)
            // This line should look like:
            //      CORRECT_TYPE response = client.CORRECT_METHOD(...);
            ChatCompletions response = client.GetChatCompletions(options);

            Utils.WriteLog("Response:\n", response);
            Console.WriteLine("Response: " + response.Choices[0].Message.Content + "\n");

        }
    }
}
