using System.Collections.Generic;
using UnityEngine;

public class CustomerOrder : MonoBehaviour
{
    [SerializeField] private List<string> requiredProductIds = new List<string>();
    private HashSet<string> delivered = new HashSet<string>();

    public void SetOrder(List<string> ids)
    {
        requiredProductIds = new List<string>(ids);
        delivered.Clear();
    }

    public bool TryDeliver(string productId, out bool orderCompleted)
    {
        orderCompleted = false;

        if (!requiredProductIds.Contains(productId)) return false;
        if (delivered.Contains(productId)) return false;

        delivered.Add(productId);

        orderCompleted = delivered.Count >= requiredProductIds.Count;
        return true;
    }
}