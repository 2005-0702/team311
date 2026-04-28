using UnityEngine;

/// <summary>
/// プレイヤーや腕の先端が触れることで、他のギミックをON/OFFするスイッチ。
/// </summary>
public class GimmickSwitch : MonoBehaviour
{
    [Header("Target")]
    public MonoBehaviour targetGimmick; // 制御したいギミックのスクリプト

    [Header("Settings")]
    public bool isOnAtStart = false;    // 開始時にONかどうか
    public bool isOneTime = false;      // 一回切り（押しっぱなし）にするか
    
    [Header("Visuals")]
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.red;

    private bool isOn = false;
    private Renderer myRenderer;

    void Start()
    {
        myRenderer = GetComponent<Renderer>();
        isOn = isOnAtStart;
        
        // 開始時の状態をギミックに反映
        if (targetGimmick != null)
        {
            targetGimmick.SendMessage("SetActivate", isOn, SendMessageOptions.DontRequireReceiver);
        }
        
        UpdateVisual();
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤー、または腕の先端（Hand）に反応
        if (other.CompareTag("Player") || other.name.Contains("Hand") || other.name.Contains("Visual"))
        {
            if (isOneTime && isOn) return;

            ToggleSwitch();
        }
    }

    public void ToggleSwitch()
    {
        isOn = !isOn;
        
        if (targetGimmick != null)
        {
            targetGimmick.SendMessage("SetActivate", isOn, SendMessageOptions.DontRequireReceiver);
        }
        
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (myRenderer != null)
        {
            myRenderer.material.color = isOn ? activeColor : inactiveColor;
        }
    }
}
