using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("管理器预制体")]
    public GameObject saveManagerPrefab;
    
    private void Awake()
    {
        // 检查并创建SaveManager
        if (SaveManager.Instance == null && saveManagerPrefab != null)
        {
            Instantiate(saveManagerPrefab);
            Debug.Log("SaveManager已创建");
        }
    }
    
    // 这个方法可以在主菜单场景的Start Game按钮调用
    public static void EnsureManagersExist()
    {
        if (SaveManager.Instance == null)
        {
            GameObject saveManagerObj = new GameObject("SaveManager");
            saveManagerObj.AddComponent<SaveManager>();
            Debug.Log("SaveManager已动态创建");
        }
    }
} 