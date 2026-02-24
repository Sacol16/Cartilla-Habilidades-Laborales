using System;
using UnityEngine;
using UnityEngine.UI;

public class Module1Activity3Presenter : MonoBehaviour
{
    [Header("Target UI")]
    [Tooltip("RawImage donde se mostrará el dibujo recuperado (PNG).")]
    public RawImage previewImage;

    [Tooltip("Opcional: GameObject que se muestra cuando NO hay imagen (placeholder/texto).")]
    public GameObject emptyState;

    [Header("Debug")]
    [Tooltip("Si está activo, imprime logs de tamaño y validaciones (no imprime el base64).")]
    public bool debugLogs = true;

    /// <summary>
    /// Aplica el PNG en base64 (SIN 'data:image/png;base64,').
    /// </summary>
    public void Apply(string pngBase64)
    {
        if (string.IsNullOrEmpty(pngBase64))
        {
            SetEmpty(true);
            if (debugLogs) Debug.Log("[Activity3Presenter] pngBase64 vacío -> empty state.");
            return;
        }

        // Algunos sistemas guardan con prefijo "data:image/png;base64,"
        pngBase64 = StripDataUrlPrefixIfNeeded(pngBase64);

        if (debugLogs) Debug.Log($"[Activity3Presenter] pngBase64 length: {pngBase64.Length}");

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(pngBase64);
        }
        catch (Exception ex)
        {
            SetEmpty(true);
            Debug.LogWarning("[Activity3Presenter] Base64 inválido: " + ex.Message);
            return;
        }

        if (bytes == null || bytes.Length == 0)
        {
            SetEmpty(true);
            Debug.LogWarning("[Activity3Presenter] Bytes vacíos tras decodificar base64.");
            return;
        }

        // Crear textura y cargar PNG
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        bool ok = tex.LoadImage(bytes, markNonReadable: false);

        if (!ok)
        {
            SetEmpty(true);
            Debug.LogWarning("[Activity3Presenter] Texture2D.LoadImage falló (PNG corrupto o no es PNG).");
            Destroy(tex);
            return;
        }

        // Aplicar en UI
        if (previewImage != null)
        {
            previewImage.texture = tex;
            previewImage.enabled = true;
        }

        SetEmpty(false);

        if (debugLogs) Debug.Log($"[Activity3Presenter] PNG aplicado. Texture: {tex.width}x{tex.height} | bytes={bytes.Length}");
    }

    /// <summary>
    /// Extrae el PNG actual del RawImage y lo retorna como base64 (PNG) si existe.
    /// </summary>
    public string Extract()
    {
        if (previewImage == null || previewImage.texture == null)
            return "";

        var tex = previewImage.texture as Texture2D;
        if (tex == null)
        {
            // Si es RenderTexture u otro tipo, aquí habría que convertirlo (según tu implementación de dibujo)
            Debug.LogWarning("[Activity3Presenter] previewImage.texture no es Texture2D. No se puede extraer PNG directo.");
            return "";
        }

        try
        {
            byte[] png = tex.EncodeToPNG();
            return Convert.ToBase64String(png);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Activity3Presenter] No se pudo extraer PNG: " + ex.Message);
            return "";
        }
    }

    /// <summary>
    /// Limpia la vista previa.
    /// </summary>
    public void Clear(bool destroyTexture = true)
    {
        if (previewImage == null) return;

        if (destroyTexture && previewImage.texture != null)
        {
            // Importante: si esa textura la usa otro sistema, no la destruyas
            Destroy(previewImage.texture);
        }

        previewImage.texture = null;
        previewImage.enabled = false;

        SetEmpty(true);
    }

    private void SetEmpty(bool isEmpty)
    {
        if (emptyState != null)
            emptyState.SetActive(isEmpty);

        if (previewImage != null)
            previewImage.enabled = !isEmpty && previewImage.texture != null;
    }

    private string StripDataUrlPrefixIfNeeded(string b64)
    {
        // Ej: "data:image/png;base64,AAAA..."
        int comma = b64.IndexOf(',');
        if (b64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0)
            return b64.Substring(comma + 1);

        return b64;
    }
}