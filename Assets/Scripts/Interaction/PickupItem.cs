using UnityEngine;

// Предмет, который можно подобрать на E: ключ, записка, фонарик, батарейка.
// Вешается на модель предмета (у неё должен быть Collider).
public class PickupItem : MonoBehaviour, IInteractable
{
    public enum ItemType { Key, Flashlight, Battery, Other }

    public ItemType type = ItemType.Key;
    [Tooltip("Уникальный ID, например \"key_basement\". Для ключа должен совпадать с Door.requiredKeyId")]
    public string itemId = "key_1";
    public string displayName = "ключ";
    public float batteryAmount = 30f;

    public AudioClip pickupSound;

    public string GetPrompt()
    {
        return "Подобрать " + displayName;
    }

    public void Interact(PlayerInteraction player)
    {
        switch (type)
        {
            case ItemType.Flashlight:
                Flashlight fl = player.GetComponentInChildren<Flashlight>(true);
                if (fl != null) fl.hasFlashlight = true;
                break;

            case ItemType.Battery:
                Flashlight f = player.GetComponentInChildren<Flashlight>(true);
                if (f != null) f.AddBattery(batteryAmount);
                break;

            default:
                if (player.inventory != null) player.inventory.AddItem(itemId);
                break;
        }

        if (pickupSound != null) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        Destroy(gameObject); // предмет исчезает со сцены
    }
}
