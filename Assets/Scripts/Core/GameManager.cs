using System.Collections;
using UnityEngine;
using TMPro;

// "Главный" объект игры: помнит последнюю контрольную точку,
// обрабатывает смерть игрока и показывает сообщения.
// На сцене должен быть ОДИН пустой объект GameManager с этим скриптом.
public class GameManager : MonoBehaviour
{
    // Синглтон: из любого скрипта можно написать GameManager.Instance.PlayerDied()
    public static GameManager Instance { get; private set; }

    public FirstPersonController player;
    public CanvasGroup deathScreen;   // чёрный экран с надписью "Вы погибли" (необязательно)
    public TMP_Text messageText;      // текст для сообщений (необязательно)
    public float respawnDelay = 2f;

    Vector3 respawnPosition;
    Quaternion respawnRotation;
    bool isDead;
    Coroutine messageRoutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Стартовая позиция — первая "контрольная точка"
        respawnPosition = player.transform.position;
        respawnRotation = player.transform.rotation;
        if (deathScreen != null) deathScreen.alpha = 0f;
        if (messageText != null) messageText.text = "";
    }

    public void SetCheckpoint(Transform point)
    {
        respawnPosition = point.position;
        respawnRotation = point.rotation;
        Debug.Log("Контрольная точка сохранена");
    }

    public void PlayerDied()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        player.canMove = false;
        if (deathScreen != null) deathScreen.alpha = 1f;

        yield return new WaitForSeconds(respawnDelay);

        // CharacterController нужно выключить, иначе он не даст телепортировать игрока
        CharacterController cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.transform.SetPositionAndRotation(respawnPosition, respawnRotation);
        cc.enabled = true;

        // Монстры возвращаются на свои стартовые места
        foreach (MonsterAI m in FindObjectsByType<MonsterAI>(FindObjectsSortMode.None))
        {
            MonsterSpawnInfo info = m.GetComponent<MonsterSpawnInfo>();
            if (info != null) m.ResetMonster(info.StartPosition);
        }

        if (deathScreen != null) deathScreen.alpha = 0f;
        player.canMove = true;
        isDead = false;
    }

    // Короткая версия для событий в Inspector (UnityEvent не видит методы с двумя параметрами)
    public void Say(string text)
    {
        ShowMessage(text, 3f);
    }

    public void ShowMessage(string text, float duration = 3f)
    {
        if (messageText == null) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageText.text = text;
        messageRoutine = StartCoroutine(HideMessage(duration));
    }

    IEnumerator HideMessage(float delay)
    {
        yield return new WaitForSeconds(delay);
        messageText.text = "";
    }
}
