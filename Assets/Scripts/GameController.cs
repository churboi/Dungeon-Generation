using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    public DungeonGeneration dunGen;
    public Player player;
    public PlayerSpawn playerSpawn;
    public Camera mainCamera;

    void Update()
    {
        if(dunGen.FinishedGen)
        {
            //Spawn Player and make Camera follow player
        }
    }
}
