// ConfirmationData.cs
using System;

namespace UI
{
    [Serializable]
    public class ConfirmationData
    {
        public string title;
        public string message;
        public Action onConfirm;
        public Action onCancel;
        public string confirmText = "确认";
        public string cancelText = "取消";
        
        public ConfirmationData(string title, string message, Action onConfirm, Action onCancel)
        {
            this.title = title;
            this.message = message;
            this.onConfirm = onConfirm;
            this.onCancel = onCancel;
        }
    }
}