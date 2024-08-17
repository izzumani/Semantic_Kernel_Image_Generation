// See https://aka.ms/new-console-template for more information
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Plugins;

public class Program

{
    private static Kernel _kernel;
    private static SecretClient keyVaultClient;
   
    
    public async static Task Main(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

        string? appTenant = config["appTenant"];
        string? appId = config["appId"] ?? null;
        string? appPassword = config["appPassword"] ?? null;
        string? keyVaultName = config["KeyVault"] ?? null;

        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        ClientSecretCredential credential = new ClientSecretCredential(appTenant, appId, appPassword);
        keyVaultClient = new SecretClient(keyVaultUri, credential);
        string apiKey = keyVaultClient.GetSecret("OpenAIapiKey").Value.Value;
        string orgId = keyVaultClient.GetSecret("OpenAIorgId").Value.Value;

        var builder = Kernel.CreateBuilder();
        builder.Plugins.AddFromType<Dalle3SematicKernelPlugin>();
        builder.AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey, orgId);
        _kernel = builder.Build();
        string prompt = "A cat sitting on a couch in the style of Monet";
        //string? url = await _kernel.InvokeAsync<string>("Dalle3SematicKernelPlugin", "ImageFromPrompt", new() { { "prompt", prompt },{ "apiKey", apiKey } });
        //Console.WriteLine(url);
        string pluginPath = Path.Combine("plugins", "AnimalGuesser");
        KernelPlugin animalGuesser = _kernel.ImportPluginFromPromptDirectory(pluginPath);
        string clues = "It's a mammal, It's a pet. It meows. It purrs.";
        KernelFunction guessAnimal = animalGuesser["GuessAnimal"];
       KernelFunction generateImage = _kernel.Plugins["Dalle3SematicKernelPlugin"]["ImageFromPrompt"];
        KernelFunction pipeline = KernelFunctionCombinators.Pipe(new[] {guessAnimal,generateImage}, "pipeline");
        KernelArguments context = new() { { "input", clues }, { "prompt", prompt }, { "apiKey", apiKey } };
        Console.WriteLine(await pipeline.InvokeAsync(_kernel, context));

        Console.ReadLine();
    }

    
}
