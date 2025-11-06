using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("현재 퀘스트 상태")]
    public bool hasActiveQuest = false;   
    public string targetItem = "";        
    public int requiredCount = 3;         
    public int currentCount = 0;          

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

   
    public void RegisterQuest(string itemName, int count = 3)
    {
        hasActiveQuest = true;
        targetItem = itemName;
        requiredCount = count;
        currentCount = 0;
        Debug.Log($"[QuestManager] 퀘스트 등록: {targetItem} {requiredCount}개");
    }

    
    public void AddItem(string itemName)
    {
      
        if (hasActiveQuest && !string.IsNullOrEmpty(targetItem) && itemName == targetItem)
        {
            currentCount++;
            Debug.Log($"[QuestManager] {itemName} 획득: {currentCount}/{requiredCount}");
        }
    }

    public bool IsQuestComplete()
    {
        return hasActiveQuest && currentCount >= requiredCount;
    }

    public void ClearQuest()
    {
        hasActiveQuest = false;
        targetItem = "";
        currentCount = 0;
    }
}
