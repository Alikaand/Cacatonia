using System.Collections.Generic;
using UnityEngine;

// Простейший инвентарь: список ID подобранных предметов.
// Вешается на объект Player.
public class PlayerInventory : MonoBehaviour
{
    readonly List<string> items = new List<string>();

    public void AddItem(string id)
    {
        if (!string.IsNullOrEmpty(id) && !items.Contains(id))
        {
            items.Add(id);
            Debug.Log("Подобран предмет: " + id);
        }
    }

    public bool HasItem(string id)
    {
        return items.Contains(id);
    }

    public bool RemoveItem(string id)
    {
        return items.Remove(id);
    }
}
