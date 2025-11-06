using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("현재 퀘스트 상태")]
    public bool hasActiveQuest = false;   // 지금 진행 중인 퀘스트가 있는가
    public string targetItem = "";        // "사과", "바나나", "배", "센베"
    public int requiredCount = 3;         // 몇 개를 가져오라고 했나
    public int currentCount = 0;          // 지금까지 플레이어가 주운 개수

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // NPC가 퀘스트를 줬다고 판단될 때 호출
    public void RegisterQuest(string itemName, int count = 3)
    {
        hasActiveQuest = true;
        targetItem = itemName;
        requiredCount = count;
        currentCount = 0;
        Debug.Log($"[QuestManager] 퀘스트 등록: {targetItem} {requiredCount}개");
    }

    // 플레이어가 아이템을 주웠을 때 호출
    public void AddItem(string itemName)
    {
        // 진행 중인 퀘스트가 있고, 그 아이템이 맞을 때만 카운트
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
