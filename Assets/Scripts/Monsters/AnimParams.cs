using UnityEngine;

public class AnimParams : MonoBehaviour
{
    private Animator anim;
    private float fixedY;

    void Start()
    {
        anim = GetComponent<Animator>();
        // Запоминаем высоту, на которой стоит монстр
        fixedY = transform.position.y;
    }

    void LateUpdate()
    {
        // Каждый кадр возвращаем его на нужную высоту, игнорируя анимацию
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;
    }
}