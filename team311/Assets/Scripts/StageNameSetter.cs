using UnityEngine;
using UnityEngine.SceneManagement;

public class StageNameSetter : MonoBehaviour
{
   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StageManager.CurrentStageName = SceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
