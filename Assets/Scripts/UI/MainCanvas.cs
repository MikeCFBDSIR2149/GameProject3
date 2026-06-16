using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class MainCanvas : MonoBehaviour
    {
        [SerializeField] private Canvas mainCanvas;

        private void OnEnable()
        {
            if (UIManager.Instance != null) UIManager.Instance.RegisterMainCanvas(mainCanvas);
        }

        private void OnDisable()
        {
            if (UIManager.Instance != null) UIManager.Instance.UnregisterMainCanvas(mainCanvas);
        }
    }
}
