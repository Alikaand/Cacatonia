using UnityEngine;

// Дверь, которая плавно поворачивается на петлях. Открывается на E.
// Скрипт вешается на объект, у которого точка вращения (pivot) стоит на петле:
//   - у дверей из модели подруги pivot уже на петле — вешай прямо на дверь;
//   - у наших тестовых дверей это пустышка "петля", а створка — её дочерний объект.
// Дверь всегда крутится вокруг вертикальной оси мира, даже если модель повёрнута.
public class Door : MonoBehaviour, IInteractable
{
    public float openAngle = 90f;
    public float speed = 3f;

    [Tooltip("Дверь сама выбирает сторону и открывается ОТ игрока (не в лицо). " +
             "Если выключено — открывается всегда на Open Angle")]
    public bool openAwayFromPlayer = false;

    [Header("Замок (необязательно)")]
    public bool isLocked = false;
    [Tooltip("ID ключа, который открывает дверь, например \"key_basement\". Пусто = не открыть ничем")]
    public string requiredKeyId = "";
    [Tooltip("Текст, когда заперто")]
    public string lockedPrompt = "Заперто";

    [Header("Двойная дверь (необязательно)")]
    [Tooltip("Вторая створка. Если указать — обе открываются и закрываются вместе")]
    public Door linkedDoor;

    [Header("Звуки (необязательно)")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;

    bool isOpen;
    Quaternion closedRotation;
    Quaternion targetRotation;
    Vector3 upAxis; // вертикальная ось мира, переведённая в координаты родителя

    void Start()
    {
        closedRotation = transform.localRotation;
        targetRotation = closedRotation;
        upAxis = transform.parent != null
            ? transform.parent.InverseTransformDirection(Vector3.up).normalized
            : Vector3.up;
    }

    void Update()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * speed);
    }

    public string GetPrompt()
    {
        if (isLocked) return lockedPrompt;
        return isOpen ? "Закрыть дверь" : "Открыть дверь";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isLocked)
        {
            // Если у игрока есть нужный ключ — отпираем
            if (!string.IsNullOrEmpty(requiredKeyId) && player.inventory != null && player.inventory.HasItem(requiredKeyId))
            {
                isLocked = false;
                if (linkedDoor != null) linkedDoor.isLocked = false;
            }
            else
            {
                Play(lockedSound);
                return;
            }
        }

        SetOpen(!isOpen, player.transform.position);
        Play(isOpen ? openSound : closeSound);

        // Вторая створка повторяет за первой (без звука, чтобы не было двойного скрипа)
        if (linkedDoor != null) linkedDoor.SetOpen(isOpen, player.transform.position);
    }

    public void SetOpen(bool open, Vector3 playerPosition)
    {
        isOpen = open;
        if (!open)
        {
            targetRotation = closedRotation;
            return;
        }

        float angle = openAngle;
        if (openAwayFromPlayer)
            angle = Mathf.Abs(openAngle) * AwaySign(playerPosition);

        targetRotation = Quaternion.AngleAxis(angle, upAxis) * closedRotation;
    }

    // +1 или -1: в какую сторону крутить, чтобы свободный край двери ушёл ОТ игрока
    int AwaySign(Vector3 playerPosition)
    {
        Renderer r = GetComponentInChildren<Renderer>();
        Vector3 hinge = transform.position;
        Vector3 edge = (r != null ? r.bounds.center : hinge + transform.right) - hinge; // от петли к середине двери
        edge.y = 0f;
        // Куда начнёт двигаться край двери при повороте на +угол вокруг вертикали
        Vector3 move = Vector3.Cross(Vector3.up, edge);
        Vector3 toPlayer = playerPosition - hinge;
        toPlayer.y = 0f;
        return Vector3.Dot(move, toPlayer) > 0f ? -1 : 1;
    }

    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }
}
