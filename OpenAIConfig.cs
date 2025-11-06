using UnityEngine;


[CreateAssetMenu(fileName = "OpenAIConfig", menuName = "AI/Open AI Config", order = 1)]
public class OpenAIConfig : ScriptableObject
{


    // API Key
    [Header("API Key")]
    [Tooltip(" API Key를 여기에 입력하세요.")]
    public string apiKey = "";

    [Header("Model Settings")]
    [Tooltip("사용할 모델 이름: ")]
    public string modelName = "gpt-3.5-turbo";
}
