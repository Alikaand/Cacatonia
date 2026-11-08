using UnityEngine;
using UnityEngine.AI;

// Монстр: бродит по точкам патруля, замечает игрока (видит или слышит бег)
// и гонится за ним. Если догнал — вызывает смерть игрока.
// Нужно: компонент NavMeshAgent на монстре + запечённый NavMesh на уровне
// (пакет AI Navigation, компонент NavMeshSurface → кнопка Bake).
[RequireComponent(typeof(NavMeshAgent))]
public class MonsterAI : MonoBehaviour
{
    enum State { Patrol, Chase, Search }

    public Transform player;
    public Transform[] patrolPoints;

    [Header("Скорости")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 4.5f; // чуть медленнее бега игрока (6), чтобы можно было убежать

    [Header("Зрение и слух")]
    public float viewDistance = 12f;
    public float viewAngle = 110f;       // ширина "конуса" зрения в градусах
    public float hearRunDistance = 10f;  // слышит, если игрок бежит
    public float hearWalkDistance = 4f;  // слышит, если игрок идёт (в приседе — не слышит)
    public float loseTargetTime = 5f;    // через сколько секунд без контакта бросает погоню

    [Header("Атака")]
    public float killDistance = 1.3f;

    [Header("Звуки (необязательно)")]
    public AudioSource audioSource;
    public AudioClip screamSound;

    NavMeshAgent agent;
    FirstPersonController playerController;
    State state = State.Patrol;
    int patrolIndex;
    float timeSinceSeen;
    Vector3 lastKnownPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (player != null) playerController = player.GetComponent<FirstPersonController>();
        GoToNextPatrolPoint();
    }

    void Update()
    {
        GetComponent<Animator>().SetFloat("Speed", GetComponent<UnityEngine.AI.NavMeshAgent>().velocity.magnitude);
        if (player == null) return;

        bool detected = CanSeePlayer() || CanHearPlayer();
        if (detected)
        {
            if (state != State.Chase && audioSource != null && screamSound != null)
                audioSource.PlayOneShot(screamSound);
                GetComponent<Animator>().SetTrigger("Alert");

            state = State.Chase;
            timeSinceSeen = 0f;
            lastKnownPosition = player.position;
        }

        switch (state)
        {
            case State.Patrol:
                agent.speed = patrolSpeed;
                GetComponent<Animator>().SetFloat("Speed", 1f);
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    GoToNextPatrolPoint();
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                GetComponent<Animator>().SetFloat("Speed", 3f);
                agent.SetDestination(lastKnownPosition);
                timeSinceSeen += Time.deltaTime;
                if (timeSinceSeen > loseTargetTime) state = State.Search;

                if (Vector3.Distance(transform.position, player.position) < killDistance)
                {
                    GameManager.Instance.PlayerDied();
                    GetComponent<Animator>().SetTrigger("Catch");
                }
                break;

            case State.Search:
                // Дошёл до места, где последний раз видел игрока, — возвращается к патрулю
                agent.speed = patrolSpeed;
                agent.SetDestination(lastKnownPosition);
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    state = State.Patrol;
                    GoToNextPatrolPoint();
                }
                break;
        }
    }

    bool CanSeePlayer()
    {
        Vector3 eyes = transform.position + Vector3.up * 1.6f;
        Vector3 toPlayer = player.position - eyes;

        if (toPlayer.magnitude > viewDistance) return false;
        if (Vector3.Angle(transform.forward, toPlayer) > viewAngle / 2f) return false;

        // Проверяем, нет ли стены между монстром и игроком
        if (Physics.Raycast(eyes, toPlayer.normalized, out RaycastHit hit, viewDistance, ~0, QueryTriggerInteraction.Ignore))
            return hit.transform == player || hit.transform.IsChildOf(player);

        return false;
    }

    bool CanHearPlayer()
    {
        if (playerController == null) return false;
        float distance = Vector3.Distance(transform.position, player.position);

        if (playerController.IsRunning) return distance < hearRunDistance;
        if (playerController.IsCrouching) return false;

        // Идёт (есть нажатые клавиши движения)
        bool moving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
        return moving && distance < hearWalkDistance;
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[patrolIndex].position);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    // Вернуть монстра в начало (при респауне игрока)
    public void ResetMonster(Vector3 position)
    {
        agent.Warp(position);
        state = State.Patrol;
        timeSinceSeen = 0f;
        GoToNextPatrolPoint();
    }

    // Рисуем радиусы в окне Scene, чтобы было удобно настраивать
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearRunDistance);
    }
}
