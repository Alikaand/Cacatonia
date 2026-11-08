using UnityEngine;
using TMPro;

// Пускает невидимый луч из центра камеры. Если луч попал в объект
// с IInteractable — показывает подсказку и по нажатию E вызывает Interact().
// Вешается на объект Player.
public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 2.5f;
    public KeyCode interactKey = KeyCode.E;

    [Tooltip("Текст на Canvas (TextMeshPro) для подсказки. Можно оставить пустым.")]
    public TMP_Text promptText;

    [Tooltip("Инвентарь игрока (необязательно)")]
    public PlayerInventory inventory;

    IInteractable current;

    void Update()
    {
        current = null;

        // Пока открыта записка (или её только что закрыли этой же кнопкой) — ничего не трогаем
        if (NoteUI.Instance != null && (NoteUI.Instance.IsOpen || NoteUI.ClosedFrame == Time.frameCount))
        {
            if (promptText != null) promptText.text = "";
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            // Ищем скрипт на объекте или на его родителях (дверь может состоять из нескольких частей)
            current = hit.collider.GetComponentInParent<IInteractable>();
        }

        // Пустая подсказка = с объектом уже нечего делать (например, телефон уже сняли)
        string prompt = current != null ? current.GetPrompt() : "";
        if (string.IsNullOrEmpty(prompt)) current = null;

        if (promptText != null)
        {
            promptText.text = current != null ? $"[{interactKey}] {prompt}" : "";
        }

        if (current != null && Input.GetKeyDown(interactKey))
        {
            current.Interact(this);
        }
    }

    // Нарисовать луч в окне Scene — удобно для отладки
    void OnDrawGizmos()
    {
        if (playerCamera == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance);
    }
}
