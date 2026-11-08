using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Автоматическая настройка больницы из модели подруги (Hospital.fbx).
// Кладётся в папку Assets/Editor. В Unity появляется меню Hospital:
//   1. Setup Doors     — скрипты на все двери, коробчатые коллайдеры, пары двойных дверей
//   2. Setup Elevator  — лифт-телепорт: двери, кнопки в кабинах и кнопки вызова
//   3. Add Lights      — настоящий свет под каждой лампой в коридорах, палатах и лифтах
// Всё можно отменить через Ctrl+Z.
public static class HospitalSetup
{
    // ---------- поиск объектов ----------
    static List<Transform> FindAll(string pattern)
    {
        var rx = new Regex(pattern);
        var result = new List<Transform>();
        foreach (Transform t in Object.FindObjectsOfType<Transform>(true))
            if (rx.IsMatch(t.name)) result.Add(t);
        result.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        return result;
    }

    static Transform FindOne(string exactName)
    {
        foreach (Transform t in Object.FindObjectsOfType<Transform>(true))
            if (t.name == exactName) return t;
        return null;
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        if (c == null) c = Undo.AddComponent<T>(go);
        return c;
    }

    // Меш-коллайдер с подвижной вещи убираем и ставим коробку по размеру меша
    static void UseBoxCollider(GameObject go, float minSize = 0f)
    {
        foreach (MeshCollider mc in go.GetComponents<MeshCollider>())
            Undo.DestroyObjectImmediate(mc);
        BoxCollider box = go.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = Undo.AddComponent<BoxCollider>(go); // сам подстраивается под меш
        }
        if (minSize > 0f)
        {
            Undo.RecordObject(box, "Box size");
            Vector3 s = box.size;
            box.size = new Vector3(Mathf.Max(s.x, minSize), Mathf.Max(s.y, minSize), Mathf.Max(s.z, minSize));
        }
    }

    static void MarkNotStatic(GameObject go)
    {
        Undo.RecordObject(go, "Not static");
        GameObjectUtility.SetStaticEditorFlags(go, 0);
    }

    static void Done(string text)
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log(text);
        EditorUtility.DisplayDialog("Hospital", text, "OK");
    }

    // ---------- 1. ДВЕРИ ----------
    [MenuItem("Hospital/1. Setup Doors")]
    static void SetupDoors()
    {
        int single = 0, pairs = 0, swing = 0, fake = 0, hidden = 0;

        // Обычные двери в палаты
        foreach (Transform t in FindAll(@"^F\d_Door_\d+$"))
        {
            UseBoxCollider(t.gameObject);
            MarkNotStatic(t.gameObject);
            Door d = GetOrAdd<Door>(t.gameObject);
            Undo.RecordObject(d, "Door");
            d.openAngle = 90f;
            d.openAwayFromPlayer = true;
            single++;
        }

        // Двойные двери и главный вход: створки _L и _R открываются вместе
        foreach (Transform l in FindAll(@"^F\d_(DoubleDoor|MainDoor)_\d+_L$"))
        {
            Transform r = FindOne(l.name.Substring(0, l.name.Length - 1) + "R");
            if (r == null) continue;
            foreach (Transform t in new[] { l, r })
            {
                UseBoxCollider(t.gameObject);
                MarkNotStatic(t.gameObject);
            }
            Door dl = GetOrAdd<Door>(l.gameObject);
            Door dr = GetOrAdd<Door>(r.gameObject);
            Undo.RecordObject(dl, "Door"); Undo.RecordObject(dr, "Door");
            dl.openAwayFromPlayer = dr.openAwayFromPlayer = true;
            dl.openAngle = dr.openAngle = 90f;
            dl.linkedDoor = dr; dr.linkedDoor = dl;

            // Главный вход заперт: открывается только ключом "exit" (по сюжету — финал)
            if (l.name.Contains("MainDoor"))
            {
                dl.isLocked = dr.isLocked = true;
                dl.requiredKeyId = dr.requiredKeyId = "exit";
                dl.lockedPrompt = dr.lockedPrompt = "Заперто снаружи";
            }
            pairs++;
        }

        // Маятниковые двери между крыльями
        foreach (Transform t in FindAll(@"^F\d_SwingDoor_\d+_[LR]$"))
        {
            UseBoxCollider(t.gameObject);
            MarkNotStatic(t.gameObject);
            GetOrAdd<SwingDoor>(t.gameObject);
            swing++;
        }

        // Фальшивые двери в конце коридора: всегда «Заперто»
        foreach (Transform t in FindAll(@"^F\d_FakeDoor_\d+$"))
        {
            UseBoxCollider(t.gameObject);
            Door d = GetOrAdd<Door>(t.gameObject);
            Undo.RecordObject(d, "Door");
            d.isLocked = true;
            d.requiredKeyId = "";
            d.lockedPrompt = "Заперто";
            fake++;
        }

        // В новой модели F2_Slab_Backup — это настоящий пол 1-го этажа. Если его спрятала
        // старая версия этой кнопки — включаем обратно.
        foreach (Transform t in FindAll(@"Slab_Backup"))
        {
            if (!t.gameObject.activeSelf)
            {
                Undo.RecordObject(t.gameObject, "Show floor");
                t.gameObject.SetActive(true);
                hidden++;
            }
        }

        Done($"Двери готовы.\nОбычных: {single}\nДвойных пар: {pairs}\nМаятниковых створок: {swing}\nФальшивых: {fake}\nВозвращено полов: {hidden}");
    }

    // ---------- 0. УБРАТЬ ПОТЕРЯННЫЕ СКРИПТЫ ----------
    // Если скрипт удалили и добавили заново, Unity теряет связь с ним,
    // и на объектах остаются пустые компоненты «Missing (Mono Script)».
    [MenuItem("Hospital/0. Remove Missing Scripts")]
    static void RemoveMissingMenu()
    {
        int n = RemoveMissingScripts();
        EditorUtility.DisplayDialog("Hospital", n > 0
            ? $"Убрано потерянных скриптов: {n}.\nТеперь запусти Hospital → 2. Setup Elevator (и 1. Setup Doors, если двери тоже не открываются)."
            : "Потерянных скриптов не найдено.", "OK");
    }

    static int RemoveMissingScripts()
    {
        int removed = 0;
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                int c = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                if (c == 0) continue;
                Undo.RegisterCompleteObjectUndo(t.gameObject, "Remove missing scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                removed += c;
            }
        }
        if (removed > 0) UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        return removed;
    }

    // ---------- 2. ЛИФТ ----------
    [MenuItem("Hospital/2. Setup Elevator")]
    static void SetupElevator()
    {
        RemoveMissingScripts();
        int floors = 3;
        var heights = new List<float>();
        for (int f = 1; f <= floors; f++)
        {
            Transform spawn = FindOne($"F{f}_LiftSpawn");
            if (spawn == null)
            {
                EditorUtility.DisplayDialog("Hospital", $"Не нашла F{f}_LiftSpawn. Модель больницы есть на сцене?", "OK");
                return;
            }
            heights.Add((float)System.Math.Round(spawn.position.y, 3));
        }

        Transform elevT = FindOne("Elevator");
        GameObject elevGo;
        if (elevT == null)
        {
            elevGo = new GameObject("Elevator");
            Undo.RegisterCreatedObjectUndo(elevGo, "Elevator");
        }
        else elevGo = elevT.gameObject;

        FakeElevator elev = GetOrAdd<FakeElevator>(elevGo);
        Undo.RecordObject(elev, "Elevator");
        elev.floorHeights = heights.ToArray();
        elev.doors = new FakeElevator.LiftDoors[floors];

        int buttons = 0;
        for (int f = 1; f <= floors; f++)
        {
            var pair = new FakeElevator.LiftDoors
            {
                left = FindOne($"F{f}_LiftDoor_L"),
                right = FindOne($"F{f}_LiftDoor_R")
            };
            elev.doors[f - 1] = pair;
            if (pair.left != null) MarkNotStatic(pair.left.gameObject);
            if (pair.right != null) MarkNotStatic(pair.right.gameObject);

            // Кнопки этажей в кабине
            for (int b = 1; b <= floors; b++)
            {
                Transform bt = FindOne($"F{f}_LiftPanelButton_{b}");
                if (bt == null) continue;
                UseBoxCollider(bt.gameObject, 0.08f); // кнопки крошечные — делаем попадание чуть больше
                FakeElevatorButton btn = GetOrAdd<FakeElevatorButton>(bt.gameObject);
                Undo.RecordObject(btn, "Button");
                btn.elevator = elev; btn.floor = b - 1; btn.isCallButton = false;
                buttons++;
            }

            // Кнопка вызова снаружи
            Transform call = FindOne($"F{f}_LiftCallButton");
            if (call != null)
            {
                UseBoxCollider(call.gameObject, 0.08f);
                FakeElevatorButton btn = GetOrAdd<FakeElevatorButton>(call.gameObject);
                Undo.RecordObject(btn, "Button");
                btn.elevator = elev; btn.floor = f - 1; btn.isCallButton = true;
                buttons++;
            }
        }

        Done($"Лифт готов.\nВысоты этажей: {string.Join(" / ", heights)}\nКнопок настроено: {buttons}");
    }

    // ---------- 3. СВЕТ ----------
    [MenuItem("Hospital/3. Add Lights")]
    static void AddLights()
    {
        if (FindOne("Hospital_Lights") != null)
        {
            EditorUtility.DisplayDialog("Hospital", "Свет уже добавлен (есть объект Hospital_Lights). Удали его, если хочешь поставить заново.", "OK");
            return;
        }

        GameObject root = new GameObject("Hospital_Lights");
        Undo.RegisterCreatedObjectUndo(root, "Lights");
        int total = 0;

        for (int f = 1; f <= 3; f++)
        {
            total += LightsFor($"F{f}_CeilingLamps", root.transform, $"F{f}_Corridor", 7f, 1.2f, new Color(0.85f, 0.92f, 1f), 5);
            total += LightsFor($"F{f}_RoomLamps", root.transform, $"F{f}_Rooms", 5f, 0.9f, new Color(1f, 0.93f, 0.8f), 0);
            total += LightsFor($"F{f}_LiftCeilingLamp", root.transform, $"F{f}_Lift", 3f, 1f, Color.white, 0);
        }

        Done($"Света поставлено: {total}.\nОни лежат в объекте Hospital_Lights, по этажам.\nКаждая 5-я лампа в коридорах мигает.");
    }

    // Находит отдельные лампы внутри общего меша и ставит под каждой Point Light
    static int LightsFor(string meshName, Transform root, string groupName, float range, float intensity, Color color, int flickerEvery)
    {
        Transform t = FindOne(meshName);
        if (t == null) return 0;
        MeshFilter mf = t.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return 0;

        List<Vector3> centers = FindIslands(mf.sharedMesh, t);
        if (centers.Count == 0) return 0;

        GameObject group = new GameObject(groupName);
        Undo.RegisterCreatedObjectUndo(group, "Lights");
        group.transform.SetParent(root, false);

        int i = 0;
        foreach (Vector3 c in centers)
        {
            GameObject lg = new GameObject($"{groupName}_Light_{i + 1:00}");
            Undo.RegisterCreatedObjectUndo(lg, "Lights");
            lg.transform.SetParent(group.transform, false);
            lg.transform.position = c + Vector3.down * 0.2f;
            Light l = lg.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = range;
            l.intensity = intensity;
            l.color = color;
            l.shadows = LightShadows.None;
            if (flickerEvery > 0 && (i + 1) % flickerEvery == 0) lg.AddComponent<FlickeringLight>();
            i++;
        }
        return i;
    }

    // Разбивает меш на отдельные куски (острова) и возвращает их центры в мире
    static List<Vector3> FindIslands(Mesh mesh, Transform t)
    {
        Vector3[] v = mesh.vertices;
        int[] tris = mesh.triangles;

        // Вершины в одной точке считаем одной (в Blender они часто продублированы)
        var keyOf = new Dictionary<Vector3Int, int>();
        int[] id = new int[v.Length];
        for (int i = 0; i < v.Length; i++)
        {
            Vector3Int k = Vector3Int.RoundToInt(v[i] * 1000f);
            if (!keyOf.TryGetValue(k, out int n)) { n = keyOf.Count; keyOf[k] = n; }
            id[i] = n;
        }

        int[] parent = new int[keyOf.Count];
        for (int i = 0; i < parent.Length; i++) parent[i] = i;
        int Find(int a) { while (parent[a] != a) { parent[a] = parent[parent[a]]; a = parent[a]; } return a; }
        void Union(int a, int b) { a = Find(a); b = Find(b); if (a != b) parent[a] = b; }

        for (int i = 0; i + 2 < tris.Length; i += 3)
        {
            Union(id[tris[i]], id[tris[i + 1]]);
            Union(id[tris[i]], id[tris[i + 2]]);
        }

        var bounds = new Dictionary<int, Bounds>();
        for (int i = 0; i < v.Length; i++)
        {
            Vector3 w = t.TransformPoint(v[i]);
            int r = Find(id[i]);
            if (bounds.TryGetValue(r, out Bounds b)) { b.Encapsulate(w); bounds[r] = b; }
            else bounds[r] = new Bounds(w, Vector3.zero);
        }

        // Корпус лампы и светящаяся часть — разные острова в одном месте: объединяем близкие
        var centers = new List<Vector3>();
        foreach (Bounds b in bounds.Values)
        {
            bool merged = false;
            for (int i = 0; i < centers.Count; i++)
            {
                if (Vector3.Distance(centers[i], b.center) < 0.6f) { merged = true; break; }
            }
            if (!merged) centers.Add(b.center);
        }
        return centers;
    }
}
