using System.Collections;
using UnityEngine;

// «Фальшивый лифт»: кабина на каждом этаже стоит на месте, никуда не едет.
//   - Кнопка вызова снаружи открывает двери лифта на этом этаже.
//   - Кнопка этажа внутри: двери закрываются → экран темнеет → игрока переносит
//     на нужную высоту (кабины стоят строго одна над другой) → экран светлеет →
//     двери на новом этаже открываются.
// Двери — пара створок, которые разъезжаются в стороны.
// Вешается на ОДИН пустой объект Elevator в корне сцены.
// Проще всего настроить через меню Hospital → 2. Setup Elevator.
public class FakeElevator : MonoBehaviour
{
    [System.Serializable]
    public class LiftDoors
    {
        public Transform left;
        public Transform right;
        [HideInInspector] public Vector3 leftClosed, rightClosed, leftDir, rightDir;
        [HideInInspector] public float openAmount, target; // 0 = закрыто, 1 = открыто
    }

    [Tooltip("Высота пола каждого этажа. Этаж 1 = элемент 0")]
    public float[] floorHeights = { 0f, 3.698f, 7.396f };

    [Tooltip("Створки лифта на каждом этаже по порядку")]
    public LiftDoors[] doors;

    [Tooltip("На сколько метров разъезжается каждая створка")]
    public float slideDistance = 0.62f;
    public float doorSpeed = 1.5f;

    [Tooltip("Игрок. Можно оставить пустым — скрипт найдёт его сам")]
    public FirstPersonController player;

    [Tooltip("Сколько секунд экран остаётся чёрным («едем»)")]
    public float rideTime = 1.5f;

    [Header("Звуки (необязательно)")]
    public AudioSource audioSource;
    public AudioClip doorSound;
    public AudioClip rideSound;
    public AudioClip arriveSound;

    [Header("Сюжет")]
    [Tooltip("Без галочки лифт обесточен: кнопки не работают (глава 1). Включается рубильниками: SetPowered(true)")]
    public bool powered = true;
    [Tooltip("Этаж, для которого нужен ключ (2 = третий этаж). -1 = ключ не нужен")]
    public int lockedFloor = -1;
    [Tooltip("ID ключа для этого этажа, например lift_key")]
    public string lockedFloorKeyId = "lift_key";

    public bool IsRiding { get; private set; }

    // Для событий: рубильники, сюжет
    public void SetPowered(bool value)
    {
        powered = value;
    }

    void Start()
    {
        if (player == null) player = FindObjectOfType<FirstPersonController>();

        if (doors == null) return;
        foreach (LiftDoors d in doors)
        {
            if (d == null || d.left == null || d.right == null) continue;
            d.leftClosed = d.left.localPosition;
            d.rightClosed = d.right.localPosition;
            // Створки разъезжаются друг от друга — направление считается само
            Vector3 apart = (d.leftClosed - d.rightClosed).normalized;
            d.leftDir = apart;
            d.rightDir = -apart;
            d.openAmount = 0f;
            d.target = 0f; // в начале все двери закрыты
        }
    }

    void Update()
    {
        if (doors == null) return;
        foreach (LiftDoors d in doors)
        {
            if (d == null || d.left == null || d.right == null) continue;
            d.openAmount = Mathf.MoveTowards(d.openAmount, d.target, Time.deltaTime * doorSpeed);
            float s = Mathf.SmoothStep(0f, 1f, d.openAmount) * slideDistance;
            d.left.localPosition = d.leftClosed + d.leftDir * s;
            d.right.localPosition = d.rightClosed + d.rightDir * s;
        }
    }

    // Номер этажа по высоте: ближайший пол под этой точкой.
    // Игрок и кнопки находятся примерно на 1 м выше пола, поэтому вычитаем 1.
    public int FloorIndexAt(float worldY)
    {
        int best = 0;
        for (int i = 1; i < floorHeights.Length; i++)
        {
            if (Mathf.Abs(worldY - 1f - floorHeights[i]) < Mathf.Abs(worldY - 1f - floorHeights[best]))
                best = i;
        }
        return best;
    }

    public bool AreDoorsOpen(int floor)
    {
        return doors != null && floor >= 0 && floor < doors.Length && doors[floor] != null && doors[floor].target > 0.5f;
    }

    // Кнопка вызова снаружи
    public void Call(int floor)
    {
        if (IsRiding || !powered) return;
        SetDoors(floor, true);
        Play(doorSound);
    }

    // Кнопка этажа внутри кабины
    public void Ride(int targetFloor)
    {
        if (IsRiding || player == null || !powered) return;
        if (targetFloor < 0 || targetFloor >= floorHeights.Length) return;
        if (targetFloor == lockedFloor && !HasFloorKey()) return;
        StartCoroutine(RideRoutine(targetFloor));
    }

    IEnumerator RideRoutine(int target)
    {
        int from = FloorIndexAt(player.transform.position.y);
        if (from == target) yield break;

        IsRiding = true;
        player.canMove = false;

        // 1. Закрываем двери
        SetDoors(from, false);
        Play(doorSound);
        yield return new WaitForSeconds(1f / doorSpeed + 0.1f);

        // 2. Темнеем
        if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeOut();

        // 3. «Едем» и переносим игрока
        Play(rideSound);
        yield return new WaitForSeconds(rideTime);

        CharacterController cc = player.GetComponent<CharacterController>();
        cc.enabled = false; // иначе CharacterController не даст передвинуть игрока
        player.transform.position += Vector3.up * (floorHeights[target] - floorHeights[from]);
        cc.enabled = true;

        // 4. Светлеем, приехали, открываем двери
        if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeIn();
        Play(arriveSound);
        yield return new WaitForSeconds(0.3f);
        SetDoors(target, true);
        Play(doorSound);

        player.canMove = true;
        IsRiding = false;
    }

    public bool HasFloorKey()
    {
        if (string.IsNullOrEmpty(lockedFloorKeyId)) return true;
        PlayerInventory inv = player != null ? player.GetComponent<PlayerInventory>() : null;
        return inv != null && inv.HasItem(lockedFloorKeyId);
    }

    void SetDoors(int floor, bool open)
    {
        if (doors == null || floor < 0 || floor >= doors.Length || doors[floor] == null) return;
        doors[floor].target = open ? 1f : 0f;
    }

    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }
}
