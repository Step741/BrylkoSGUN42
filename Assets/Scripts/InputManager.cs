using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _restartBack;
    [SerializeField]
    private Image _restart;
    [SerializeField, Range(0.1f, 1f)]
    private float _restartTime = 2f;
    private Controls.GameActions _gameActions;
    private Coroutine _fillRoutine;
    [Inject]
    public void Construct(Controls.GameActions gameActions)
    {
        _gameActions = gameActions;

    }

    private void Awake()
    {
        if (_restartBack != null) _restartBack.SetActive(false);
        if (_restart != null) _restart.fillAmount = 0f;
    }
    private void OnEnable()
    {
        _gameActions.Restart.started += OnRestart;
        _gameActions.Restart.canceled += OnRestartCancel;
    }
    private void OnDisable()
    {
        _gameActions.Restart.started -= OnRestart;
        _gameActions.Restart.canceled -= OnRestartCancel;
    }
    private void OnRestart(InputAction.CallbackContext context)
    {
        if (_restartBack != null) _restartBack.SetActive(true);
        if (_fillRoutine != null) StopCoroutine(_fillRoutine);
        _fillRoutine = StartCoroutine(FillRoutine());
    }
    private void OnRestartCancel(InputAction.CallbackContext context)
    {
        if (_fillRoutine != null)
        {
            StopCoroutine(_fillRoutine);
            _fillRoutine = null;
        }
        if (_restartBack != null) _restartBack.SetActive(false);
        if (_restart != null) _restart.fillAmount = 0f;
    }
    private IEnumerator FillRoutine()
    {
        if (_restartBack == null || _restart == null)
            yield break;

        float elapsed = 0f;
        _restart.fillAmount = 0f;
        while (elapsed < _restartTime)
        {
            elapsed += Time.deltaTime;
            _restart.fillAmount = elapsed / _restartTime;
            yield return null;

        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
