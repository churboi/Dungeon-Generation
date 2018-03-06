using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public Player player;
    public DungeonGeneration dunGen;

    private bool following = false;

    private Vector3 offset = new Vector3(0f,20f,-30f);

    void LateUpdate()
    {
        if(dunGen.GetComponent<DungeonGeneration>().FinishedGen)
        {
            following = true;
        }
        if(following)
        {
            if (player != null)
            {
                if (player.enabled)
                {
                    transform.position = player.transform.position + offset;
                }
            }
        }
    }
}
