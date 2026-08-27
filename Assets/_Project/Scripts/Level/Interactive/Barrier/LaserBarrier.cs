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


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (_leftEmitter != null)
        {
            _leftStartPosition =
                _leftEmitter.localPosition;
        }

        if (_rightEmitter != null)
        {
            _rightStartPosition =
                _rightEmitter.localPosition;
        }
    }


    private void Start()
    {
        CreateLaserAnimation();
    }


    private void OnDisable()
    {
        KillLaserAnimation();
    }


    private void OnDestroy()
    {
        KillLaserAnimation();
    }


    // =========================================================
    // LASER ANIMATION
    // =========================================================

    private void CreateLaserAnimation()
    {
        if (_isDisabled)
            return;


        if (
            _leftEmitter == null ||
            _rightEmitter == null
        )
        {
            return;
        }


        // На всякий случай полностью очищаем
        // предыдущую анимацию перед созданием новой.
        KillLaserAnimation();


        _laserSequence =
            DOTween.Sequence()
                .SetLink(
                    gameObject
                );


        _laserSequence.Append(
            _leftEmitter
                .DOLocalMoveY(
                    _leftStartPosition.y +
                    _moveDistance,
                    _moveDuration
                )
                .SetEase(
                    Ease.InOutSine
                )
        );


        _laserSequence.Join(
            _rightEmitter
                .DOLocalMoveY(
                    _rightStartPosition.y +
                    _moveDistance,
                    _moveDuration
                )
                .SetEase(
                    Ease.InOutSine
                )
        );


        _laserSequence.Append(
            _leftEmitter
                .DOLocalMoveY(
                    _leftStartPosition.y -
                    _moveDistance,
                    _moveDuration * 2f
                )
                .SetEase(
                    Ease.InOutSine
                )
        );


        _laserSequence.Join(
            _rightEmitter
                .DOLocalMoveY(
                    _rightStartPosition.y -
                    _moveDistance,
                    _moveDuration * 2f
                )
                .SetEase(
                    Ease.InOutSine
                )
        );


        _laserSequence.Append(
            _leftEmitter
                .DOLocalMove(
                    _leftStartPosition,
                    _moveDuration
                )
                .SetEase(
                    Ease.InOutSine
                )
        );


        _laserSequence.Join(
            _rightEmitter
                .DOLocalMove(
                    _rightStartPosition,
                    _moveDuration
                )
                .SetEase(
                    Ease.InOutSine
                )
        );


        _laserSequence
            .SetLoops(
                -1,
                LoopType.Restart
            )
            .OnKill(
                () =>
                {
                    _laserSequence = null;
                }
            );
    }


    // =========================================================
    // DISABLE BARRIER
    // =========================================================

    public void DisableBarrier()
    {
        if (_isDisabled)
            return;


        _isDisabled = true;


        KillLaserAnimation();


        foreach (
            LineRenderer laserLine
            in _laserLines
        )
        {
            if (laserLine != null)
            {
                laserLine.enabled =
                    false;
            }
        }


        if (_blockCollider != null)
        {
            _blockCollider.enabled =
                false;
        }
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void KillLaserAnimation()
    {
        if (
            _laserSequence != null &&
            _laserSequence.IsActive()
        )
        {
            _laserSequence.Kill();
        }

        _laserSequence = null;


        if (_leftEmitter != null)
        {
            _leftEmitter.DOKill();
        }


        if (_rightEmitter != null)
        {
            _rightEmitter.DOKill();
        }
    }
}