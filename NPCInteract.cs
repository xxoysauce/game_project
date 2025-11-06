using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    public OpenAIConnector apiConnector;

    public float interactDistance = 2500f;
    public Transform playerOverride;
    Transform player;

    // 👇 여기부터 NPC 개별 설정
    [Header("NPC 설정")]
    public string npcName = "토끼님";     // 화면에 찍힐 이름
    [TextArea(3, 6)]
    public string npcPersona = "당신은 따뜻한 마을 촌장입니다.";  // LLM에 넣을 system 프롬프트 조각

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
            // E로 대화 시작
            if (Input.GetKeyDown(KeyCode.E) && apiConnector != null && !apiConnector.IsDialogueActive)
            {
                // ⭐️ 이 NPC의 프로필을 먼저 알려준다
                apiConnector.SetNpcProfile(npcName, npcPersona);

                apiConnector.StartDialogue();
            }

            // Enter로 첫 턴
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
