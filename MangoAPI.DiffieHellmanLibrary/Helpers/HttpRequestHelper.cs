﻿using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;

namespace MangoAPI.DiffieHellmanLibrary.Helpers;

public static class HttpRequestHelper
{
    public static async Task<string> PostWithBodyAsync(HttpClient client, string route, object body)
    {
        var json = JsonConvert.SerializeObject(body);
        var uri = new Uri(route, UriKind.Absolute);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(uri, data);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }
        
    public static async Task<string> PostWithoutBodyAsync(HttpClient client, string route)
    {
        var uri = new Uri(route);
        var response = await client.PostAsync(uri, null!);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    public static async Task<string> PutWithBodyAsync(HttpClient client, string route, object body)
    {
        var json = JsonConvert.SerializeObject(body);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var uri = new Uri(route);
        var response = await client.PutAsync(uri, data);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    public static async Task<string> DeleteWithoutBodyAsync(HttpClient client, string route)
    {
        var uri = new Uri(route);
        var response = await client.DeleteAsync(uri);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }
        
    public static async Task<string> DeleteWithBodyAsync(HttpClient client, string route, object body)
    {
        var json = JsonContent.Create(body);

        var request = new HttpRequestMessage
        {
            Content = json,
            Method = HttpMethod.Delete,
            RequestUri = new Uri(route)
        };

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    public static async Task<string> GetAsync(HttpClient client, string route)
    {
        var uri = new Uri(route);
        var response = await client.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }
}