using UnityEngine;
using System.Collections;

public class TimerSwitch : MonoBehaviour
{
    [Header("タイマー設定")]
    public float onDuration = 3f;      // ONを維持する秒数
    public bool resetOnRetouch = true; // ON中に再度踏んだらタイマーをリセットするか

    [Header("見た目")]
    public Color offColor = Color.yellow;
    public Color onColor = Color.green;

    private bool isOn = false;
    private Renderer rend;
    private Coroutine timerCoroutine;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = offColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!isOn)
        {
            // OFFのとき → ONにしてタイマー開始
            Activate();
        }
        else if (resetOnRetouch)
        {
            // ON中に再度踏んだ → タイマーリセット
            StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(TimerRoutine());
        }
    }

    void Activate()
    {
        isOn = true;
        rend.material.color = onColor;
        BlockManager.Instance.ChangeColor(true); // ブロック・棘に通知

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    IEnumerator TimerRoutine()
    {
        float elapsed = 0f;

        while (elapsed < onDuration)
        {
            elapsed += Time.deltaTime;

            // 残り時間で色を変化させる（ちかちか演出）
            if (onDuration - elapsed <= 1f)
            {
                rend.material.color = Time.time % 0.2f < 0.1f ? onColor : offColor;
            }

            yield return null;
        }

        Deactivate();
    }

    void Deactivate()
    {
        isOn = false;
        rend.material.color = offColor;
        BlockManager.Instance.ChangeColor(false); // ブロック・棘をOFFに通知
    }
}