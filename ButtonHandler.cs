using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    [Header("API 연결")]
    public OpenAIConnector apiConnector;

    private TextMeshProUGUI buttonText;

    [Header("버튼 설정")]
    public bool isOptionA;

    [HideInInspector] public string textToSend;

    void Start()
    {

        buttonText = GetComponentInChildren<TextMeshProUGUI>();


        if (buttonText != null)
        {
            buttonText.text = isOptionA ? "응 그래~" : "아니 괜찮아";
        }


        textToSend = buttonText != null ? buttonText.text : "";
    }

    public void OnClick()
    {
        if (apiConnector == null)
        {
            Debug.LogError("API Connector가 ButtonHandler에 연결되지 않았습니다! 확인해주세요.");
            return;
        }

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

        if (buttonText != null)
        {
            buttonText.text = "응답 대기 중...";
        }
        textToSend = "";
    }
}
