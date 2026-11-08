using UnityEngine;
using UnityEngine.SceneManagement;

// Финиш уровня: зона-триггер. Когда игрок заходит — грузится следующая сцена
// (или сцена с титрами "Вы выбрались"). Сцену нужно добавить в File → Build Profiles.
[RequireComponent(typeof(Collider))]
public class LevelExit : MonoBehaviour
{
    public string nextSceneName = "Ending";

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(nextSceneName);
    }
}
