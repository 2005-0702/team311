using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("視差効果の速度 (0: 動かない, 1: カメラと同期)")]
    [Range(0f, 1f)]
    public float parallaxSpeed;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        // 前のフレームからのカメラの移動量を計算
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Xのみ移動量を反映させ、Yは 0 にすることで縦移動を無視します
        transform.position += new Vector3(deltaMovement.x * parallaxSpeed, 0, 0);

        // カメラの現在位置を保存（Y軸の変化も含めて保存しておかないと計算がズレるため全体を保存）
        lastCameraPosition = cameraTransform.position;
    }
}
