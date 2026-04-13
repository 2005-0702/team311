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

        // プレイヤーの正面方向（親オブジェクトなどから取得。ここでは簡易的にスケールで判定）
        float facingDir = 1f;
        if (transform.root != null) 
            facingDir = Mathf.Sign(transform.root.localScale.x);

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
        if (armVisual != null)
        {
            // 円柱（Cylinder）の高さを調整
            float scaleY = currentLength / 2f;
            armVisual.localScale = new Vector3(armVisual.localScale.x, scaleY, armVisual.localScale.z);
            
            // X軸方向に伸びるように位置を調整
            armVisual.localPosition = new Vector3(currentLength / 2f, 0, 0);
            
            // 円柱が右（X軸）を向くように回転（Z軸周りに-90度回転）
            armVisual.localRotation = Quaternion.Euler(0, 0, -90);
        }

        if (handVisual != null)
        {
            // 手の先端をX軸方向の現在の長さの位置に配置
            handVisual.localPosition = new Vector3(currentLength, 0, 0);
        }
    }
}
