using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextCanvas : MonoBehaviour
{
    public GameObject canva1;
    public GameObject canva2;

    [SerializeField] private GameObject subtitulo;   // panel/texto de subt�tulos
    [SerializeField] private Button subtituloBtn;    // bot�n que activa/desactiva

    [Header("Colores del bot�n")]
    [SerializeField] private Color colorOn = Color.green;
    [SerializeField] private Color colorOff = Color.red;

    public void Subtitulos()
    {
        if (subtitulo == null || subtituloBtn == null) return;

        // Detectar estado actual
        bool activos = subtitulo.activeSelf;

        // Toggle
        subtitulo.SetActive(!activos);

        // Cambiar color del bot�n seg�n estado final
        bool ahoraActivos = subtitulo.activeSelf;
        subtituloBtn.image.color = ahoraActivos ? colorOn : colorOff;
    }

    // Opcional: para que al iniciar el juego el bot�n ya refleje el estado real
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

    public void Modulo2()
    {
        SceneManager.LoadScene("Modulo 2 Check");
    }

    public void HideCanva()
    {
        canva1.SetActive(false);
    }

    public void ShowCanva()
    {
        canva1.SetActive(true);
    }

    public void BacktoLobby()
    {
        SceneManager.LoadScene("Estudiante");
    }
}
