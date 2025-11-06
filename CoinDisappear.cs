using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    [Header("아이템 이름 (예: 사과, 바나나, 배, 센베)")]
    public string itemName = "사과"; 

    private void Awake()
    {

        Collider col = GetComponent<Collider>();
        col.isTrigger = true; // 반드시 Trigger여야 함


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

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddItem(itemName);
        }


        Destroy(gameObject);
    }
}
