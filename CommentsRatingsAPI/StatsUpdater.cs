using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class StatsUpdater
{
    private readonly HttpClient _httpClient;

    public StatsUpdater(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task UpdateStatsAsync(string endpoint)
    {
        var update = new { Endpoint = endpoint };
        var content = new StringContent(JsonSerializer.Serialize(update), Encoding.UTF8, "application/json");
        await _httpClient.PostAsync("/statsbrdarovski/PostUpdate", content);
    }
}