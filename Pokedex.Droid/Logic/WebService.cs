using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Pokedex.Common.MachineLearning;

namespace Pokedex.Droid.Logic
{
    public class WebService
    {
        private static readonly string URL = Environment.GetEnvironmentVariable("WEB_API_URL");
        private static readonly string TOKEN = Environment.GetEnvironmentVariable("AUTH_TOKEN"); 

        private HttpClient _httpClient;

        public WebService()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(URL)
            };

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", TOKEN);
        }

        public async Task<OutputModel> ClassifyAsync(byte[] image, CancellationToken cancellation = default)
        {
            using ByteArrayContent content = new ByteArrayContent(image);

            return await PostAsync<OutputModel>("/classify", content, cancellation);
        }


        private async Task<T> PostAsync<T>(string requestUri, HttpContent content, CancellationToken cancellation = default)
        {
            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, content, cancellation);
            response.EnsureSuccessStatusCode();
            string message = await response.Content.ReadAsStringAsync();

            return DeserializeJson<T>(message);
        }

        private T DeserializeJson<T>(string json)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}