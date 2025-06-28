using UnityEngine;

/// <summary>
/// 音频系统集成测试脚本
/// 用于验证所有攻击音效都正确集成了AudioVolumeManager
/// </summary>
public class AudioIntegrationTest : MonoBehaviour
{
    [Header("测试对象")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject playerPrefab;
    
    private AudioVolumeManager audioVolumeManager;
    
    void Start()
    {
        // 查找音量管理器
        audioVolumeManager = FindFirstObjectByType<AudioVolumeManager>();
        
        if (audioVolumeManager == null)
        {
            Debug.LogError("❌ AudioVolumeManager 未找到！");
            return;
        }
        
        Debug.Log("✅ AudioVolumeManager 找到，开始测试音频集成...");
        
        // 测试各个组件
        TestEnemyAudioIntegration();
        TestBossAudioIntegration();
        TestPlayerAudioIntegration();
        TestMagicBulletAudioIntegration();
        
        Debug.Log("🎵 音频系统集成测试完成！");
    }
    
    void TestEnemyAudioIntegration()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Debug.Log($"🔍 找到 {enemies.Length} 个Enemy对象");
        
        foreach (Enemy enemy in enemies)
        {
            // 检查是否有AudioVolumeManager引用
            var audioManagerField = typeof(Enemy).GetField("audioVolumeManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (audioManagerField != null)
            {
                var audioManager = audioManagerField.GetValue(enemy) as AudioVolumeManager;
                if (audioManager != null)
                {
                    Debug.Log($"✅ Enemy '{enemy.name}' - AudioVolumeManager 集成正常");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Enemy '{enemy.name}' - AudioVolumeManager 为null");
                }
            }
        }
    }
    
    void TestBossAudioIntegration()
    {
        BossController[] bosses = FindObjectsOfType<BossController>();
        Debug.Log($"🔍 找到 {bosses.Length} 个BossController对象");
        
        foreach (BossController boss in bosses)
        {
            var audioManagerField = typeof(BossController).GetField("audioVolumeManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (audioManagerField != null)
            {
                var audioManager = audioManagerField.GetValue(boss) as AudioVolumeManager;
                if (audioManager != null)
                {
                    Debug.Log($"✅ BossController '{boss.name}' - AudioVolumeManager 集成正常");
                }
                else
                {
                    Debug.LogWarning($"⚠️ BossController '{boss.name}' - AudioVolumeManager 为null");
                }
            }
        }
    }
    
    void TestPlayerAudioIntegration()
    {
        PlayerAttackSystem[] players = FindObjectsOfType<PlayerAttackSystem>();
        Debug.Log($"🔍 找到 {players.Length} 个PlayerAttackSystem对象");
        
        foreach (PlayerAttackSystem player in players)
        {
            var audioManagerField = typeof(PlayerAttackSystem).GetField("audioVolumeManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (audioManagerField != null)
            {
                var audioManager = audioManagerField.GetValue(player) as AudioVolumeManager;
                if (audioManager != null)
                {
                    Debug.Log($"✅ PlayerAttackSystem '{player.name}' - AudioVolumeManager 集成正常");
                }
                else
                {
                    Debug.LogWarning($"⚠️ PlayerAttackSystem '{player.name}' - AudioVolumeManager 为null");
                }
            }
        }
    }
    
    void TestMagicBulletAudioIntegration()
    {
        MagicBulletSkill[] magicBullets = FindObjectsOfType<MagicBulletSkill>();
        Debug.Log($"🔍 找到 {magicBullets.Length} 个MagicBulletSkill对象");
        
        foreach (MagicBulletSkill magicBullet in magicBullets)
        {
            var audioManagerField = typeof(MagicBulletSkill).GetField("audioVolumeManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (audioManagerField != null)
            {
                var audioManager = audioManagerField.GetValue(magicBullet) as AudioVolumeManager;
                if (audioManager != null)
                {
                    Debug.Log($"✅ MagicBulletSkill '{magicBullet.name}' - AudioVolumeManager 集成正常");
                }
                else
                {
                    Debug.LogWarning($"⚠️ MagicBulletSkill '{magicBullet.name}' - AudioVolumeManager 为null");
                }
            }
        }
    }
    
    void Update()
    {
        // 按T键测试音量变化
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (audioVolumeManager != null)
            {
                float currentVolume = audioVolumeManager.GetCurrentVolume();
                Debug.Log($"🎵 当前音量: {currentVolume * 100}%");
                
                // 测试音量变化
                audioVolumeManager.SetVolume(currentVolume > 0.5f ? 0.2f : 0.8f);
                Debug.Log($"🔄 音量已调整到: {audioVolumeManager.GetCurrentVolume() * 100}%");
            }
        }
        
        // 按M键测试静音
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (audioVolumeManager != null)
            {
                audioVolumeManager.ToggleMute();
                bool isMuted = audioVolumeManager.IsMuted();
                Debug.Log($"🔇 静音状态: {(isMuted ? "开启" : "关闭")}");
            }
        }
    }
}
