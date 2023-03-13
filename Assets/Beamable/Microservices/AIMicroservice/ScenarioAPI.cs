using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Beamable.Microservices
{
    public static class ScenarioAPI
    {
        private static readonly HttpClient HttpClient = new();
        private static readonly ScenarioModel ScenarioModel = ScenarioModel.WEAPON;
        static ScenarioAPI()
        {
            HttpClient.BaseAddress = new Uri("https://api.cloud.scenario.gg/v1/");
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Configuration.ScenarioApiKey);
        }

        public static async Task<GenerateImageResponse> GenerateImage(GenerateImageRequest request)
        {
            var requestJson = new StringContent(
                JsonConvert.SerializeObject( new 
                {
                    parameters = new
                    {
                        request.type,
                        request.numSamples,
                        request.prompt
                    }
                }),
                Encoding.UTF8,
                "application/json");
            using var response = await HttpClient.PostAsync($"models/{ScenarioModel}/inferences", requestJson);
            var stringContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GenerateImageResponse>(stringContent);
        }
        
        public static async Task<FetchImageResponse> FetchImage(FetchImageRequest request)
        {
            using var response = await HttpClient.GetAsync($"models/{ScenarioModel}/inferences/{request.inferenceId}");
            var stringContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FetchImageResponse>(stringContent);
        }
    }

    public record GenerateImageRequest
    {
        public string type => "txt2img";
        public string prompt { get; set; }
        public int numSamples { get; set; } = 1;
    }
    
    public record GenerateImageResponse
    {
        public GenerateImageInference inference { get; set; }
    }
    
    public record GenerateImageInference
    {
        public string id { get; set; }
        public string status { get; set; }
    }
    
    public record FetchImageRequest
    {
        public string inferenceId { get; set; }
    }
    
    public record FetchImageResponse
    {
        public ImageInference inference { get; set; }
    }
    
    public record ImageInference
    {
        public string id { get; set; }
        public string status { get; set; }
        public List<ImageData> images { get; set; }        
    }
    
    public record ImageData
    {
        public string id { get; set; }
        public string url { get; set; }
    }

    public class ScenarioModel
    {
        public static ScenarioModel GEMSTONE => new("W0Fy6cQbTsa4RFFxiGj_sA");
        public static ScenarioModel STICKER => new("Ts8DY2QRTXSsdRbE6ow6Cw");
        public static ScenarioModel WEAPON => new("O7WOnYVUTJ6ECUY63ayUqg");
        public static ScenarioModel ROBOT => new("eUpJ0jKdRwmDZgFuTDaPtw");
        public static ScenarioModel POTION => new("potions");
        public static ScenarioModel BADGE => new("badges");

        public ScenarioModel(string name)
        {
            Name = name;
        }
        public string Name { get; }
        public override string ToString() => Name;
    }
}