using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartRoom : Room {

    public PlayerSpawn spawner;

	public void SpawnPlayer()
    {
        Instantiate(spawner.player, spawner.transform.position, spawner.transform.rotation);
        Debug.Log("Start Room Player");
    }
}
