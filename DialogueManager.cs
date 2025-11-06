using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("타이핑 설정")]
    public bool useTypewriter = true;
    public float charsPerSecond = 40f;

    private Coroutine typingCo;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowTyping(TMP_Text target, string fullText)
    {
        if (!useTypewriter || target == null)
        {
            target.text = fullText;
            return;
        }

        if (typingCo != null)
            StopCoroutine(typingCo);

        typingCo = StartCoroutine(Typewriter(target, fullText));
    }

    private IEnumerator Typewriter(TMP_Text target, string full)
    {
        target.text = "";
        float t = 0f;
        int shown = 0;

        while (shown < full.Length)
        {
            t += Time.deltaTime * charsPerSecond;
            int n = Mathf.Clamp(Mathf.FloorToInt(t), 0, full.Length);
            if (n != shown)
            {
                shown = n;
                target.text = full.Substring(0, shown);
            }
            yield return null;
        }
    }
}
