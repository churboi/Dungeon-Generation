using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGeneration))]
public class DunGenEditor : Editor
{ 
    public override void OnInspectorGUI()
    {

        DungeonGeneration DunGen = (DungeonGeneration)target;

        if (GUI.changed) { EditorUtility.SetDirty(DunGen); }

        base.OnInspectorGUI();

        DunGen.debugBuildMessages = EditorGUILayout.BeginToggleGroup("Debug Messages", DunGen.debugBuildMessages);
        DunGen.pendingNodesShow = EditorGUILayout.Toggle("Pending Node", DunGen.pendingNodesShow);
        DunGen.newExitNodesShow = EditorGUILayout.Toggle("New Exit Nodes", DunGen.newExitNodesShow);
        DunGen.nodeSubtraction = EditorGUILayout.Toggle("Node Subtraction", DunGen.nodeSubtraction);
        DunGen.wallBlockerShow = EditorGUILayout.Toggle("Wall Blocking", DunGen.wallBlockerShow);
        DunGen.DEFCShow = EditorGUILayout.Toggle("Dead end frequency correction", DunGen.DEFCShow);
        DunGen.clusterShow = EditorGUILayout.Toggle("Cluster information", DunGen.clusterShow);
        EditorGUILayout.EndToggleGroup();
    }
}
