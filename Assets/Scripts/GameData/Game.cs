using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Game
{
    #region Important References
    public static InputActions Input = new _InputActions();
    #endregion

    #region Managers
    public static GameManager Manager;
    #endregion

    #region Settings Parameters
    public static float VolumeScale = 0.5f;
    #endregion


    static Game() {
        Manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }




    // Modified input controls class - enables on construction
    private class _InputActions : InputActions {
        public _InputActions() : base() {
            Enable();
        }
    }
}
