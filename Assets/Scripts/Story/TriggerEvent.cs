using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Невидимая зона: игрок вошёл — что-то случилось. Без программирования,
// действия настраиваются в Inspector (кнопка «+» в списках событий).
// Примеры: мальчик появляется за дверями, гаснет свет, звучит шорох, новая задача.
// Как поставить: Create Empty → Box Collider (Is Trigger) → этот скрипт.
public class TriggerEvent : MonoBehaviour
{
    [Tooltip("Сработает только один раз за игру")]
    public bool onlyOnce = true;

    [Header("Что произойдёт")]
    public UnityEvent onEnter;

    public float delaySeconds = 2f;
    [Tooltip("Через Delay Seconds после входа")]
    public UnityEvent onAfterDelay;

    [Tooltip("Когда игрок вышел из зоны (необязательно)")]
    public UnityEvent onExit;

    bool used;

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (used && onlyOnce) return;
        used = true;
        onEnter.Invoke();
        if (onAfterDelay.GetPersistentEventCount() > 0) StartCoroutine(After());
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) onExit.Invoke();
    }

    IEnumerator After()
    {
        yield return new WaitForSeconds(delaySeconds);
        onAfterDelay.Invoke();
    }
}
