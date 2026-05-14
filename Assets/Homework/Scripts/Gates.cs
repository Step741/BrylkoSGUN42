using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gates : MonoBehaviour
{
    public int Score = 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {         
            Score++;
            Destroy(other.gameObject);
            Debug.Log("Ваш счет: " + Score);
        }
    }
}
