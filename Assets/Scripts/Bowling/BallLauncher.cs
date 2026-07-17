using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    [SerializeField] 
    private float launchForce = 25f;

    private bool launched;

    public void Launch(Rigidbody ball)
    {
        if (launched)
            return;

        launched = true;

        ball.AddForce(
            transform.forward * launchForce,
            ForceMode.Impulse);
    }

    public void ResetLaunch()
    {
        launched = false;
    }
}