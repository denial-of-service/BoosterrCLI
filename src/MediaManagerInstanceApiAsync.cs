using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace BoosterrCLI;

public record MediaManagerInstanceApiAsync
{
    private readonly MediaManagerInstance _mediaManagerInstance;
    private readonly HttpClient _client;
    private readonly HttpClient _clientAcceptsJson;
    // Cooldown between requests necessary to prevent overwhelming the Radarr/Sonarr instance with requests
    private readonly TimeSpan _minTimeBetweenRequests = TimeSpan.FromMilliseconds(333);
    private long _mostRecentRequestTimestamp;

    public MediaManagerInstanceApiAsync(MediaManagerInstance mediaManagerInstance)
    {
        _mediaManagerInstance = mediaManagerInstance;
        _client = new HttpClient();
        _clientAcceptsJson = new HttpClient();
        HttpClient[] clients = [_client, _clientAcceptsJson];
        foreach (HttpClient client in clients)
        {
            client.BaseAddress = new Uri($"{mediaManagerInstance.Url}/");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-Api-Key", mediaManagerInstance.ApiKey);
        }

        _clientAcceptsJson.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<bool> IsConnectableAsync()
    {
        await WaitOutCooldown();
        HttpResponseMessage response;
        try
        {
            response = await _client.GetAsync("api");
        }
        catch (HttpRequestException)
        {
            return false;
        }
        finally
        {
            UpdateMostRecentRequestTimestamp();
        }

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        string json = await response.Content.ReadAsStringAsync();
        ApiInfo receivedApiInfo = ApiInfo.Parse(json);
        ApiInfo expectedApiInfo = new ApiInfo("v3", []);
        return receivedApiInfo.Equals(expectedApiInfo);
    }

    public async Task<ImmutableArray<CustomFormat>> GetAllCustomFormatsAsync()
    {
        await WaitOutCooldown();
        HttpResponseMessage response = await _clientAcceptsJson.GetAsync("api/v3/customformat");
        UpdateMostRecentRequestTimestamp();
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"{_mediaManagerInstance.Name}: Failed to get all custom formats");
        }

        string json = await response.Content.ReadAsStringAsync();
        return CustomFormat.ParseAll(json);
    }

    public async Task AddCustomFormatAsync(CustomFormat customFormat)
    {
        StringContent content = new StringContent(customFormat.NormalizedJson, Encoding.UTF8, "application/json");
        await WaitOutCooldown();
        HttpResponseMessage response = await _client.PostAsync("api/v3/customformat", content);
        UpdateMostRecentRequestTimestamp();
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"{_mediaManagerInstance.Name}: Failed to add custom format: {customFormat.PrettyName}");
        }
    }

    public async Task UpdateCustomFormatAsync(CustomFormat customFormat)
    {
        StringContent content = new StringContent(customFormat.NormalizedJson, Encoding.UTF8, "application/json");
        await WaitOutCooldown();
        HttpResponseMessage response = await _client.PutAsync("api/v3/customformat", content);
        UpdateMostRecentRequestTimestamp();
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"{_mediaManagerInstance.Name}: Failed to update custom format: {customFormat.PrettyName}");
        }
    }

    public async Task DeleteCustomFormatAsync(int id)
    {
        await WaitOutCooldown();
        HttpResponseMessage response = await _client.DeleteAsync($"api/v3/customformat/{id}");
        UpdateMostRecentRequestTimestamp();
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"{_mediaManagerInstance.Name}: Failed to delete custom format with id: {id}");
        }
    }

    private Task WaitOutCooldown()
    {
        TimeSpan timeSinceLastRequest = Stopwatch.GetElapsedTime(_mostRecentRequestTimestamp);
        TimeSpan timeToWait = _minTimeBetweenRequests - timeSinceLastRequest;
        if (timeToWait > TimeSpan.Zero)
        {
            return Task.Delay(timeToWait);
        }

        return Task.CompletedTask;
    }

    private void UpdateMostRecentRequestTimestamp()
    {
        _mostRecentRequestTimestamp = Stopwatch.GetTimestamp();
    }
}