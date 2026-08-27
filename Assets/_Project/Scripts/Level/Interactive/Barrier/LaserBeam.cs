using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField]
    private LineRenderer _lineRenderer;

    [SerializeField]
    private Transform _startPoint;

    [SerializeField]
    private Transform _endPoint;


    private void LateUpdate()
    {
        if (
            _lineRenderer == null ||
            _startPoint == null ||
            _endPoint == null
        )
        {
            return;
        }


        _lineRenderer.SetPosition(
            0,
            _startPoint.position
        );

        _lineRenderer.SetPosition(
            1,
            _endPoint.position
        );
    }
}