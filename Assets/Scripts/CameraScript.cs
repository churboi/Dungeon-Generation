using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public Player player;
    public DungeonGeneration dunGen;

    private bool following = false;

    private Vector3 offset = new Vector3(0f,20f,-30f);
}
