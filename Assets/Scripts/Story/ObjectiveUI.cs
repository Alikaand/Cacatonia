using UnityEngine;
using TMPro;

// Текущая задача сверху экрана: «Задача: подняться на 2 этаж».
// Вешается на TextMeshPro-текст в Canvas.
// Менять задачу можно из событий (TriggerEvent, InteractEvent, NoteItem):
//   ObjectiveUI → SetObjective("подняться на 2 этаж")
// Для счётчиков («Палаты: 2 / 5»):
//   StartCounter("Отметить палаты", 5)  и потом  AddProgress()  в каждой палате.
public class ObjectiveUI : MonoBehaviour
{
    public static ObjectiveUI Instance { get; private set; }

    public TMP_Text text;
    [TextArea] public string startObjective = "Осмотреть пост медсестры";

    string counterTitle;
    int counterCurrent, counterTotal;

    [Tooltip("Что произойдёт, когда счётчик заполнится (необязательно)")]
    public UnityEngine.Events.UnityEvent onCounterComplete;

    void Awake()
    {
        Instance = this;
        if (text == null) text = GetComponent<TMP_Text>();
    }

    void Start()
    {
        SetObjective(startObjective);
    }

    public void SetObjective(string objective)
    {
        counterTotal = 0;
        if (text != null) text.text = string.IsNullOrEmpty(objective) ? "" : "Задача: " + objective;
    }

    public void StartCounter(string title)
    {
        // Вариант для событий: число берётся из Counter Total ниже
        StartCounterWithTotal(title, defaultCounterTotal);
    }

    [Tooltip("Сколько нужно для StartCounter, если вызывать его из события")]
    public int defaultCounterTotal = 5;

    public void StartCounterWithTotal(string title, int total)
    {
        counterTitle = title;
        counterCurrent = 0;
        counterTotal = total;
        Refresh();
    }

    public void AddProgress()
    {
        if (counterTotal <= 0) return;
        counterCurrent = Mathf.Min(counterCurrent + 1, counterTotal);
        Refresh();
        if (counterCurrent >= counterTotal) onCounterComplete.Invoke();
    }

    public void Clear()
    {
        SetObjective("");
    }

    void Refresh()
    {
        if (text == null) return;
        string check = counterCurrent >= counterTotal ? "  (готово)" : "";
        text.text = $"Задача: {counterTitle}  {counterCurrent} / {counterTotal}{check}";
    }
}
