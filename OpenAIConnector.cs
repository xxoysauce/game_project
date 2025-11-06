using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// ⭐️ 파일 이름: OpenAIConnector.cs
public class OpenAIConnector : MonoBehaviour
{
    private const string OpenAIApiUrl = "https://api.openai.com/v1/chat/completions";

    [Header("OpenAI API 설정")]
    [SerializeField] private OpenAIConfig config;

    // 대화 히스토리
    private List<ChatMessage> conversationHistory = new List<ChatMessage>();

    // ----------------------------------------------------------------------
    // UI
    // ----------------------------------------------------------------------
    [Header("UI 및 게임 설정")]
    public TextMeshProUGUI responseText;
    public GameObject optionButtonsContainer;
    public GameObject nextButton;
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;

    [Header("옵션 버튼 요소")]
    public TextMeshProUGUI optionAText;
    public TextMeshProUGUI optionBText;
    public ButtonHandler optionAButton;
    public ButtonHandler optionBButton;

    // 상태
    public bool IsDialogueActive { get; private set; } = false;
    [HideInInspector] public bool awaitingUserSelection = false;
    [HideInInspector] public bool isTyping = false;

    // ----------------------------------------------------------------------
    // NPC / 플레이어 프로필
    // ----------------------------------------------------------------------
    private string currentNpcName = "";
    private string currentNpcPersona =
        "";

    private string BuildDefaultSystemPrompt()
    {
        return
            "제공된 NPC의 말투와 제공된 플레이어의 말투를 존중해서 대화를 자연스럽게 이어나가세요. 하지만 플레이어가 말을 걸었을 때(입력이 들어왔을 때) NPC의 첫 마디는 무조건 안부인사여야 합니다. 그 이후는 NPC의 말투를 존중하여 대화를 구성하세요. " + currentNpcPersona + " " +
            "플레이어의 말투를 존중해서 대화를 이어가세요. " +
            "플레이어의 말투는 선택지로 제공되는 것을 의미합니다. 플레이어의 성격은 다음과 같습니다: " + playerPersona + "\n\n" +
            "플레이어의 말을 들은 뒤 1~3문장으로 대답하고, 그 상황에 맞는 2개의 짧은 한국어 선택지를 제공합니다. 만약 플레이어가  이 2개의 선택지는 반드시 플레이어 입장에서의 대답이어야 하며, NPC 입장에서의 발화는 절대 포함하지 마세요.\n\n" +
            "또한 플레이어는 NPC에게 퀘스트를 주지 않습니다. 모든 부탁/심부름은 NPC가 플레이어에게 합니다.\n\n" +
            "심부름(퀘스트)을 제안할 때는 반드시 다음 중 하나만 사용하세요: '사과', '바나나', '배', '센베'. 예: '사과 3개만 주워와줄래?' 처럼 제안합니다.\n\n" +
            "퀘스트를 제안할 경우, 플레이어의 선택지는 무조건 예시처럼 구성합니다: 선택 1: 응 주워올게! / 선택 2: 미안, 다음에 하도록 할게.\n\n" +
            "모든 응답은 반드시 아래 JSON 한 줄 형식으로만 출력합니다:\n" +
            "{\"npc\":\"NPC의 1~3문장 대답\",\"option_a\":\"플레이어의 선택지1\",\"option_b\":\"플레이어의 선택지2\"}\n" +
            "추가 설명, 코드블록, 마크다운, 따옴표 외의 텍스트는 절대 포함하지 마세요.";
    }

    [SerializeField]
    private string playerPersona =
        "플레이어는 밝고 귀엽고, 말을 짧게 끝내는 스타일입니다. 반말을 사용합니다. 퀘스트가 들어올 시에는 '응 좋아!', '아니 다음에 할래..' 같은 긍/부정의 대답만 합니다. 퀘스트(사과를 주워줘, 과자를 주워줘 와 같은 퀘스트)가 아닌 일반 대화에서는 대화가 자연스럽게 이어질 만한 대답을 만듭니다. 퀘스트가 아닌 일반 대화에서는 긍부정의 말을 최대한 사용하지마시, 퀘스트 시에만 사용하세요.  그 외의 대화에 대해서는 NPC들의 말을 공감해주는 형태의 대답을 뱉습니다. 넌센스 퀴즈나 퀴즈 형태의 질문이 들어오면 정답 혹은 오답을 대답합니다."
        + "플레이어의 맣투는 구어체여야 하며 단답식으로 끝나는 대답은 지양합니다.대화가 자연스럽게 이어지도록 구어체만을 사용합니다.";

    // ----------------------------------------------------------------------
    // 플레이어 랜덤 첫 대사
    // ----------------------------------------------------------------------
    private readonly string[] playerOpeningLines = new string[]
    {
        "안녕. 여기에 처음 이사 왔어.",
        "안녕하세요! 오늘부터 이 마을 사람이에요.",
        "처음 보는 얼굴이지? 방금 이사 왔어!",
        "안녕, 여기 마을 구경하러 왔어.",
        "새로 왔어. 잘 부탁해!",

    };
    private string lastPlayerOpeningLine = "";
    private int lastPlayerLineIndex = -1;

    // LLM 응답 저장
    private string currentNpc = "";
    private string currentOptionA = "";
    private string currentOptionB = "";

    // ----------------------------------------------------------------------
    // 직렬화용
    // ----------------------------------------------------------------------
    [Serializable] public class ChatMessage { public string role; public string content; }

    [Serializable]
    private class RequestPayload
    {
        public string model;
        public List<ChatMessage> messages;
        public float temperature;
        public int max_tokens;
        public bool stream;
    }

    [Serializable] private class OpenAIResponse { public Choice[] choices; }
    [Serializable] private class Choice { public Message message; }
    [Serializable] private class Message { public string role; public string content; }

    [Serializable]
    private class LlmTurn
    {
        public string npc;
        public string option_a;
        public string option_b;
    }

    // ----------------------------------------------------------------------
    // 타이핑
    // ----------------------------------------------------------------------
    [Header("타이핑 효과 설정")]
    public bool useTypewriter = true;
    public float charsPerSecond = 65f;
    private Coroutine typingCoroutine;

    // ======================================================================
    // Unity
    // ======================================================================
    void Start()
    {
        EndDialogue();
    }

    // ======================================================================
    // 외부에서 NPC 프로필 세팅
    // ======================================================================
    public void SetNpcProfile(string npcName, string npcPersona)
    {
        if (!string.IsNullOrEmpty(npcName))
            currentNpcName = npcName;
        else if (string.IsNullOrEmpty(currentNpcName))
            currentNpcName = "NPC";

        if (!string.IsNullOrEmpty(npcPersona))
            currentNpcPersona = npcPersona;
        else if (string.IsNullOrEmpty(currentNpcPersona))
            currentNpcPersona = "당신은 이 마을의 친절한 NPC입니다.";
    }

    // ======================================================================
    // 대화 시작
    // ======================================================================
    public void StartDialogue()
    {
        if (IsDialogueActive) return;

        if (config == null || string.IsNullOrEmpty(config.apiKey))
        {
            Debug.LogError("OpenAIConfig 에셋에 API Key가 없습니다!");
            return;
        }

        // 1) 먼저 퀘스트 완료 체크
        if (QuestManager.Instance != null && QuestManager.Instance.IsQuestComplete())
        {
            IsDialogueActive = true;
            dialoguePanel?.SetActive(true);
            optionButtonsContainer?.SetActive(false);

            conversationHistory.Clear();
            conversationHistory.Add(new ChatMessage
            {
                role = "system",
                content =
                    currentNpcPersona +
                    " 플레이어는 이런 말투를 가진 사람입니다: " + playerPersona + " " +
                    "지금 플레이어는 네가 부탁했던 아이템을 전부 가져왔어요. 아주 기쁘고 고마운 말투로 1~3문장으로 답하고, 그 뒤에 플레이어가 고를 수 있는 짧은 한국어 선택지 2개를 JSON 한 줄로만 출력하세요."
            });

            string itemName = QuestManager.Instance.targetItem;
            int count = QuestManager.Instance.requiredCount;

            AddMessageToHistory("user", $"제가 부탁하신 {itemName} {count}개를 전부 가져왔어요!");

            if (nameText != null) nameText.text = currentNpcName;
            if (responseText != null) responseText.text = "건네준 걸 확인하는 중...";

            SendRequestToOpenAI($"플레이어가 {itemName} {count}개를 전부 가져왔어. 기쁘게 반응해줘.");
            
            QuestManager.Instance.ClearQuest();
            

            return;
        }

        // 2) 일반 대화 시작
        conversationHistory.Clear();
        conversationHistory.Add(new ChatMessage
        {
            role = "system",
            content = string.IsNullOrEmpty(currentNpcPersona)
                ? BuildDefaultSystemPrompt() // 인스펙터가 비어 있으면 기본 프롬프트 사용
                : currentNpcPersona + " " +  // 인스펙터 프롬프트 우선
                    "플레이어의 말투를 존중해서 대화를 이어가세요. " +
                    "플레이어의 말투는 선택지로 제공되는 것을 의미합니다. 플레이어의 성격은 다음과 같습니다: " + playerPersona + "\n\n" +
                    "플레이어의 말을 들은 뒤 1~3문장으로 대답하고, 그 상황에 맞는 2개의 짧은 한국어 선택지를 제공합니다. 이 2개의 선택지는 반드시 플레이어 입장에서의 대답이어야 하며, NPC 입장에서의 발화는 절대 포함하지 마세요.\n\n" +
                    "또한 플레이어는 NPC에게 퀘스트를 주지 않습니다. 모든 부탁/심부름은 NPC가 플레이어에게 합니다.\n\n" +
                    "심부름(퀘스트)을 제안할 때는 반드시 하나만 사용하세요: '사과' 예: '사과 3개만 주워와줄래?' 처럼 제안합니다.\n\n" +
                    "###퀘스트를 제안할 경우, 플레이어의 선택지는 예시처럼 구성합니다: 선택 1: 응 주워올게! / 선택 2: 미안, 다음에 할게.\n\n###ㄴ" +
                    "모든 응답은 반드시 아래 JSON 한 줄 형식으로만 출력합니다:\n" +
                    "{\"npc\":\"NPC의 1~3문장 대답\",\"option_a\":\"플레이어의 선택지1\",\"option_b\":\"플레이어의 선택지2\"}\n" +
                    "추가 설명, 코드블록, 마크다운, 따옴표 외의 텍스트는 절대 포함하지 마세요."
        });


        IsDialogueActive = true;
        dialoguePanel?.SetActive(true);
        optionButtonsContainer?.SetActive(false);

        // 플레이어 첫 대사
        lastPlayerOpeningLine = GetRandomPlayerLine();
        if (nameText != null) nameText.text = "다람쥐";

        if (responseText != null)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            if (useTypewriter)
                typingCoroutine = StartCoroutine(TypewriterEffect(lastPlayerOpeningLine));
            else
                responseText.text = lastPlayerOpeningLine;
        }

        awaitingUserSelection = true;
    }

    // 랜덤 첫 대사
    private string GetRandomPlayerLine()
    {
        if (playerOpeningLines == null || playerOpeningLines.Length == 0)
            return "안녕. 여기에 처음 이사 왔어.";

        if (playerOpeningLines.Length == 1)
        {
            lastPlayerLineIndex = 0;
            return playerOpeningLines[0];
        }

        int idx;
        do { idx = UnityEngine.Random.Range(0, playerOpeningLines.Length); }
        while (idx == lastPlayerLineIndex);

        lastPlayerLineIndex = idx;
        return playerOpeningLines[idx];
    }

    // 대화 종료
    public void EndDialogue()
    {
        IsDialogueActive = false;
        awaitingUserSelection = false;
        isTyping = false;

        if (conversationHistory.Count > 1)
            conversationHistory.RemoveRange(1, conversationHistory.Count - 1);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (optionButtonsContainer != null) optionButtonsContainer.SetActive(false);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    private void AddMessageToHistory(string role, string content)
    {
        conversationHistory.Add(new ChatMessage { role = role, content = content });
    }

    // ----------------------------------------------------------------------
    // 버튼에서 선택됐을 때
    // ----------------------------------------------------------------------
    public void OnOptionSelected(string selectedOption)
    {
        Debug.Log($"[OpenAIConnector] 옵션 선택 감지: {selectedOption}");

        // 히스토리
        AddMessageToHistory("user", selectedOption);

        // 플레이어 이름 찍기
        if (nameText != null)
            nameText.text = "다람쥐";

        // 플레이어가 고른 것도 한 번 화면에 보여주기
        if (responseText != null)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            if (useTypewriter)
                typingCoroutine = StartCoroutine(TypewriterEffect(selectedOption));
            else
                responseText.text = selectedOption;
        }

        // 수락/거절 패턴
        string lower = selectedOption.ToLower();

        bool accept =
            lower.Contains("가져올게") || lower.Contains("가져올게요") ||
            lower.Contains("주워올게") || lower.Contains("주워올게요") ||
            lower.Contains("가져다줄게") || lower.Contains("가져다 줄게") ||
            lower.Contains("해올게") || lower.Contains("할게") ||
            lower.Contains("알겠") || lower.Contains("가져") ||
            lower.Contains("좋아") || lower.Contains("주워");

        bool reject =
            lower.Contains("싫") || lower.Contains("싫어요") ||
            lower.Contains("미안") || lower.Contains("못해") ||
            lower.Contains("다음에") || lower.Contains("나중에") ||
            lower.Contains("안할래") || lower.Contains("안 할래") ||
            lower.Contains("지금은");

        if (accept)
        {
            Debug.Log("[OpenAIConnector] 퀘스트 수락 감지! 대화 종료합니다.");
            EndDialogue();
            return;
        }

        if (reject)
        {
            Debug.Log("[OpenAIConnector] 플레이어가 퀘스트를 거절했습니다. 대화 종료.");
            EndDialogue();
            return;
        }

        // 일반 대화면 다음 턴
        optionButtonsContainer?.SetActive(false);
        SendRequestToOpenAI(selectedOption);
    }

    // ----------------------------------------------------------------------
    // LLM 요청
    // ----------------------------------------------------------------------
    public void SendRequestToOpenAI(string playerSelection)
    {
        if (config == null || string.IsNullOrEmpty(config.apiKey))
        {
            Debug.LogError("OpenAIConfig 에셋이 없거나 API Key가 비어 있습니다!");
            EndDialogue();
            return;
        }

        StartCoroutine(PostRequest(playerSelection));
    }

    private string CreateJsonPayload(string playerSelection)
    {
        List<ChatMessage> temp = new List<ChatMessage>(conversationHistory);

        temp.Add(new ChatMessage
        {
            role = "system",
            content =
                "JSON 한 줄로만 답하세요. 구조는 다음과 같습니다:\n" +
                "{\"npc\":\"NPC 대사 1~3문장\",\"option_a\":\"선택지1\",\"option_b\":\"선택지2\"}\n" +
                "퀘스트를 제안할 때는 반드시 '사과' 하나만 사용하세요."
        });

        temp.Add(new ChatMessage
        {
            role = "user",
            content = playerSelection
        });

        RequestPayload payload = new RequestPayload
        {
            model = config.modelName,
            messages = temp,
            temperature = 0.6f,
            max_tokens = 180,
            stream = false
        };

        return JsonUtility.ToJson(payload);
    }

    private IEnumerator PostRequest(string playerSelection)
    {
        string jsonPayload = CreateJsonPayload(playerSelection);

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(OpenAIApiUrl, ""))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Authorization", "Bearer " + config.apiKey);
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 20;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"OpenAI 통신 실패: {request.error}");
                EndDialogue();
            }
            else
            {
                string content = ExtractContent(request.downloadHandler.text);
                ApplyTurn(content);
            }
        }
    }

    private string ExtractContent(string jsonResponse)
    {
        try
        {
            OpenAIResponse res = JsonUtility.FromJson<OpenAIResponse>(jsonResponse);
            if (res != null && res.choices != null && res.choices.Length > 0)
                return res.choices[0].message.content;
        }
        catch (Exception e)
        {
            Debug.LogError("content 파싱 실패: " + e.Message);
        }
        return jsonResponse;
    }

    // ----------------------------------------------------------------------
    // LLM 응답을 실제 UI에 반영
    // ----------------------------------------------------------------------
    private void ApplyTurn(string content)
    {
        LlmTurn turn = null;
        try { turn = JsonUtility.FromJson<LlmTurn>(content); } catch { }

        if (turn == null || string.IsNullOrEmpty(turn.npc))
        {
            currentNpc = content;
            currentOptionA = "응";
            currentOptionB = "아니";
        }
        else
        {
            currentNpc = turn.npc;
            currentOptionA = string.IsNullOrEmpty(turn.option_a) ? "응" : turn.option_a;
            currentOptionB = string.IsNullOrEmpty(turn.option_b) ? "아니" : turn.option_b;
        }

        // 🎯 이 대사가 실제 퀘스트 요청인지 검사 (사과/바나나/배/센베만)
        TryDetectLimitedQuest(currentNpc);

        if (nameText != null) nameText.text = currentNpcName;

        if (responseText != null)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            if (useTypewriter)
                typingCoroutine = StartCoroutine(TypewriterEffect(currentNpc));
            else
                responseText.text = currentNpc;
        }

        if (optionAText != null) optionAText.text = currentOptionA;
        if (optionBText != null) optionBText.text = currentOptionB;

        if (optionAButton != null) optionAButton.textToSend = currentOptionA;
        if (optionBButton != null) optionBButton.textToSend = currentOptionB;

        awaitingUserSelection = false;
        optionButtonsContainer?.SetActive(true);
    }

    // ----------------------------------------------------------------------
    // 우리 게임에 있는 4종만 퀘스트로 인정
    // ----------------------------------------------------------------------
    private void TryDetectLimitedQuest(string npcLine)
    {
        if (QuestManager.Instance == null) return;
        if (string.IsNullOrEmpty(npcLine)) return;

        string lower = npcLine.ToLower();

        bool mentionsApple = lower.Contains("사과");
        bool mentionsBanana = lower.Contains("바나나");
        bool mentionsPear = lower.Contains("배");
        bool mentionsSenbei = lower.Contains("센베") || lower.Contains("센베이");
        bool asksToBring = lower.Contains("가져") || lower.Contains("주워") || lower.Contains("모아") || lower.Contains("구해");

        if (!asksToBring) return;

        if (mentionsApple)
            QuestManager.Instance.RegisterQuest("사과", 3);
        else if (mentionsBanana)
            QuestManager.Instance.RegisterQuest("바나나", 3);
        else if (mentionsSenbei)
            QuestManager.Instance.RegisterQuest("센베", 3);
        else if (mentionsPear)
            QuestManager.Instance.RegisterQuest("배", 3);
    }

    // ----------------------------------------------------------------------
    // 타이핑 코루틴
    // ----------------------------------------------------------------------
    private IEnumerator TypewriterEffect(string fullText)
    {
        isTyping = true;
        responseText.text = "";
        float t = 0f;
        int shown = 0;

        while (shown < fullText.Length)
        {
            t += Time.deltaTime * charsPerSecond;
            int n = Mathf.Clamp(Mathf.FloorToInt(t), 0, fullText.Length);
            if (n != shown)
            {
                shown = n;
                responseText.text = fullText.Substring(0, shown);
            }
            yield return null;
        }

        isTyping = false;
    }

    // ----------------------------------------------------------------------
    // 입력
    // ----------------------------------------------------------------------
    void Update()
    {
        if (!IsDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Space))
            OnClickNext();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (responseText != null)
                responseText.text = "다음에 또 보자!";
            StartCoroutine(CloseAfterDelay(1.0f));
        }
    }

    private IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndDialogue();
    }

    public void OnClickNext()
    {
        if (!awaitingUserSelection)
        {
            Debug.LogWarning("[OpenAIConnector] 버튼 선택 대기 중.");
            return;
        }

        AddMessageToHistory("user", lastPlayerOpeningLine);
        SendRequestToOpenAI(lastPlayerOpeningLine);

        optionButtonsContainer?.SetActive(false);
        awaitingUserSelection = false;
    }

    // ----------------------------------------------------------------------
    // 선물 주기 (Z/X에서 호출)
    // ----------------------------------------------------------------------
    public void OnGiftGiven(string itemName, bool liked = true)
    {
        if (config == null || string.IsNullOrEmpty(config.apiKey))
        {
            Debug.LogError("OpenAIConfig 에셋이 없거나 API Key가 비어 있습니다!");
            return;
        }

        if (!IsDialogueActive)
        {
            StartDialogue();
        }

        AddMessageToHistory("user", $"플레이어가 너에게 {itemName}를(을) 건넸어.");

        string prompt;
        if (liked)
        {
            prompt =
                $"플레이어가 너에게 {itemName}를 선물했어. 아주 기쁘고 다정한 말투로 1~2문장으로 반응해줘. 그리고 플레이어가 이어서 말할 수 있도록 짧은 한국어 선택지 2개를 JSON 한 줄로만 줘.";
        }
        else
        {
            prompt =
                $"플레이어가 너에게 {itemName}를 줬어. 살짝 당황했거나 별로 마음에 들지 않았지만 예의를 지키는 말투로 1~2문장으로 반응해줘. 그리고 대화를 자연스럽게 이어갈 수 있도록 짧은 한국어 선택지 2개를 JSON 한 줄로만 줘.";
        }

        SendRequestToOpenAI(prompt);
    }
}
