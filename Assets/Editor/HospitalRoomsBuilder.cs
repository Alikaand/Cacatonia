using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Расставляет стены кабинетов (перегородки и стены коридоров с дверными проёмами).
// Положи этот файл в папку Assets/Editor. В Unity появится меню Hospital вверху.
// Нужно, чтобы на сцене уже были пустые объекты Floor_1 / Floor_2 / Floor_3.
public static class HospitalRoomsBuilder
{
    struct W
    {
        public string n; public Vector3 p; public Vector3 s;
        public W(string n, float px, float py, float pz, float sx, float sy, float sz)
        { this.n = n; p = new Vector3(px, py, pz); s = new Vector3(sx, sy, sz); }
    }

    static readonly W[] Floor1 =
    {
        new W("R1_N1", -17.25f, 1.65f, 7.5f, 5.5f, 3.3f, 0.2f),
        new W("R1_N2", -12.25f, 1.65f, 7.5f, 2.5f, 3.3f, 0.2f),
        new W("R1_N3", -7f, 1.65f, 7.5f, 4f, 3.3f, 0.2f),
        new W("R1_N4", -2.5f, 1.65f, 7.5f, 3f, 3.3f, 0.2f),
        new W("R1_NE1", 16.5f, 1.65f, 7.5f, 2f, 3.3f, 0.2f),
        new W("R1_NE2", 19.25f, 1.65f, 7.5f, 1.5f, 3.3f, 0.2f),
        new W("R1_NP1", -12f, 1.65f, 9.75f, 0.2f, 3.3f, 4.5f),
        new W("R1_NP2", -8f, 1.65f, 9.75f, 0.2f, 3.3f, 4.5f),
        new W("R1_NP3", -1f, 1.65f, 9.75f, 0.2f, 3.3f, 4.5f),
        new W("R1_W1", -15.5f, 1.65f, -32f, 0.2f, 3.3f, 3f),
        new W("R1_W2", -15.5f, 1.65f, -26.75f, 0.2f, 3.3f, 5.5f),
        new W("R1_W3", -15.5f, 1.65f, -20.25f, 0.2f, 3.3f, 5.5f),
        new W("R1_W4", -15.5f, 1.65f, -13.5f, 0.2f, 3.3f, 6f),
        new W("R1_W5", -15.5f, 1.65f, -6.5f, 0.2f, 3.3f, 6f),
        new W("R1_W6", -15.5f, 1.65f, -0.5f, 0.2f, 3.3f, 4f),
        new W("R1_W7", -15.5f, 1.65f, 3.75f, 0.2f, 3.3f, 2.5f),
        new W("R1_W8", -15.5f, 1.65f, 6.75f, 0.2f, 3.3f, 1.5f),
        new W("R1_WP1", -17.75f, 1.65f, 4f, 4.5f, 3.3f, 0.2f),
        new W("R1_WP2", -17.75f, 1.65f, 0.5f, 4.5f, 3.3f, 0.2f),
        new W("R1_WP3", -17.75f, 1.65f, -6.5f, 4.5f, 3.3f, 0.2f),
        new W("R1_WP4", -17.75f, 1.65f, -13.5f, 4.5f, 3.3f, 0.2f),
        new W("R1_WP5", -17.75f, 1.65f, -20.5f, 4.5f, 3.3f, 0.2f),
        new W("R1_WP6", -17.75f, 1.65f, -27f, 4.5f, 3.3f, 0.2f),
        new W("R1_E1", -12.5f, 1.65f, -32.25f, 0.2f, 3.3f, 2.5f),
        new W("R1_E2", -12.5f, 1.65f, -27.75f, 0.2f, 3.3f, 4.5f),
        new W("R1_E3", -12.5f, 1.65f, -22.25f, 0.2f, 3.3f, 4.5f),
        new W("R1_E4", -12.5f, 1.65f, -18f, 0.2f, 3.3f, 2f),
        new W("R1_E5", -12.5f, 1.65f, -11.75f, 0.2f, 3.3f, 2.5f),
        new W("R1_E6", -12.5f, 1.65f, -7.25f, 0.2f, 3.3f, 4.5f),
        new W("R1_E7", -12.5f, 1.65f, -2f, 0.2f, 3.3f, 4f),
        new W("R1_E8", -12.5f, 1.65f, 2.25f, 0.2f, 3.3f, 2.5f),
        new W("R1_EP1", -10.25f, 1.65f, 3.5f, 4.5f, 3.3f, 0.2f),
        new W("R1_EP2", -10.25f, 1.65f, -2f, 4.5f, 3.3f, 0.2f),
        new W("R1_EP3", -10.25f, 1.65f, -7.5f, 4.5f, 3.3f, 0.2f),
        new W("R1_EP4", -10.25f, 1.65f, -13f, 4.5f, 3.3f, 0.2f),
        new W("R1_EP5", -10.25f, 1.65f, -17f, 4.5f, 3.3f, 0.2f),
        new W("R1_EP6", -10.25f, 1.65f, -22.5f, 4.5f, 3.3f, 0.2f),
        new W("R1_EP7", -10.25f, 1.65f, -28f, 4.5f, 3.3f, 0.2f),
        new W("Reception", 4f, 0.55f, 5f, 4f, 1.1f, 0.8f)
    };

    static readonly W[] Upper =
    {
        new W("R2_N1", -17.25f, 1.65f, 7.5f, 5.5f, 3.3f, 0.2f),
        new W("R2_N2", -12.25f, 1.65f, 7.5f, 2.5f, 3.3f, 0.2f),
        new W("R2_N3", -7.5f, 1.65f, 7.5f, 3f, 3.3f, 0.2f),
        new W("R2_N4", -2.5f, 1.65f, 7.5f, 5f, 3.3f, 0.2f),
        new W("R2_N5", 3.75f, 1.65f, 7.5f, 5.5f, 3.3f, 0.2f),
        new W("R2_N6", 8.5f, 1.65f, 7.5f, 2f, 3.3f, 0.2f),
        new W("R2_NE1", 16.5f, 1.65f, 7.5f, 2f, 3.3f, 0.2f),
        new W("R2_NE2", 19.25f, 1.65f, 7.5f, 1.5f, 3.3f, 0.2f),
        new W("R2_NP1", -12f, 1.65f, 9.75f, 0.2f, 3.3f, 4.5f),
        new W("R2_NP2", -8f, 1.65f, 9.75f, 0.2f, 3.3f, 4.5f),
        new W("R2_NP3", -3.5f, 1.65f, 9.75f, 0.2f, 3.3f, 4.5f),
        new W("R2_NP4", 4f, 1.65f, 9.75f, 0.2f, 3.3f, 4.5f),
        new W("R2_W1", -15.5f, 1.65f, -33f, 0.2f, 3.3f, 1f),
        new W("R2_W2", -15.5f, 1.65f, -30.25f, 0.2f, 3.3f, 2.5f),
        new W("R2_W3", -15.5f, 1.65f, -26.75f, 0.2f, 3.3f, 2.5f),
        new W("R2_W4", -15.5f, 1.65f, -23.25f, 0.2f, 3.3f, 2.5f),
        new W("R2_W5", -15.5f, 1.65f, -19.75f, 0.2f, 3.3f, 2.5f),
        new W("R2_W6", -15.5f, 1.65f, -16.5f, 0.2f, 3.3f, 2f),
        new W("R2_W7", -15.5f, 1.65f, -13.75f, 0.2f, 3.3f, 1.5f),
        new W("R2_W8", -15.5f, 1.65f, -10.75f, 0.2f, 3.3f, 2.5f),
        new W("R2_W9", -15.5f, 1.65f, -7.25f, 0.2f, 3.3f, 2.5f),
        new W("R2_W10", -15.5f, 1.65f, -3.75f, 0.2f, 3.3f, 2.5f),
        new W("R2_W11", -15.5f, 1.65f, 0f, 0.2f, 3.3f, 3f),
        new W("R2_W12", -15.5f, 1.65f, 3.75f, 0.2f, 3.3f, 2.5f),
        new W("R2_W13", -15.5f, 1.65f, 6.75f, 0.2f, 3.3f, 1.5f),
        new W("R2_WP1", -17.75f, 1.65f, 4f, 4.5f, 3.3f, 0.2f),
        new W("R2_WP2", -17.75f, 1.65f, 0.5f, 4.5f, 3.3f, 0.2f),
        new W("R2_WP3", -17.75f, 1.65f, -3.5f, 4.5f, 3.3f, 0.2f),
        new W("R2_WP4", -17.75f, 1.65f, -7f, 4.5f, 3.3f, 0.2f),
        new W("R2_WP5", -17.75f, 1.65f, -10.5f, 4.5f, 3.3f, 0.2f),
        new W("R2_WP6", -17.75f, 1.65f, -14f, 4.5f, 3.3f, 0.2f),
        new W("R2_WP7", -17.75f, 1.65f, -16.5f, 4.5f, 3.3f, 0.2f),
        new W("R2_WP8", -17.75f, 1.65f, -19.5f, 4.5f, 3.3f, 0.2f),
        new W("R2_WP9", -17.75f, 1.65f, -23f, 4.5f, 3.3f, 0.2f),
        new W("R2_WP10", -17.75f, 1.65f, -26.5f, 4.5f, 3.3f, 0.2f),
        new W("R2_WP11", -17.75f, 1.65f, -30f, 4.5f, 3.3f, 0.2f),
        new W("R2_E1", -12.5f, 1.65f, -33f, 0.2f, 3.3f, 1f),
        new W("R2_E2", -12.5f, 1.65f, -30.25f, 0.2f, 3.3f, 2.5f),
        new W("R2_E3", -12.5f, 1.65f, -26.75f, 0.2f, 3.3f, 2.5f),
        new W("R2_E4", -12.5f, 1.65f, -23.25f, 0.2f, 3.3f, 2.5f),
        new W("R2_E5", -12.5f, 1.65f, -19f, 0.2f, 3.3f, 4f),
        new W("R2_E6", -12.5f, 1.65f, -14.5f, 0.2f, 3.3f, 3f),
        new W("R2_E7", -12.5f, 1.65f, -10.75f, 0.2f, 3.3f, 2.5f),
        new W("R2_E8", -12.5f, 1.65f, -7.25f, 0.2f, 3.3f, 2.5f),
        new W("R2_E9", -12.5f, 1.65f, -3.75f, 0.2f, 3.3f, 2.5f),
        new W("R2_E10", -12.5f, 1.65f, -0.5f, 0.2f, 3.3f, 2f),
        new W("R2_EP1", -10.25f, 1.65f, 0.5f, 4.5f, 3.3f, 0.2f),
        new W("R2_EP2", -10.25f, 1.65f, -3.5f, 4.5f, 3.3f, 0.2f),
        new W("R2_EP3", -10.25f, 1.65f, -7f, 4.5f, 3.3f, 0.2f),
        new W("R2_EP4", -10.25f, 1.65f, -10.5f, 4.5f, 3.3f, 0.2f),
        new W("R2_EP5", -10.25f, 1.65f, -14f, 4.5f, 3.3f, 0.2f),
        new W("R2_EP6", -10.25f, 1.65f, -19.5f, 4.5f, 3.3f, 0.2f),
        new W("R2_EP7", -10.25f, 1.65f, -23f, 4.5f, 3.3f, 0.2f),
        new W("R2_EP8", -10.25f, 1.65f, -26.5f, 4.5f, 3.3f, 0.2f),
        new W("R2_EP9", -10.25f, 1.65f, -30f, 4.5f, 3.3f, 0.2f)
    };

    [MenuItem("Hospital/Build Rooms - Floor 1")]
    static void BuildFloor1()
    {
        Build("Floor_1", Floor1);
    }

    [MenuItem("Hospital/Build Rooms - Floors 2 and 3")]
    static void BuildUpper()
    {
        Build("Floor_2", Upper);
        Build("Floor_3", Upper);
    }

    static void Build(string floorName, W[] walls)
    {
        GameObject floor = GameObject.Find(floorName);
        if (floor == null)
        {
            Debug.LogWarning("Не нашла объект " + floorName + " на сцене — пропускаю.");
            return;
        }
        if (floor.transform.Find("Rooms") != null)
        {
            Debug.LogWarning("В " + floorName + " уже есть Rooms — второй раз не ставлю.");
            return;
        }

        GameObject rooms = new GameObject("Rooms");
        Undo.RegisterCreatedObjectUndo(rooms, "Build Rooms");
        rooms.transform.SetParent(floor.transform, false);

        string matPath = floorName == "Floor_3" ? "Assets/Materials/Burned.mat" : "Assets/Materials/Wall.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        foreach (W w in walls)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = w.n;
            Undo.RegisterCreatedObjectUndo(go, "Build Rooms");
            go.transform.SetParent(rooms.transform, false);
            go.transform.localPosition = w.p;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = w.s;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        EditorSceneManager.MarkSceneDirty(floor.scene);
        Debug.Log(floorName + ": поставлено объектов: " + walls.Length);
    }
}
