using UnityEngine;

// Контрольная точка: невидимая зона (Box Collider с галочкой Is Trigger).
// Когда игрок в неё заходит:
//   1) сохраняется точка респауна;
//   2) включаются монстры из списка monstersToActivate.
// Игрок должен иметь тег "Player".
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Где появится игрок после смерти. Если пусто — в центре этой зоны.")]
    public Transform respawnPoint;

    [Tooltip("Монстры, которые появятся после этой точки (сначала они выключены на сцене)")]
    public GameObject[] monstersToActivate;

    [Tooltip("Текст на экране при достижении точки (необязательно)")]
    public string message = "";

    bool triggered;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        Transform point = respawnPoint != null ? respawnPoint : transform;
        GameManager.Instance.SetCheckpoint(point);

        foreach (GameObject m in monstersToActivate)
            if (m != null) m.SetActive(true);

        if (!string.IsNullOrEmpty(message))
            GameManager.Instance.ShowMessage(message);
    }
}
