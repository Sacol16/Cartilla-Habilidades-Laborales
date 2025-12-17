using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class RestClient
{
    public static async Task<string> SendJson(string url, string method, string jsonBody = null, string bearerToken = null)
    {
        using var req = new UnityWebRequest(url, method);
        req.downloadHandler = new DownloadHandlerBuffer();

        if (!string.IsNullOrEmpty(jsonBody))
        {
            var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.SetRequestHeader("Content-Type", "application/json");
        }

        if (!string.IsNullOrEmpty(bearerToken))
            req.SetRequestHeader("Authorization", $"Bearer {bearerToken}");

        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        // WebGL: req.result sirve bien en 2022.3
        if (req.result != UnityWebRequest.Result.Success)
            throw new Exception($"HTTP {req.responseCode}: {req.downloadHandler.text}");

        return req.downloadHandler.text;
    }
}
