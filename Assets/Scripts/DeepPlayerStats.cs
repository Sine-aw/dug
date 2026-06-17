using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    public int playCount = 0;       // 누적 플레이 횟수
    public int totalKills = 0;      // 누적 처치 수
    public int totalGold = 0;       // 누적 골드
    public int permanentUpgrade = 0; // 영구 강화 수치
}

public class DeepPlayerStats : MonoBehaviour
{
    [Header("체력 설정")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI 연동 (체력 바)")]
    public Image hpBarFill;

    [Header("게임 데이터 (사망 시 보존용)")]
    public int currentGold = 0;
    public int currentKills = 0;

    private SaveData saveData = new SaveData();
    private string saveFilePath;
    private UIManager uiManager;

    void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "savedata.json");
        LoadGameData();
    }

    void Start()
    {
        currentHealth = maxHealth;

        uiManager = FindAnyObjectByType<UIManager>();  // 한 번만 찾기

        UpdateHPBar();
        saveData.playCount++;
        SaveGameData();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHPBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHPBar();
    }

    void UpdateHPBar()
    {
        if (hpBarFill != null)
            hpBarFill.fillAmount = currentHealth / maxHealth;

        if (uiManager != null)
            uiManager.SetHealth(currentHealth, maxHealth);
    }

    void Die()
    {
        saveData.totalKills += currentKills;
        saveData.totalGold += currentGold;
        SaveGameData();
        RestartGame();
    }

    void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void AddKill()
    {
        currentKills++;
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
    }

    public void SaveGameData()
    {
        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(saveFilePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"데이터 저장 실패: {e.Message}");
        }
    }

    public void LoadGameData()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                saveData = JsonUtility.FromJson<SaveData>(json);
            }
            catch (System.Exception e)
            {
                saveData = new SaveData();
            }
        }
        else
        {
            saveData = new SaveData();
        }
    }
}