using UnityEngine;

// Невидимая зона: когда игрок в неё заходит, с фонариком что-то случается.
// Как поставить: Create Empty → Box Collider (галочка Is Trigger) → этот скрипт.
// Растяни Box Collider так, чтобы он перекрыл коридор или дверной проём.
public class FlashlightEventZone : MonoBehaviour
{
    public enum Effect
    {
        Die,      // мигает и садится насовсем (нужна новая батарейка)
        Break,    // мигает и гаснет на время, потом снова горит
        DrainTo   // заряд падает до значения ниже, свет тусклый
    }

    public Effect effect = Effect.Break;

    [Tooltip("Для Break: на сколько секунд фонарик гаснет")]
    public float breakSeconds = 5f;

    [Tooltip("Для DrainTo: до какого заряда упадёт (из 100)")]
    public float drainToValue = 10f;

    [Tooltip("Срабатывает только один раз за игру")]
    public bool onlyOnce = true;

    [Tooltip("Сообщение на экране (необязательно), например «Что-то не так с фонарём...»")]
    public string message = "";

    bool used;

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (used && onlyOnce) return;
        if (!other.CompareTag("Player")) return;

        Flashlight fl = other.GetComponentInChildren<Flashlight>(true);
        if (fl == null || !fl.hasFlashlight) return; // фонарика ещё нет — нечему гаснуть

        switch (effect)
        {
            case Effect.Die: fl.Die(); break;
            case Effect.Break: fl.Break(breakSeconds); break;
            case Effect.DrainTo: fl.DrainTo(drainToValue); break;
        }

        if (!string.IsNullOrEmpty(message) && GameManager.Instance != null)
            GameManager.Instance.ShowMessage(message);

        used = true;
    }
}
