using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Pantallas (GameObjects raíz)")]
    public GameObject principal;
    public GameObject iniciarSesion;
    public GameObject registro;

    private void Start()
    {
        ShowPrincipal();
    }

    public void ShowPrincipal()
    {
        principal.SetActive(true);
        iniciarSesion.SetActive(false);
        registro.SetActive(false);
    }

    public void ShowLogin()
    {
        principal.SetActive(false);
        iniciarSesion.SetActive(true);
        registro.SetActive(false);
    }

    public void ShowRegistro()
    {
        principal.SetActive(false);
        iniciarSesion.SetActive(false);
        registro.SetActive(true);
    }
}
