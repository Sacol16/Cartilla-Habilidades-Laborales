// ===========================
// FruitSpawner2D.cs
// - Spawnea aleatoriamente entre 12 prefabs (8 Good + 4 Bad)
// - Controla cadencia, posiciones X, y dificultad
// - Usa spawnContainer para poder limpiar sin tags extra (Good/Bad se mantienen)
// - Incluye ResetSpawner() para reinicio sin recargar escena
// ===========================

using UnityEngine;

public class FruitSpawner2D : MonoBehaviour
{
    [Header("Prefabs (12 total)")]
    [Tooltip("Arrastra aquí tus 12 prefabs (8 frutas/verduras + 4 basuras).")]
    [SerializeField] private GameObject[] prefabs; // 12

    [Header("Spawn Container (para limpiar al reiniciar)")]
    [Tooltip("Todos los objetos spawneados serán hijos de este Transform. Crea un GO vacío (ej: SpawnedItems) y asígnalo aquí.")]
    [SerializeField] private Transform spawnContainer;

    [Header("Spawn Area")]
    [Tooltip("Mínimo y máximo X donde pueden aparecer.")]
    [SerializeField] private float minX = -7f;
    [SerializeField] private float maxX = 7f;

    [Tooltip("Si está activado, usa spawnY (fijo). Si no, usa transform.position.y del spawner.")]
    [SerializeField] private bool useFixedSpawnY = true;
    [SerializeField] private float spawnY = 5.5f;

    [Header("Spawn Timing")]
    [Tooltip("Intervalo inicial entre spawns (segundos).")]
    [SerializeField] private float startInterval = 0.75f;

    [Tooltip("Intervalo mínimo al final (segundos).")]
    [SerializeField] private float minInterval = 0.30f;

    [Tooltip("Duración total de la partida para escalar dificultad (normalmente 30).")]
    [SerializeField] private float gameDuration = 30f;

    [Header("Fall Speed Settings (passed to FruitFall2D)")]
    [Tooltip("Velocidad inicial de caída (unidades/seg).")]
    [SerializeField] private float startFallSpeed = 3.0f;

    [Tooltip("Velocidad máxima de caída (unidades/seg) al final.")]
    [SerializeField] private float maxFallSpeed = 7.0f;

    [Header("Despawn")]
    [Tooltip("Y por debajo de esto se destruyen los objetos.")]
    [SerializeField] private float destroyBelowY = -6.5f;

    [Header("Miss Penalty (opcional)")]
    [Tooltip("Si se pierde (no se atrapa) una fruta buena, ¿penaliza?")]
    [SerializeField] private bool penalizeMissGood = false;

    [Tooltip("Referencia al manager para penalizar misses si activas penalizeMissGood.")]
    [SerializeField] private MiniGameManager1 manager;

    private float elapsed;
    private float nextSpawnIn;
    private bool running = true;

    private void Awake()
    {
        nextSpawnIn = 0f;
    }

    private void Update()
    {
        if (!running) return;

        elapsed += Time.deltaTime;

        nextSpawnIn -= Time.deltaTime;
        if (nextSpawnIn <= 0f)
        {
            SpawnOne();
            nextSpawnIn = GetCurrentInterval();
        }
    }

    private void SpawnOne()
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("[FruitSpawner2D] No hay prefabs asignados.");
            return;
        }

        int idx = Random.Range(0, prefabs.Length);
        GameObject prefab = prefabs[idx];
        if (prefab == null) return;

        float x = Random.Range(minX, maxX);
        float y = useFixedSpawnY ? spawnY : transform.position.y;

        Vector3 pos = new Vector3(x, y, 0f);

        // IMPORTANTE: parent = spawnContainer (si existe) para poder limpiar fácil
        Transform parent = spawnContainer != null ? spawnContainer : null;
        GameObject go = Instantiate(prefab, pos, Quaternion.identity, parent);

        // Configurar caída
        FruitFall2D fall = go.GetComponent<FruitFall2D>();
        if (fall == null) fall = go.AddComponent<FruitFall2D>();

        fall.SetConfig(
            spawner: this,
            fallSpeed: GetCurrentFallSpeed(),
            destroyBelowY: destroyBelowY
        );
    }

    private float GetDifficulty01()
    {
        if (gameDuration <= 0f) return 1f;
        return Mathf.Clamp01(elapsed / gameDuration);
    }

    private float GetCurrentInterval()
    {
        float t = GetDifficulty01();
        return Mathf.Lerp(startInterval, minInterval, t);
    }

    private float GetCurrentFallSpeed()
    {
        float t = GetDifficulty01();
        return Mathf.Lerp(startFallSpeed, maxFallSpeed, t);
    }

    public void NotifyMissed(GameObject missedObj)
    {
        if (!penalizeMissGood) return;
        if (manager == null) return;

        // Si es Good y se salió de la pantalla, lo contamos como error
        if (missedObj != null && missedObj.CompareTag("Good"))
        {
            manager.RegisterWrong();
        }
    }

    public void SetRunning(bool value) => running = value;

    public void ResetSpawner()
    {
        elapsed = 0f;
        nextSpawnIn = 0f;
    }

    // ===== NUEVO: limpiar spawneados sin tags extra =====
    public void ClearSpawned()
    {
        if (spawnContainer == null) return;

        for (int i = spawnContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(spawnContainer.GetChild(i).gameObject);
        }
    }
}