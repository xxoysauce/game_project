using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    [Header("아이템 이름 (예: 사과, 바나나, 배, 센베)")]
    public string itemName = "사과";  // Inspector에서 바꿀 수 있게

    private void Awake()
    {
        // 🟢 콜라이더 자동 설정
        Collider col = GetComponent<Collider>();
        col.isTrigger = true; // 반드시 Trigger여야 함

        // 🟢 Rigidbody 안정화
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print($"[Coin] {itemName} 충돌 감지됨 with {other.name}");
            CollectCoin();
        }
    }

    private void CollectCoin()
    {
        // 🟢 퀘스트 매니저 연결
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddItem(itemName);
        }

        // 🟢 사운드/이펙트 등 나중에 추가 가능
        Destroy(gameObject);
    }
}
