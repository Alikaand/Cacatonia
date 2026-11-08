using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Универсальный предмет «нажми E — что-то случится». Без программирования:
// все действия настраиваются в Inspector, в списках событий (кнопка «+»).
// Примеры: телефон на посту, журнал медсестры, рубильник, диктофон.
//   On Interact      — что сделать сразу (включить звук, показать мальчика, текст...)
//   On After Delay   — что сделать через Delay Seconds секунд (спрятать мальчика...)
// Вешается на объект с коллайдером.
public class InteractEvent : MonoBehaviour, IInteractable
{
    [Tooltip("Подсказка на экране, например «Снять трубку»")]
    public string prompt = "Осмотреть";
    [Tooltip("Сработает только один раз, потом подсказка пропадёт")]
    public bool onlyOnce = true;

    [Header("Что произойдёт")]
    public UnityEvent onInteract;

    [Tooltip("Через сколько секунд после нажатия выполнить второй список")]
    public float delaySeconds = 3f;
    public UnityEvent onAfterDelay;

    bool used;

    public string GetPrompt()
    {
        // Пустая подсказка = объект «молчит», если уже использован
        if (used && onlyOnce) return "";
        return prompt;
    }

    public void Interact(PlayerInteraction player)
    {
        if (used && onlyOnce) return;
        used = true;
        onInteract.Invoke();
        if (onAfterDelay.GetPersistentEventCount() > 0) StartCoroutine(After());
    }

    IEnumerator After()
    {
        yield return new WaitForSeconds(delaySeconds);
        onAfterDelay.Invoke();
    }

    // Чтобы другой скрипт или событие могли «перезарядить» предмет
    public void ResetUsed()
    {
        used = false;
    }
}
