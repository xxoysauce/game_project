using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    public OpenAIConnector apiConnector;

    public float interactDistance = 2500f;
    public Transform playerOverride;
    Transform player;


    [Header("NPC 설정")]
    public string npcName = "토끼님";    
    [TextArea(3, 6)]
    public string npcPersona = "당신은 따뜻한 마을 촌장입니다."; 

    void Start()
    {
        player = playerOverride ? playerOverride : GameObject.FindWithTag("Player")?.transform;
        if (!player) Debug.LogWarning($"[{name}] player Transform을 찾지 못했어요. 'Player' 태그를 확인하세요.");

        if (apiConnector == null)
            apiConnector = FindObjectOfType<OpenAIConnector>();
        if (apiConnector == null)
            Debug.LogError("[NPCInteract] OpenAIConnector를 찾을 수 없습니다! ApiManager에 스크립트가 붙어있는지 확인하세요.");
    }

    void Update()
    {
        if (!player) return;

        Vector3 a = player.position; a.y = 0f;
        Vector3 b = transform.position; b.y = 0f;
        float planarDist = Vector3.Distance(a, b);

        if (planarDist <= interactDistance)
        {


            if (Input.GetKeyDown(KeyCode.E) && apiConnector != null && !apiConnector.IsDialogueActive)
            {
 
 
                apiConnector.SetNpcProfile(npcName, npcPersona);

                apiConnector.StartDialogue();
            }



            if (Input.GetKeyDown(KeyCode.Return) && apiConnector != null && apiConnector.IsDialogueActive)
            {
                if (apiConnector.awaitingUserSelection)
                {
                    apiConnector.OnClickNext();
                }
            }
        }
    }
}
