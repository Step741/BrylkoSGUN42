using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Restart : MonoBehaviour
{
    public event System.Action OnFillComplete;

    [SerializeField]
    private GameObject _restartPanel;

    [SerializeField]
    private Image _fill;

    private Coroutine _fillCoroutine;

    public bool IsEnd { get; private set; }

    public void ShowPanel()
    {
        _restartPanel.SetActive(true);
    }

    public void HidePanel()
    {
        _restartPanel.SetActive(false);
    }

    [ContextMenu("Fill")]
    public void Test()
    {
        StartFillPanel(2);
    }

    public void StartFillPanel(float time)
    {
        IsEnd = false;
        _fill.fillAmount = 0f;

        if (_fillCoroutine != null)
        {
            StopCoroutine(_fillCoroutine);
        }

        _fillCoroutine = StartCoroutine(FillPanel(time));
    }

    public void StopFillPanel()
    {
        if (_fillCoroutine != null)
        {
            StopCoroutine(_fillCoroutine);
            _fillCoroutine = null;
        }

        IsEnd = false;
        _fill.fillAmount = 0f;
    }

    private IEnumerator FillPanel(float time)
    {
        float fillAmount = 0f;

        while (fillAmount < 1f)
        {
            fillAmount += Time.deltaTime / time;

            _fill.fillAmount = Mathf.Clamp01(fillAmount);

            yield return null;
        }

        _fill.fillAmount = 1f;

        IsEnd = true;
        _fillCoroutine = null;

        OnFillComplete?.Invoke();
    }
}