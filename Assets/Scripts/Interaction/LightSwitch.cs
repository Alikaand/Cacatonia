using UnityEngine;

// Выключатель на стене: включает/выключает одну или несколько ламп.
// Вешается на модель выключателя (у неё должен быть Collider).
// В поле "lights" перетащи объекты с компонентом Light.
public class LightSwitch : MonoBehaviour, IInteractable
{
    public Light[] lights;
    public bool isOn = true;

    [Tooltip("Модель плафона с 'светящимся' материалом — необязательно")]
    public Renderer[] lampRenderers;
    public Material onMaterial;
    public Material offMaterial;

    public AudioSource audioSource;
    public AudioClip clickSound;

    void Start()
    {
        Apply();
    }

    public string GetPrompt()
    {
        return isOn ? "Выключить свет" : "Включить свет";
    }

    public void Interact(PlayerInteraction player)
    {
        isOn = !isOn;
        Apply();
        if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);
    }

    void Apply()
    {
        foreach (Light l in lights)
            if (l != null) l.enabled = isOn;

        if (onMaterial != null && offMaterial != null)
            foreach (Renderer r in lampRenderers)
                if (r != null) r.material = isOn ? onMaterial : offMaterial;
    }
}
