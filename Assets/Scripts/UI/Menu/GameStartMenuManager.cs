using UI;
using UnityEngine;

public class GameStartMenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 显示主菜单
        UIManager.Instance.ShowUI("MainMenu");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
