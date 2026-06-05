using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Primitives : MonoBehaviour
{
    public enum Team
    {
        Player1,
        Player2
    }
    public enum NeighbourType
    {
        None = 0,
        Left,
        Right,
        Up,
        Down,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,

    }
}
