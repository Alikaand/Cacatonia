using UnityEngine;

// Маятниковая дверь (как в больницах между отделениями): без ручки.
// Когда игрок подходит — створка сама отходит ОТ него, когда уходит — закрывается.
// Вешается на каждую створку (у модели подруги pivot уже на петле).
// Кнопка E не нужна.
public class SwingDoor : MonoBehaviour
{
    public float openAngle = 80f;
    [Tooltip("На каком расстоянии от петли дверь начинает открываться")]
    public float openDistance = 1.8f;
    public float speed = 4f;

    [Header("Звук (необязательно)")]
    public AudioSource audioSource;
    public AudioClip swingSound;

    Transform player;
    Quaternion closedRotation;
    Vector3 upAxis;
    int currentSide; // 0 = закрыта, +1 / -1 = открыта в одну из сторон

    void Start()
    {
        closedRotation = transform.localRotation;
        upAxis = transform.parent != null
            ? transform.parent.InverseTransformDirection(Vector3.up).normalized
            : Vector3.up;

        FirstPersonController fpc = FindObjectOfType<FirstPersonController>();
        if (fpc != null) player = fpc.transform;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        bool near = toPlayer.magnitude < openDistance;

        if (!near)
        {
            currentSide = 0;
        }
        else if (currentSide == 0)
        {
            // Выбираем сторону один раз, когда игрок подошёл, чтобы дверь не дёргалась
            currentSide = AwaySign(toPlayer);
            if (audioSource != null && swingSound != null) audioSource.PlayOneShot(swingSound);
        }

        Quaternion target = currentSide == 0
            ? closedRotation
            : Quaternion.AngleAxis(openAngle * currentSide, upAxis) * closedRotation;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * speed);
    }

    int AwaySign(Vector3 toPlayer)
    {
        Renderer r = GetComponentInChildren<Renderer>();
        Vector3 edge = (r != null ? r.bounds.center : transform.position + transform.right) - transform.position;
        edge.y = 0f;
        Vector3 move = Vector3.Cross(Vector3.up, edge);
        return Vector3.Dot(move, toPlayer) > 0f ? -1 : 1;
    }
}
