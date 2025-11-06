using UnityEngine;

// 이 속성을 추가하면 Project 창에서 우클릭 > Create > AI/Open AI Config 메뉴가 생깁니다.
[CreateAssetMenu(fileName = "OpenAIConfig", menuName = "AI/Open AI Config", order = 1)]
public class OpenAIConfig : ScriptableObject
{
    // 여기에 API 키, 모델 이름 등 설정 변수들을 추가할 예정입니다.
    // 현재는 빈 클래스입니다.

    // API Key
    [Header("API Key")]
    [Tooltip(" API Key를 여기에 입력하세요.")]
    public string apiKey = "";

    // 모델 설정
    [Header("Model Settings")]
    [Tooltip("사용할 모델 이름: ")]
    public string modelName = "gpt-3.5-turbo";
}
