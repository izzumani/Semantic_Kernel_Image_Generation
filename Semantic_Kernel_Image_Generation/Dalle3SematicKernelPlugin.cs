using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Plugins;

    public class Dalle3SematicKernelPlugin
    {
       
       
        [KernelFunction, Description("Generate an image from a Promp")]
        public async Task<string> ImageFromPrompt([Description("Prompt describing the image you want to generate")] string prompt, string apiKey)
        {
            try
            {
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri("https://api.openai.com/v1/")
                };

                client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var obj = new
                {
                    model = "dall-e-3",
                    prompt = prompt,
                    n = 1,
                    size = "1024x1024"
                };
                var content = new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("images/generations", content);

                if (!response.IsSuccessStatusCode)
                {
                    return $"Error: {response.StatusCode}";
                }
                string jsonString = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(jsonString);
                JsonElement root = doc.RootElement;
                return root.GetProperty("data")[0]!.GetProperty("url")!.GetString()!;


            }
            catch (Exception)
            {

                throw;
            }
        }
    }

