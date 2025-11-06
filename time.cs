using UnityEngine;

public class FiveStageSkyboxOnly : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light sun;                    
    [SerializeField] private Material[] skyMaterials = new Material[5]; 

    [Header("Cycle Settings")]
    [Tooltip("전체 낮~밤 순환 시간 (초). 30 = 30초에 5단계 전환(각 6초)")]
    [SerializeField] private float fullCycleSeconds = 210f;
    [Tooltip("끝나면 다시 낮으로 돌아갈지")]
    [SerializeField] private bool loop = true;

    [Header("Sun (Optional)")]
    [Tooltip("단계별 태양 밝기 (선택). 비워두면 변경하지 않음.")]
    [SerializeField] private float[] sunIntensityPerStage = new float[5]; 
    [Tooltip("낮 각도(시작) / 밤 각도(끝). 회전 안 쓰면 같은 값으로 두면 됨.")]
    [SerializeField] private float startAngle = 60f;
    [SerializeField] private float endAngle = -30f;

    private int stage = 0;           
    private float stageDuration;    
    private float timer = 0f;

    private const int STAGES = 5;

    private void Start()
    {
        if (skyMaterials == null || skyMaterials.Length < STAGES)
        {
            Debug.LogError("[FiveStageSkyboxOnly] skyMaterials를 정확히 5개 넣어주세요.");
            enabled = false;
            return;
        }

        if (sun == null)
            sun = RenderSettings.sun;

        stageDuration = fullCycleSeconds / STAGES;
        ApplyStage(0);
    }

    private void Update()
    {
        if (stageDuration <= 0f) return;

        timer += Time.unscaledDeltaTime;
        if (timer >= stageDuration)
        {
            timer = 0f;
            stage++;
            if (stage >= STAGES)
            {
                stage = loop ? 0 : STAGES - 1;
            }
            ApplyStage(stage);
        }

        
        if (sun != null)
        {
            float totalRatio = (stage + (timer / stageDuration)) / (float)STAGES; // 0~1
            float angle = Mathf.Lerp(startAngle, endAngle, totalRatio);
            sun.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }
    }

    private void ApplyStage(int index)
    {
        
        if (skyMaterials[index] != null)
        {
            RenderSettings.skybox = skyMaterials[index];
            DynamicGI.UpdateEnvironment();
        }
        else
        {
            Debug.LogWarning($"[FiveStageSkyboxOnly] skyMaterials[{index}] 가 비어있어요.");
        }

       
        if (sun != null && sunIntensityPerStage != null && sunIntensityPerStage.Length >= STAGES)
        {
            float intensity = sunIntensityPerStage[index];
            if (intensity > 0f) sun.intensity = intensity; 
        }

        Debug.Log($"[FiveStageSkyboxOnly] Stage {index + 1}/5 적용: {(skyMaterials[index] ? skyMaterials[index].name : "null")}");
    }
}
