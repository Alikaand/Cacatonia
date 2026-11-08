using UnityEngine;
using UnityEngine.Events;

// Записка, журнал, документ, план эвакуации: нажал E — текст (или картинка) на экране.
// Вешается на модель записки (у неё должен быть Collider).
public class NoteItem : MonoBehaviour, IInteractable
{
    public string prompt = "Прочитать";
    public string title = "Журнал дежурной медсестры";
    [TextArea(4, 12)]
    public string text = "03:13 — ушла на обход 2 этажа.";
    [Tooltip("Картинка вместо или вместе с текстом (необязательно)")]
    public Sprite image;

    [Tooltip("Что произойдёт после первого прочтения (новая задача, звук и т. д.)")]
    public UnityEvent onFirstRead;

    bool read;

    public string GetPrompt()
    {
        return prompt;
    }

    public void Interact(PlayerInteraction player)
    {
        if (NoteUI.Instance == null)
        {
            Debug.LogWarning("На сцене нет NoteUI — записку некуда показать");
            return;
        }
        if (NoteUI.Instance.IsOpen) return;

        NoteUI.Instance.Show(title, text, image);
        if (!read)
        {
            read = true;
            onFirstRead.Invoke();
        }
    }
}
