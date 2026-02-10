using UnityEngine;

public class BotonGuardarDibujo : MonoBehaviour
{
    public GameObject canva1;
    public GameObject canva2;

    [Header("Refs")]
    [SerializeField] private DrawingSaver drawingSaver; // arrastra aquí el objeto que tiene DrawingSaver

    public void SaveSwitch()
    {
        if (drawingSaver == null)
        {
            Debug.LogError("[BotonGuardarDibujo] Falta asignar DrawingSaver.");
            return;
        }

        byte[] png = drawingSaver.CapturePngBytes();
        if (png == null || png.Length == 0)
        {
            Debug.LogError("[BotonGuardarDibujo] No se pudo capturar el PNG.");
            return;
        }

        // ? guardar para subirlo al final (un solo POST)
        if (Module1ActivityManager.Instance != null)
            Module1ActivityManager.Instance.SetActivity3Drawing(png);

        // UI flow
        canva2.SetActive(true);
        canva1.SetActive(false);
    }
}