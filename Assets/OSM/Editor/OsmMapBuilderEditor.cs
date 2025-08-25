#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OsmMapBuilder))]
public class OsmMapBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(8);

        var t = (OsmMapBuilder)target;
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("OSM ▸ Build", GUILayout.Height(28))) t.BuildFromOSM();
            if (GUILayout.Button("OSM ▸ Clear", GUILayout.Height(28))) t.ClearGenerated();
            if (GUILayout.Button("OSM ▸ Bake",  GUILayout.Height(28))) t.Bake();
        }

        GUILayout.Space(6);
        if (GUILayout.Button("OSM ▸ Scan Tags", GUILayout.Height(24)))
            t.ScanTags(20);

        GUILayout.Space(6);
        EditorGUILayout.HelpBox(
            "Build: 생성 · Clear: 제거 · Bake: 고정(태그/내부 목록 비움)\n" +
            "Scan Tags: OSM 태그 분포를 콘솔에 출력합니다.",
            MessageType.Info);
    }
}
#endif
