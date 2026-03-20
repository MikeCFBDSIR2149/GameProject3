// MainMenu.cs

using UI.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : BaseMenu
    {
        [Header("主菜单按钮")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button levelSelectButton;
        
        public override void OnInit()
        {
            base.OnInit();
            
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartClicked);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }
            
            if (creditsButton != null)
            {
                creditsButton.onClick.AddListener(OnCreditsClicked);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
            
            if (levelSelectButton != null)
            {
                levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
            }
        }

        public override void UpdateUI(object data)
        {
            throw new System.NotImplementedException();
        }

        private void OnStartClicked()
        {
            // 使用反射找到GameManager
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.StartNewGame();
            }
            else
            {
                Debug.LogError("未找到GameManager");
            }
            
            OnHide();
        }
        
        private void OnSettingsClicked()
        {
            MenuData settingsData = new MenuData(MenuType.SettingsMenu);
            UIManager.Instance.ShowUI("SettingsMenu", settingsData);
        }
        
        private void OnCreditsClicked()
        {
            MenuData creditsData = new MenuData(MenuType.CreditsMenu);
            UIManager.Instance.ShowUI("CreditsMenu", creditsData);
        }
        
        private void OnLevelSelectClicked()
        {
            MenuData levelSelectData = new MenuData(MenuType.LevelSelectMenu);
            UIManager.Instance.ShowUI("LevelSelectMenu", levelSelectData);
        }
        
        private void OnQuitClicked()
        {
           // ShowQuitConfirmation();
        }
        
        // private void ShowQuitConfirmation()
        // {
        //     ConfirmationData confirmData = new ConfirmationData(
        //         "确认退出",
        //         "确定要退出游戏吗？",
        //         () => {
        //             #if UNITY_EDITOR
        //             UnityEditor.EditorApplication.isPlaying = false;
        //             #else
        //             Application.Quit();
        //             #endif
        //         },
        //         null
        //     );
        //     
        //     MenuData confirmMenuData = new MenuData(MenuType.ConfirmationDialog, false, confirmData);
        //     UIManager.Instance.ShowUI("ConfirmationDialog", confirmMenuData);
        // }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (startButton != null) startButton.onClick.RemoveListener(OnStartClicked);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (creditsButton != null) creditsButton.onClick.RemoveListener(OnCreditsClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitClicked);
            if (levelSelectButton != null) levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
        }
    }
}