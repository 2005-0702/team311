using UnityEngine;

/// <summary>
/// カートゥーン風の伸縮する腕を制御するスクリプト。
/// 手元（Shoulder）から正面に向かって伸び、障害物があればそこで止まります。
/// </summary>
public class ExtendableArm : MonoBehaviour
{
    [Header("Components")]
    public Transform armVisual;   // 腕のメッシュ（円柱など）
    public Transform handVisual;  // 手のメッシュ（球体など）

    [Header("Settings")]
    public float maxArmLength = 5f;    // 腕の最大長
    public float extensionSpeed = 15f; // 伸びる速さ
    public float retractionSpeed = 20f; // 縮む速さ
    public LayerMask obstacleLayer;    // 貫通させないレイヤー
    [Header("Visual Settings")]
    public float armThickness = 0.5f;  // 腕の太さ
    public float handSize = 1.0f;      // 手の大きさ

    private float currentLength = 0f;
    private bool isExtending = false;

    void Update()
    {
        UpdateAiming();

        // マウスの左クリックを押している間、腕が伸びる
        if (Input.GetMouseButton(0))
        {
            isExtending = true;
        }
        else
        {
            isExtending = false;
        }

        UpdateArmMovement();
        UpdateArmVisuals();
    }

    void UpdateAiming()
    {
        // マウスのワールド座標を取得（Z軸はプレイヤーと同じ位置に投影）
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // 肩からマウスへの方向
        Vector3 targetDir = mouseWorldPos - transform.position;
        targetDir.z = 0; // 奥行きは無視（サイドビューを想定）

        // プレイヤーの向きを親オブジェクトから取得
        float facingDir = Mathf.Sign(transform.parent.localScale.x);


        // 背中側（向きと逆）を向こうとしたら制限をかける
        bool isBehind = Mathf.Sign(targetDir.x) != Mathf.Sign(facingDir);
        if (isBehind)
        {
            // 背中に回らないよう、Xを0にして真上・真下を限界にする
            targetDir.x = 0;
        }

        // 腕（Shoulder）の向きを更新（X軸を先端とする）
        if (targetDir != Vector3.zero)
        {
            transform.right = targetDir.normalized;
        }

        // プレイヤーの向きに合わせて肩の向きを反転
        transform.localScale = new Vector3(Mathf.Sign(transform.parent.localScale.x), 1, 1);
    }

    void UpdateArmMovement()
    {
        float targetLength = 0f;

        if (isExtending)
        {
            // 現在の向き（マウス方向）にレイを飛ばして、障害物までの距離を測る
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.right, out hit, maxArmLength, obstacleLayer))
            {
                targetLength = hit.distance;
            }
            else
            {
                targetLength = maxArmLength;
            }

            // 伸びる動き
            currentLength = Mathf.MoveTowards(currentLength, targetLength, extensionSpeed * Time.deltaTime);
        }
        else
        {
            // 縮む動き
            currentLength = Mathf.MoveTowards(currentLength, 0f, retractionSpeed * Time.deltaTime);
        }
    }

    void UpdateArmVisuals()
    {
        // 1. 手（HandVisual）の位置を更新
        if (handVisual != null)
        {
            float lossyScaleX = Mathf.Abs(transform.lossyScale.x);
            if (lossyScaleX < 0.01f) lossyScaleX = 1f;

            handVisual.localPosition = new Vector3(currentLength / lossyScaleX, 0, 0);
            
            // 手の大きさを反映
            handVisual.localScale = Vector3.one * handSize;
        }

        // 2. 腕（ArmVisual）を表示。手と肩の距離に合わせて伸縮させる
        if (armVisual != null && handVisual != null)
        {
            // 手のローカル位置を基準にする
            float localDist = handVisual.localPosition.x;

            float halfDist = localDist / 2f;

            // 長さを設定（Y軸を長さとして使用）、太さを X, Z に適用
            armVisual.localScale = new Vector3(armThickness, Mathf.Abs(halfDist), armThickness);
            
            // 位置を設定
            armVisual.localPosition = new Vector3(halfDist, 0, 0);
            
            // 向きを調整
            armVisual.localRotation = Quaternion.Euler(0, 0, -90);
        }
    }
}
