using UnityEngine;
using TMPro;
using UnityEngine.UI;

// ⭐️ 파일 이름이 ButtonHandler.cs 인지 확인하세요.

public class ButtonHandler : MonoBehaviour
{
    [Header("API 연결")]
    public OpenAIConnector apiConnector;

    private TextMeshProUGUI buttonText;

    [Header("버튼 설정")]
    public bool isOptionA;

    // 👉 OpenAIConnector가 여기다 실제로 보낼 값을 넣어줄 거야
    [HideInInspector] public string textToSend;

    void Start()
    {
        // 버튼 텍스트 가져오기
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        // ✅ 여기서는 더 이상 onClick 안 단다.
        //    인스펙터에서만 연결해서 "두 번 호출"되는 걸 막음

        // 기본 표시만 넣어두기
        if (buttonText != null)
        {
            buttonText.text = isOptionA ? "응 그래~" : "아니 괜찮아";
        }

        // 텍스트가 비어 있으면 일단 표시된 걸 넣어두기
        textToSend = buttonText != null ? buttonText.text : "";
    }

    // 이 함수는 인스펙터에서 Button 의 OnClick 으로만 연결해서 쓸 것!
    public void OnClick()
    {
        if (apiConnector == null)
        {
            Debug.LogError("API Connector가 ButtonHandler에 연결되지 않았습니다! 확인해주세요.");
            return;
        }

        // textToSend가 있으면 그걸 우선으로, 없으면 버튼에 적힌 글자
        string selectedOption = !string.IsNullOrEmpty(textToSend)
            ? textToSend
            : (buttonText != null ? buttonText.text : "선택지 텍스트 오류");

        Debug.Log($"[ButtonHandler] 클릭된 옵션: {selectedOption}");

        if (selectedOption == "대화 종료")
        {
            apiConnector.EndDialogue();
            return;
        }

        apiConnector.OnOptionSelected(selectedOption);

        // 클릭하면 로딩 표시
        if (buttonText != null)
        {
            buttonText.text = "응답 대기 중...";
        }
        textToSend = "";
    }
}
