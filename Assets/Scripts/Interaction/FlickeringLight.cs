using UnityEngine;

// Мигающая лампа — для атмосферы. Вешается на объект с компонентом Light.
// Работает только когда свет включён (не мешает LightSwitch).
[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    public float minIntensity = 0.2f;
    public float maxIntensity = 1.5f;
    public float minDelay = 0.05f;
    public float maxDelay = 0.4f;

    Light lamp;
    float timer;

    void Start()
    {
        lamp = GetComponent<Light>();
    }

    void Update()
    {
        if (!lamp.enabled) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            lamp.intensity = Random.Range(minIntensity, maxIntensity);
            timer = Random.Range(minDelay, maxDelay);
        }
    }
}
