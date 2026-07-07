using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemCleaner : MonoBehaviour
{
    void Awake()
    {
        // シーン内にあるすべてのEventSystemを検索
        EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        // もし2個以上見つかったら、自分以外の古い方を強制消滅させる
        if (systems.Length > 1)
        {
            foreach (var sys in systems)
            {
                if (sys.gameObject != this.gameObject)
                {
                    Destroy(sys.gameObject);
                    Debug.Log("重複していた余分なEventSystemを強制削除しました！");
                    break;
                }
            }
        }
    }
}