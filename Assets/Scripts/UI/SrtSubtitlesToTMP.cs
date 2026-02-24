using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class SrtSubtitlesToTMP : MonoBehaviour
{
    [Header("References (ASIGNA ESTO EN EL INSPECTOR)")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextAsset srtFile;

    private class Cue { public double start; public double end; public string text; }
    private readonly List<Cue> cues = new();
    private int cueIndex = 0;

    private void OnEnable()
    {
        if (videoPlayer)
        {
            videoPlayer.started += OnVideoStarted;
            videoPlayer.loopPointReached += OnVideoEnded;
            videoPlayer.seekCompleted += OnVideoSeeked;
        }
    }

    private void OnDisable()
    {
        if (videoPlayer)
        {
            videoPlayer.started -= OnVideoStarted;
            videoPlayer.loopPointReached -= OnVideoEnded;
            videoPlayer.seekCompleted -= OnVideoSeeked;
        }
    }

    private void Start()
    {
        // Validaciones con logs (NO fallar en silencio)
        if (!videoPlayer) Debug.LogError("[SRT] Falta asignar VideoPlayer en el inspector.", this);
        if (!subtitleText) Debug.LogError("[SRT] Falta asignar TextMeshProUGUI en el inspector.", this);
        if (!srtFile) Debug.LogError("[SRT] Falta asignar el archivo SRT (TextAsset) en el inspector.", this);

        if (!videoPlayer || !subtitleText || !srtFile) return;

        ParseSrt(srtFile.text);
        Debug.Log($"[SRT] Cues cargados: {cues.Count}", this);
        subtitleText.text = "TEST SUB";

        cueIndex = 0;
    }

    private void Update()
    {
        if (!videoPlayer || !subtitleText || cues.Count == 0) return;

        double t = videoPlayer.time;

        // Si el video volvió hacia atrás (restart/seek), reiniciamos el índice
        if (cueIndex > 0 && t < cues[cueIndex - 1].start)
            cueIndex = 0;

        // Avanza el índice si ya pasamos el cue actual
        while (cueIndex < cues.Count && t > cues[cueIndex].end)
            cueIndex++;

        if (cueIndex >= cues.Count)
        {
            subtitleText.text = "";
            return;
        }

        var cue = cues[cueIndex];
        subtitleText.text = (t >= cue.start && t <= cue.end) ? cue.text : "";
    }

    private void OnVideoStarted(VideoPlayer vp)
    {
        cueIndex = 0;
        if (subtitleText) subtitleText.text = "";
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        cueIndex = 0;
        if (subtitleText) subtitleText.text = "";
    }

    private void OnVideoSeeked(VideoPlayer vp)
    {
        cueIndex = 0;
        if (subtitleText) subtitleText.text = "";
    }

    private void ParseSrt(string srt)
    {
        cues.Clear();
        cueIndex = 0;

        var blocks = Regex.Split(srt.Trim(), @"\r?\n\r?\n");
        foreach (var block in blocks)
        {
            var lines = block.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length < 3) continue;

            var timeLine = lines[1].Trim();
            var match = Regex.Match(timeLine, @"(.+?)\s*-->\s*(.+)");
            if (!match.Success) continue;

            double start = ParseTime(match.Groups[1].Value.Trim());
            double end = ParseTime(match.Groups[2].Value.Trim());
            string text = string.Join("\n", lines, 2, lines.Length - 2).Trim();

            cues.Add(new Cue { start = start, end = end, text = text });
        }

        cues.Sort((a, b) => a.start.CompareTo(b.start));
    }

    private double ParseTime(string s)
    {
        s = s.Replace(',', '.');
        if (TimeSpan.TryParseExact(s, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture, out var ts))
            return ts.TotalSeconds;

        TimeSpan.TryParse(s, out ts);
        return ts.TotalSeconds;
    }
}
