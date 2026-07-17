using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] 
    private TMP_Text scoreText;

    private void Update()
    {
        scoreText.text = $"Score: {GameManager.Instance.Score}";
    }
}