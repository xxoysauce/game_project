using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraAutoMove : MonoBehaviour
{
    [Header("이동 경로")]
    public Vector3 startPosition = new Vector3(-1.7f, 1.2f, -35f);
    public Vector3 endPosition = new Vector3(-1.7f, 3.5f, 10f);
    public float moveDuration = 5f; // 이동 시간
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("씬 전환")]
    public string nextSceneName = "IslandScene"; // 이동할 씬 이름
    public float delayAfterMove = 1.5f; // 이동 후 약간의 대기 시간

    private float t = 0f;
    private bool hasLoaded = false;

    void Start()
    {
        transform.position = startPosition;
    }

    void Update()
    {
        if (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float curveT = moveCurve.Evaluate(t);
            transform.position = Vector3.Lerp(startPosition, endPosition, curveT);
        }
        else if (!hasLoaded)
        {
            hasLoaded = true;
            Invoke(nameof(LoadNextScene), delayAfterMove);
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
