using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VolumeButton))]
public class VolumeButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        VolumeButton volumeButton = (VolumeButton)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("快速设置", EditorStyles.boldLabel);
        
        if (GUILayout.Button("自动查找并设置AudioVolumeManager"))
        {
            AudioVolumeManager audioManager = FindObjectOfType<AudioVolumeManager>();
            if (audioManager != null)
            {
                SerializedProperty audioManagerProp = serializedObject.FindProperty("audioVolumeManager");
                audioManagerProp.objectReferenceValue = audioManager;
                serializedObject.ApplyModifiedProperties();
                Debug.Log("已找到并设置AudioVolumeManager!");
            }
            else
            {
                Debug.LogWarning("场景中未找到AudioVolumeManager!");
            }
        }
        
        if (GUILayout.Button("加载默认音量图标"))
        {
            // 尝试从项目中加载音量图标
            string[] volumeOnPaths = AssetDatabase.FindAssets("volume t:Sprite");
            string[] volumeOffPaths = AssetDatabase.FindAssets("mute t:Sprite");
            
            if (volumeOnPaths.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(volumeOnPaths[0]);
                Sprite volumeOnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                SerializedProperty volumeOnProp = serializedObject.FindProperty("volumeOnIcon");
                volumeOnProp.objectReferenceValue = volumeOnSprite;
            }
            
            if (volumeOffPaths.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(volumeOffPaths[0]);
                Sprite volumeOffSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                SerializedProperty volumeOffProp = serializedObject.FindProperty("volumeOffIcon");
                volumeOffProp.objectReferenceValue = volumeOffSprite;
            }
            
            serializedObject.ApplyModifiedProperties();
            Debug.Log("已尝试加载默认图标!");
        }
    }
} 