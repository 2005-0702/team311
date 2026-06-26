using UnityEngine;

public class SwitchingSpikeTrap : MonoBehaviour
{
    [Header("ダメージ設定")]
    public int damageAmount = 1;

    [Header("スイッチ連動設定")]
    public bool activeWhenRed = true; // trueならRedでON、falseならBlueでON

    private Collider myCollider;
    private MeshRenderer myRenderer;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        myRenderer = GetComponent<MeshRenderer>();
    }

    // BlockManagerから呼ばれる
    public void RefreshState(bool isRedActive)
    {
        bool shouldBeActive = (activeWhenRed == isRedActive);

        // 見た目の切替
        if (myRenderer != null)
        {
            Color c = myRenderer.material.color;
            c.a = shouldBeActive ? 1.0f : 0.2f;
            myRenderer.material.color = c;
        }

        // 当たり判定の切替
        if (myCollider != null)
            myCollider.enabled = shouldBeActive;
    }

    // ダメージ処理
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
            player.TakeDamage(damageAmount);
    }
}