// ===========================
// FruitFall2D.cs
// - Hace caer el objeto a velocidad constante
// - Lo destruye al pasar cierto Y
// - Notifica al spawner si se perdió (miss) para penalizar frutas buenas (opcional)
// ===========================

using UnityEngine;

public class FruitFall2D : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 3.0f;
    [SerializeField] private float destroyBelowY = -6.5f;

    private FruitSpawner2D spawner;
    private bool notifiedMiss = false;

    public void SetConfig(FruitSpawner2D spawner, float fallSpeed, float destroyBelowY)
    {
        this.spawner = spawner;
        this.fallSpeed = fallSpeed;
        this.destroyBelowY = destroyBelowY;
    }

    private void Update()
    {
        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

        if (transform.position.y <= destroyBelowY)
        {
            if (!notifiedMiss)
            {
                notifiedMiss = true;
                if (spawner != null)
                    spawner.NotifyMissed(gameObject);
            }

            Destroy(gameObject);
        }
    }
}