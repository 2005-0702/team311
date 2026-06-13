using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "GoalScene";

    private void OnTriggerEnter(Collider other)
    {
        //親オブジェクトも含めて「Player」スクリプトがついているかチェック
        Player player = other.GetComponent<Player>();
        if (player == null) player = other.GetComponentInParent<Player>();

        // プレイヤーが存在する場合のみ処理
        if (player != null)
        {
            //「鍵を持っているか」をプレイヤーに問い合わせる
            if (player.HasKey)
            {
                Debug.Log("鍵を持っているので、ステージクリア！");
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                // 鍵を持っていない場合の処理（お好みで音を鳴らしたりUIを出したり）
                Debug.Log("鍵がありません！ステージ内の鍵を探してください。");
            }
        }
    }
}
