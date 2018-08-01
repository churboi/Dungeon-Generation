using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartRoom : Room {

    public GameObject spawnPoint;
    public GameObject player;

    public void spawnPlayer()
    {
        Instantiate(player, spawnPoint.transform);
    }

    

}
