using UnityEngine;

public class DrawingSaver : MonoBehaviour
{
    [SerializeField] private Camera captureCam;
    [SerializeField] private RenderTexture renderTexture;

    public byte[] CapturePngBytes()
    {
        if (captureCam == null || renderTexture == null)
        {
            Debug.LogError("[DrawingSaver] Falta captureCam o renderTexture.");
            return null;
        }

        var prev = RenderTexture.active;
        RenderTexture.active = renderTexture;

        captureCam.targetTexture = renderTexture;
        captureCam.Render();

        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        captureCam.targetTexture = null;
        RenderTexture.active = prev;

        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        return bytes;
    }
}