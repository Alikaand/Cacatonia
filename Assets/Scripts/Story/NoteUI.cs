using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Экран чтения записки: тёмная панель, заголовок, текст и (по желанию) картинка.
// Вешается на панель внутри Canvas. Сама панель в начале игры прячется.
// Закрыть записку — E, Esc или правая кнопка мыши.
public class NoteUI : MonoBehaviour
{
    public static NoteUI Instance { get; private set; }

    public GameObject panel;          // сама панель (обычно этот же объект)
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public Image picture;             // для картинок (план эвакуации, рисунок). Необязательно

    public bool IsOpen { get; private set; }
    // Кадр, в котором записку закрыли: чтобы та же нажатая E не открыла её снова
    public static int ClosedFrame = -1;

    FirstPersonController player;
    float openedAt;

    void Awake()
    {
        Instance = this;
        if (panel == null) panel = gameObject;
        panel.SetActive(false);
    }

    void Start()
    {
        player = FindObjectOfType<FirstPersonController>();
    }

    void Update()
    {
        if (!IsOpen) return;
        // Небольшая пауза, чтобы та же нажатая E не закрыла записку сразу
        if (Time.unscaledTime - openedAt < 0.2f) return;
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            Close();
    }

    public void Show(string title, string body, Sprite image)
    {
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;
        if (picture != null)
        {
            picture.gameObject.SetActive(image != null);
            picture.sprite = image;
            picture.preserveAspect = true;
        }
        panel.SetActive(true);
        IsOpen = true;
        openedAt = Time.unscaledTime;
        if (player != null) player.canMove = false;
    }

    public void Close()
    {
        panel.SetActive(false);
        IsOpen = false;
        ClosedFrame = Time.frameCount;
        if (player != null) player.canMove = true;
    }
}
