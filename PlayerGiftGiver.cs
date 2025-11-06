using UnityEngine;

public class PlayerGiftGiver : MonoBehaviour
{
    public OpenAIConnector connector;   
    public float interactRange = 3f;      

    void Update()
    {
        
        if (connector == null) return;

        
        if (!connector.IsDialogueActive) return;

        
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (CheckNpcInFront())
            {
                connector.OnGiftGiven("사과", true);
            }
        }

        
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (CheckNpcInFront())
            {
                connector.OnGiftGiven("과자", false);  
            }
        }
    }

    
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
