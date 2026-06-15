using UnityEngine;
using UnityEngine.UI;

public class Restart : MonoBehaviour
{
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
        }

        _fill.fillAmount = 0;
    }

    private System.Collections.IEnumerator FillPanel(float time)
    {
        float fillAmount = 0;

        while (_fill.fillAmount < 1)
        {
            _fill.fillAmount = fillAmount;

            yield return new WaitForEndOfFrame();
            fillAmount += Time.deltaTime / time;
        }

        IsEnd = true;
        _fillCoroutine = null;
    }
}