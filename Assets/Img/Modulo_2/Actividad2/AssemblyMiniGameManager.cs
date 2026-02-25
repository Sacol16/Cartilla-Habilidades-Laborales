// ===========================
// AssemblyMiniGameManager.cs
// - Termina el minijuego cuando las 3 estaciones están completas
// ===========================

using UnityEngine;

public class AssemblyMiniGameManager : MonoBehaviour
{
    [SerializeField] private AssemblyStation[] stations; // 3 estaciones
    [SerializeField] private GameObject endPanel;        // opcional

    private void Awake()
    {
        if (endPanel != null) endPanel.SetActive(false);

        // Auto-hook
        if (stations != null)
        {
            foreach (var st in stations)
            {
                if (st != null)
                    st.OnCompleted += HandleStationCompleted;
            }
        }
    }

    private void HandleStationCompleted(AssemblyStation station)
    {
        // ¿Ya están todas completas?
        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i] != null && !stations[i].IsComplete)
                return;
        }

        EndMiniGame();
    }

    private void EndMiniGame()
    {
        Debug.Log("[AssemblyMiniGame] Minijuego completado.");

        if (endPanel != null) endPanel.SetActive(true);

        // Si quieres desactivar interacción global, puedes desactivar la cinta:
        // (ej: ConveyorBeltUI.SetRunning(false))
    }
}