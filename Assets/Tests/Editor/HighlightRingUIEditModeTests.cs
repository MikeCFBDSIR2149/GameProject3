#if UNITY_EDITOR
using NUnit.Framework;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class HighlightRingUIEditModeTests
{
    private GameObject _canvasObject;
    private GameObject _ringObject;

    [SetUp]
    public void SetUp()
    {
        _canvasObject = new GameObject("TestCanvas", typeof(RectTransform), typeof(Canvas));
        RectTransform canvasRect = _canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1920f, 1080f);

        _ringObject = new GameObject("TestRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
            typeof(HighlightRingUI));
        _ringObject.transform.SetParent(_canvasObject.transform, false);

        RectTransform ringRect = _ringObject.GetComponent<RectTransform>();
        ringRect.sizeDelta = new Vector2(100f, 100f);
        ringRect.anchoredPosition = Vector2.zero;
        _ringObject.GetComponent<HighlightRingUI>().OnInit();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_ringObject);
        Object.DestroyImmediate(_canvasObject);
    }

    [Test]
    public void AimScore_IsZeroAtCenter_AndRejectsOutsidePoint()
    {
        HighlightRingUI ring = _ringObject.GetComponent<HighlightRingUI>();
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        ring.ApplyVisual(screenCenter, 0f);

        Assert.That(ring.TryGetAimScore(screenCenter, out float score), Is.True);
        Assert.That(score, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(ring.TryGetAimScore(screenCenter + new Vector2(1000f, 1000f), out _), Is.False);
    }

    [Test]
    public void OnInit_DisablesGraphicRaycastTarget()
    {
        Assert.That(_ringObject.GetComponent<Image>().raycastTarget, Is.False);
    }
}
#endif
