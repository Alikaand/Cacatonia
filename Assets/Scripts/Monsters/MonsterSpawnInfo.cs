using UnityEngine;

// Запоминает, где монстр стоял в начале, чтобы вернуть его туда после смерти игрока.
// Повесь на того же монстра, что и MonsterAI.
public class MonsterSpawnInfo : MonoBehaviour
{
    public Vector3 StartPosition { get; private set; }

    void Awake()
    {
        StartPosition = transform.position;
    }
}
