using System.Collections;
using UnityEngine;

// Чёрный экран, который плавно появляется и исчезает.
// Вешается на UI-объект (Image чёрного цвета на весь экран) вместе с компонентом Canvas Group.
// Другие скрипты пишут: yield return ScreenFader.Instance.FadeOut();  — экран темнеет
//                       yield return ScreenFader.Instance.FadeIn();   — экран светлеет
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Tooltip("Сколько секунд длится затемнение")]
    public float duration = 0.7f;

    CanvasGroup group;

    void Awake()
    {
        Instance = this;
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;              // в начале игры экран прозрачный
        group.blocksRaycasts = false;
    }

    public IEnumerator FadeOut()
    {
        yield return Fade(1f);
    }

    public IEnumerator FadeIn()
    {
        yield return Fade(0f);
    }

    IEnumerator Fade(float target)
    {
        float start = group.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        group.alpha = target;
    }
}
