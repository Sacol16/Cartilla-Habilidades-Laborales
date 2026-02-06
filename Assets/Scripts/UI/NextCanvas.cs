using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextCanvas : MonoBehaviour
{
    public GameObject canva1;
    public GameObject canva2;

    [SerializeField] private GameObject subtitulo;   // panel/texto de subtítulos
    [SerializeField] private Button subtituloBtn;    // botón que activa/desactiva

    [Header("Colores del botón")]
    [SerializeField] private Color colorOn = Color.green;
    [SerializeField] private Color colorOff = Color.red;

    public void Subtitulos()
    {
        if (subtitulo == null || subtituloBtn == null) return;

        // Detectar estado actual
        bool activos = subtitulo.activeSelf;

        // Toggle
        subtitulo.SetActive(!activos);

        // Cambiar color del botón según estado final
        bool ahoraActivos = subtitulo.activeSelf;
        subtituloBtn.image.color = ahoraActivos ? colorOn : colorOff;
    }

    // Opcional: para que al iniciar el juego el botón ya refleje el estado real
    private void Start()
    {
        if (subtitulo != null && subtituloBtn != null)
            subtituloBtn.image.color = subtitulo.activeSelf ? colorOn : colorOff;
    }

    public void SwitchCanva()
    {
        canva2.SetActive(true);
        canva1.SetActive(false);
    }

    public void Modulo1()
    {
        SceneManager.LoadScene("Modulo 1");
    }
}
