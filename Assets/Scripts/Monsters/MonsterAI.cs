using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

// Монстр из палаты 313.
// Состояния: Бродит -> Заметил (крик НА МЕСТЕ) -> Погоня -> Ищет (потерял из виду) -> Поимка (разворот камеры + укус).
// Анимации запускаются ТОЛЬКО этим скриптом по именам состояний в Animator (переходы в Animator не нужны).
[RequireComponent(typeof(NavMeshAgent))]
public class MonsterAI : MonoBehaviour
{
    public enum State { Wander, Alert, Chase, Search, Catch }

    [Header("Ссылки на игрока")]
    [Tooltip("Корень игрока (объект, который поворачивается влево-вправо)")]
    public Transform player;
    [Tooltip("Камера игрока (поворачивается вверх-вниз)")]
    public Transform playerCamera;
    [Tooltip("Скрипты управления игроком: ходьба, мышь и т.п. Выключаются, когда монстр поймал игрока")]
    public Behaviour[] playerControlScripts;
    [Tooltip("Фонарик игрока (необязательно). Если включён — монстр видит дальше")]
    public Light playerFlashlight;

    [Header("Монстр")]
    [Tooltip("Точка глаз. Если пусто — берётся кость головы")]
    public Transform eyes;
    [Tooltip("Точки, между которыми монстр бродит. Можно оставить пустым — тогда стоит на месте")]
    public Transform[] wanderPoints;

    [Header("Зрение")]
    public float viewDistance = 12f;
    public float viewDistanceFlashlightOn = 20f;
    [Range(10f, 180f)] public float viewAngle = 110f;       // ширина конуса зрения
    [Tooltip("Слои, которые закрывают обзор (стены, двери). Слой монстра сюда НЕ включать")]
    public LayerMask obstacleMask = ~0;
    [Tooltip("Сколько секунд монстр ищет игрока, потеряв его из виду, прежде чем вернуться бродить")]
    public float searchTime = 5f;

    [Header("Скорости")]
    public float walkSpeed = 1.0f;
    public float runSpeed = 3.2f;
    public float wanderWait = 2f;
    public float turnSpeed = 360f;                          // градусов в секунду

    [Header("Поимка")]
    public float catchDistance = 1.3f;
    [Range(10f, 180f)] public float catchAngle = 70f;       // игрок должен быть перед монстром
    public float turnPlayerTime = 0.4f;                     // за сколько камера разворачивается к лицу монстра
    [Tooltip("Через сколько секунд после начала укуса показывать смерть")]
    public float biteKillDelay = 0.8f;
    public UnityEvent onPlayerKilled;                       // сюда: экран смерти / вспышка / телепорт к лифту

    [Header("Имена состояний в Animator")]
    public string idleState = "Idle";
    public string walkState = "Walk";
    public string runState = "Run";
    public string yellState = "Yell";
    public string biteState = "Bite";
    [Tooltip("0 = взять длину клипа крика автоматически")]
    public float yellDuration = 0f;
    public float crossFade = 0.15f;

    public State CurrentState { get; private set; }

    NavMeshAgent agent;
    Animator anim;
    Transform head;
    string currentAnim;
    float lastSeenTime;
    Vector3 lastSeenPos;
    int wanderIndex;
    float waitUntil = -1f;
    float nextRepath;
    bool lockCamera;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        anim.applyRootMotion = false;                       // двигает только NavMeshAgent, анимации — на месте
        head = anim.isHuman ? anim.GetBoneTransform(HumanBodyBones.Head) : transform;
        if (eyes == null) eyes = head;
        agent.stoppingDistance = catchDistance * 0.8f;      // не упирается в игрока и не толкает его
        agent.autoBraking = true;
    }

    void Start() { EnterWander(); }

    void Update()
    {
        switch (CurrentState)
        {
            case State.Wander: UpdateWander(); break;
            case State.Chase: UpdateChase(); break;
            case State.Search: UpdateSearch(); break;
                // Alert и Catch управляются корутинами
        }
    }

    // ---------- БРОДИТ ----------
    void EnterWander()
    {
        CurrentState = State.Wander;
        agent.isStopped = false;
        agent.speed = walkSpeed;
        waitUntil = -1f;
        if (wanderPoints != null && wanderPoints.Length > 0)
            agent.SetDestination(wanderPoints[wanderIndex].position);
    }

    void UpdateWander()
    {
        if (CanSeePlayer()) { StartCoroutine(AlertRoutine()); return; }   // крик только при ПЕРВОМ обнаружении

        if (wanderPoints != null && wanderPoints.Length > 0 && !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            if (waitUntil < 0f) waitUntil = Time.time + wanderWait;
            else if (Time.time >= waitUntil)
            {
                wanderIndex = (wanderIndex + 1) % wanderPoints.Length;
                agent.SetDestination(wanderPoints[wanderIndex].position);
                waitUntil = -1f;
            }
        }
        PlayMove(walkState);
    }

    // ---------- ЗАМЕТИЛ: КРИК НА МЕСТЕ ----------
    IEnumerator AlertRoutine()
    {
        CurrentState = State.Alert;
        StopAgent();
        PlayAnim(yellState, true);

        yield return null; yield return null;               // ждём, пока аниматор переключится
        float dur = yellDuration;
        if (dur <= 0f)
        {
            var info = anim.IsInTransition(0) ? anim.GetNextAnimatorStateInfo(0) : anim.GetCurrentAnimatorStateInfo(0);
            dur = info.length > 0.1f ? info.length : 2f;
        }

        float t = 0f;
        while (t < dur)
        {
            StopAgent();                                     // стоит на месте весь крик
            FaceTowards(player.position);
            t += Time.deltaTime;
            yield return null;
        }

        lastSeenTime = Time.time;
        lastSeenPos = player.position;
        CurrentState = State.Chase;                          // только теперь начинает бежать
        agent.isStopped = false;
        agent.speed = runSpeed;
    }

    // ---------- ПОГОНЯ ----------
    void UpdateChase()
    {
        if (CanSeePlayer()) { lastSeenTime = Time.time; lastSeenPos = player.position; }
        else { CurrentState = State.Search; agent.SetDestination(lastSeenPos); PlayMove(runState); return; }

        float dist = FlatDistance(transform.position, player.position);
        if (dist <= catchDistance)
        {
            StopAgent();                                     // рядом — не толкает, а останавливается
            FaceTowards(player.position);
            if (IsInFront(catchAngle)) { StartCoroutine(CatchRoutine()); return; }
            PlayAnim(idleState);
            return;
        }

        agent.isStopped = false;
        agent.speed = runSpeed;
        if (Time.time >= nextRepath) { agent.SetDestination(player.position); nextRepath = Time.time + 0.2f; }
        PlayMove(runState);
    }

    // ---------- ПОТЕРЯЛ ИЗ ВИДУ ----------
    void UpdateSearch()
    {
        if (CanSeePlayer())                                  // снова увидел — БЕЗ повторного крика
        {
            lastSeenTime = Time.time; lastSeenPos = player.position;
            CurrentState = State.Chase;
            return;
        }
        agent.isStopped = false;
        agent.speed = runSpeed;
        PlayMove(runState);

        bool arrived = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f;
        if (arrived && Time.time - lastSeenTime > searchTime) EnterWander();   // сдался — снова бродит
    }

    // ---------- ПОИМКА ----------
    IEnumerator CatchRoutine()
    {
        CurrentState = State.Catch;
        StopAgent();
        foreach (var s in playerControlScripts) if (s) s.enabled = false;     // игрок не может двигаться

        // монстр лицом к игроку, камера игрока — к лицу монстра
        Quaternion pStart = player.rotation;
        Quaternion cStart = playerCamera.rotation;
        float t = 0f;
        while (t < turnPlayerTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / turnPlayerTime);
            FaceTowards(player.position);
            Vector3 flat = head.position - player.position; flat.y = 0f;
            if (flat.sqrMagnitude > 0.001f) player.rotation = Quaternion.Slerp(pStart, Quaternion.LookRotation(flat), k);
            playerCamera.rotation = Quaternion.Slerp(cStart, Quaternion.LookRotation(head.position - playerCamera.position), k);
            PlayAnim(idleState);
            yield return null;
        }

        lockCamera = true;                                   // камера держит лицо монстра во время укуса
        PlayAnim(biteState, true);                           // укус начинается только ПОСЛЕ разворота
        yield return new WaitForSeconds(biteKillDelay);
        onPlayerKilled.Invoke();
        // Вызываем смерть напрямую, без UnityEvent
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }
    }

    void LateUpdate()
    {
        if (lockCamera && playerCamera && head)
            playerCamera.rotation = Quaternion.LookRotation(head.position - playerCamera.position);
    }

    // Вызвать после экрана смерти/респавна: вернуть управление игроку и монстра в обычный режим
    public void ReleasePlayerAndReset()
    {
        StopAllCoroutines();
        lockCamera = false;
        foreach (var s in playerControlScripts) if (s) s.enabled = true;
        currentAnim = null;
        EnterWander();
    }

    // ---------- ЗРЕНИЕ ----------
    public bool CanSeePlayer()
    {
        if (!player) return false;
        Vector3 eyePos = eyes.position;
        Vector3 target = playerCamera ? playerCamera.position : player.position + Vector3.up * 1.6f;
        Vector3 to = target - eyePos;
        float dist = to.magnitude;

        bool lightOn = playerFlashlight && playerFlashlight.enabled && playerFlashlight.gameObject.activeInHierarchy;
        if (dist > (lightOn ? viewDistanceFlashlightOn : viewDistance)) return false;

        Vector3 flat = to; flat.y = 0f;
        if (Vector3.Angle(transform.forward, flat) > viewAngle * 0.5f) return false;   // за спиной/сбоку — не видит

        var hits = Physics.RaycastAll(eyePos, to / dist, dist, obstacleMask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var h in hits)
        {
            if (h.transform.IsChildOf(transform)) continue;   // собственные коллайдеры монстра
            return h.transform.IsChildOf(player);             // первое препятствие — игрок? значит видит
        }
        return true;
    }

    bool IsInFront(float angle)
    {
        Vector3 flat = player.position - transform.position; flat.y = 0f;
        return Vector3.Angle(transform.forward, flat) <= angle * 0.5f;
    }

    // ---------- ВСПОМОГАТЕЛЬНОЕ ----------
    void StopAgent()
    {
        if (!agent.enabled) return;
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    void FaceTowards(Vector3 pos)
    {
        Vector3 d = pos - transform.position; d.y = 0f;
        if (d.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(d), turnSpeed * Time.deltaTime);
    }

    static float FlatDistance(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }

    void PlayMove(string moveState)
    {
        PlayAnim(agent.velocity.sqrMagnitude > 0.05f ? moveState : idleState);
    }

    void PlayAnim(string state, bool restart = false)
    {
        if (!restart && currentAnim == state) return;
        anim.CrossFadeInFixedTime(state, crossFade, 0, 0f);
        currentAnim = state;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 p = transform.position + Vector3.up * 1.5f;
        Vector3 l = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 r = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawLine(p, p + l * viewDistance);
        Gizmos.DrawLine(p, p + r * viewDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
    }
    // Возвращает монстра на точку после респавна игрока
    public void ResetMonster(Vector3 position)
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.enabled)
        {
            agent.Warp(position);
            EnterWander();
        }
    }
    // Вызывает смерть игрока через GameManager
    public void KillPlayer()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }
        else
        {
            Debug.LogError("GameManager не найден на сцене!");
        }
    }
}

