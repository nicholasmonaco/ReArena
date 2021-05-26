using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager() {
        // We might use this, not sure
    }


    private static PlayerController _player;
    public static PlayerController Player {
        get => _player;
        set {
            _player = value;
            PlayerTransform = value.transform;
        }
    }
    public static Transform PlayerTransform { get; private set; }


    public float PlayerGravity = -30;
}
