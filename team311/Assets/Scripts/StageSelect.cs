using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelect : MonoBehaviour
{
    [Header("開始ステージ")]
    public StagePoint startStage;

    [Header("移動速度")]
    public float moveSpeed = 3f;

    private StagePoint currentStage;
    private StagePoint targetStage;
    private bool isMoving;
    bool IsLoading = false;

    void Start()
    {
        if (startStage == null)
        {
            Debug.LogError("開始ステージ（startStage）が設定されていません！");
            return;
        }

        currentStage = startStage;
        transform.position = currentStage.transform.position;
    }

    void Update()
    {
        Debug.Log($"Update動いてます。isMoving = {isMoving}");
        // 移動中は入力を受け付けない
        if (isMoving)
        {
            MovePlayer();
            return;
        }

        // 行き先を決定する（独立したif文にすることで入力をクリアに）
        if (Input.GetKeyDown(KeyCode.RightArrow)) MoveTo(currentStage.right, "右");
        if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveTo(currentStage.left, "左");
        if (Input.GetKeyDown(KeyCode.UpArrow)) MoveTo(currentStage.up, "上");
        if (Input.GetKeyDown(KeyCode.DownArrow)) MoveTo(currentStage.down, "下");

        // ステージ決定
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!string.IsNullOrEmpty(currentStage.sceneName)&& !IsLoading)
            {
                IsLoading = true;
                Debug.Log("Scene読み込み : " + currentStage.sceneName);
                SceneManager.LoadSceneAsync(currentStage.sceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogWarning($"{currentStage.name} にシーン名が設定されていません。");
            }
        }
    }

    // デバッグしやすくするために、どの方向へ進もうとしたか引数を追加
    void MoveTo(StagePoint next, string direction)
    {
        if (next == null)
        {
            // 動かない原因が「次のステージが未設定だから」だと分かるようにする
            Debug.LogWarning($"{currentStage.name} から見て 【{direction}】 にはステージが繋がっていません。");
            return;
        }

        targetStage = next;
        isMoving = true;
    }

    void MovePlayer()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetStage.transform.position,
            moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetStage.transform.position) < 5.0f)
        {
            transform.position = targetStage.transform.position;

            currentStage = targetStage;
            targetStage = null;
            isMoving = false;

            // 到着したステージの名前と、そこから移動できる方向をログに出す
            Debug.Log($"現在地更新：【{currentStage.name}】に到着しました。");
        }
    }
}
