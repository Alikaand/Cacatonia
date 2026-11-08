using System.Collections;
using UnityEngine;

// Фонарик в руках игрока. Включается клавишей F.
// Создай Spot Light как ДОЧЕРНИЙ объект камеры, повесь на него этот скрипт.
// Пока игрок не подобрал фонарик (hasFlashlight = false), он не работает.
//
// Другие скрипты могут «посадить» фонарик в нужный момент:
//   flashlight.Die();          — мигает и гаснет, заряд 0 (нужна батарейка)
//   flashlight.Break(5f);      — мигает и гаснет на 5 секунд, потом снова работает
//   flashlight.DrainTo(10f);   — заряд падает до 10 (свет становится тусклым)
// Проще всего — через зону FlashlightEventZone.
[RequireComponent(typeof(Light))]
public class Flashlight : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.F;
    public bool hasFlashlight = false;

    [Header("Батарейка (можно выключить)")]
    public bool useBattery = true;
    public float maxBattery = 100f;
    public float drainPerSecond = 1f;
    public float battery = 100f;

    [Header("Мигание перед тем, как погаснуть")]
    public float flickerTime = 1.2f;

    public AudioSource audioSource;
    public AudioClip clickSound;
    [Tooltip("Звук, когда фонарик сам гаснет (треск, щелчок)")]
    public AudioClip dieSound;

    Light lamp;
    float baseIntensity;
    bool busy; // сейчас идёт мигание или поломка — кнопка F не работает

    void Start()
    {
        lamp = GetComponent<Light>();
        baseIntensity = lamp.intensity;
        lamp.enabled = false;
    }

    void Update()
    {
        if (!hasFlashlight || busy) return;

        if (Input.GetKeyDown(toggleKey) && battery > 0f)
        {
            lamp.enabled = !lamp.enabled;
            Play(clickSound);
        }

        if (useBattery && lamp.enabled)
        {
            battery -= drainPerSecond * Time.deltaTime;
            UpdateBrightness();
            if (battery <= 0f)
            {
                battery = 0f;
                lamp.enabled = false;
            }
        }
    }

    void UpdateBrightness()
    {
        // Когда заряд меньше 20% — свет тускнеет
        lamp.intensity = baseIntensity * Mathf.Clamp01(battery / (maxBattery * 0.2f));
    }

    public void AddBattery(float amount)
    {
        battery = Mathf.Min(maxBattery, battery + amount);
        UpdateBrightness();
    }

    // Фонарик садится полностью: мигает и гаснет, заряд 0. Включить можно только после новой батарейки.
    public void Die()
    {
        if (!busy) StartCoroutine(DieRoutine(-1f));
    }

    // Фонарик ломается на время: мигает, гаснет, через seconds секунд снова работает.
    public void Break(float seconds)
    {
        if (!busy) StartCoroutine(DieRoutine(seconds));
    }

    // Заряд резко падает до указанного значения (например, 10 — фонарь почти сел и тусклый).
    public void DrainTo(float value)
    {
        battery = Mathf.Clamp(value, 0f, battery);
        UpdateBrightness();
        if (battery <= 0f) lamp.enabled = false;
    }

    IEnumerator DieRoutine(float breakSeconds)
    {
        if (!lamp.enabled) yield break; // если фонарь и так выключен — ничего не делаем
        busy = true;

        // Нервное мигание
        float t = 0f;
        while (t < flickerTime)
        {
            lamp.intensity = baseIntensity * Random.Range(0.05f, 1f);
            lamp.enabled = Random.value > 0.3f;
            float wait = Random.Range(0.03f, 0.15f);
            t += wait;
            yield return new WaitForSeconds(wait);
        }

        lamp.enabled = false;
        Play(dieSound);

        if (breakSeconds < 0f)
        {
            battery = 0f; // сел насовсем
        }
        else
        {
            yield return new WaitForSeconds(breakSeconds);
            UpdateBrightness();
            lamp.enabled = battery > 0f; // снова загорается сам
        }

        busy = false;
    }

    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }
}
