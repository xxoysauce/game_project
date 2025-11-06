using UnityEngine;

public class PlayerGiftGiver : MonoBehaviour
{
    public OpenAIConnector connector;     // 인스펙터에서 연결
    public float interactRange = 3f;      // NPC에게 줄 수 있는 거리

    void Update()
    {
        // 커넥터가 없으면 아무것도 안 함
        if (connector == null) return;

        // 대화 열려 있을 때만 선물 주고 싶으면 이거 유지
        if (!connector.IsDialogueActive) return;

        // Z키: 좋아하는 선물
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (CheckNpcInFront())
            {
                connector.OnGiftGiven("사과", true);
            }
        }

        // X키: 별로인 선물
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (CheckNpcInFront())
            {
                connector.OnGiftGiven("과자", false);   // ← 깨끗한 문자열로 수정
            }
        }
    }

    // NPC가 앞에 있는지 검사
    bool CheckNpcInFront()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 1f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            if (hit.collider.CompareTag("NPC"))
            {
                Debug.Log($"[PlayerGiftGiver] NPC 감지됨: {hit.collider.name}");
                return true;
            }
        }
        return false;
    }
}
