using DG.Tweening;
using UnityEngine;

public class LaserBarrier : MonoBehaviour
{
    [Header("Laser")]
    [SerializeField] private LineRenderer[] _laserLines;
    [SerializeField] private Transform _leftEmitter;
    [SerializeField] private Transform _rightEmitter;

    [Header("Animation")]
    [SerializeField] private float _moveDistance = 0.15f;
    [SerializeField] private float _moveDuration = 0.8f;

    [Header("Block")]
    [SerializeField] private Collider _blockCollider;

    private Sequence _laserSequence;
    private bool _isDisabled;

    private Vector3 _leftStartPosition;
    private Vector3 _rightStartPosition;

    private void Awake()
    {
        _leftStartPosition = _leftEmitter.localPosition;
        _rightStartPosition = _rightEmitter.localPosition;
    }

    private void Start()
    {
        CreateLaserAnimation();
    }

    private void CreateLaserAnimation()
    {
        if (_isDisabled)
            return;

        DOTween.Kill(_leftEmitter);
        DOTween.Kill(_rightEmitter);

        _laserSequence = DOTween.Sequence();

        _laserSequence.Append(
            _leftEmitter.DOLocalMoveY(
                _leftStartPosition.y + _moveDistance,
                _moveDuration
            ).SetEase(Ease.InOutSine)
        );

        _laserSequence.Join(
            _rightEmitter.DOLocalMoveY(
                _rightStartPosition.y + _moveDistance,
                _moveDuration
            ).SetEase(Ease.InOutSine)
        );

        _laserSequence.Append(
            _leftEmitter.DOLocalMoveY(
                _leftStartPosition.y - _moveDistance,
                _moveDuration * 2f
            ).SetEase(Ease.InOutSine)
        );

        _laserSequence.Join(
            _rightEmitter.DOLocalMoveY(
                _rightStartPosition.y - _moveDistance,
                _moveDuration * 2f
            ).SetEase(Ease.InOutSine)
        );

        _laserSequence.Append(
            _leftEmitter.DOLocalMove(
                _leftStartPosition,
                _moveDuration
            ).SetEase(Ease.InOutSine)
        );

        _laserSequence.Join(
            _rightEmitter.DOLocalMove(
                _rightStartPosition,
                _moveDuration
            ).SetEase(Ease.InOutSine)
        );

        _laserSequence.SetLoops(-1, LoopType.Restart);
        _laserSequence.SetLink(gameObject);
    }

    public void DisableBarrier()
    {
        if (_isDisabled)
            return;

        _isDisabled = true;

        if (_laserSequence != null)
        {
            _laserSequence.Kill();
            _laserSequence = null;
        }

        DOTween.Kill(_leftEmitter);
        DOTween.Kill(_rightEmitter);

        foreach (LineRenderer laserLine in _laserLines)
        {
            if (laserLine != null)
                laserLine.enabled = false;
        }

        if (_blockCollider != null)
            _blockCollider.enabled = false;
    }

    private void OnDestroy()
    {
        _laserSequence?.Kill();
        DOTween.Kill(_leftEmitter);
        DOTween.Kill(_rightEmitter);
    }
}