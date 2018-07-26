using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Room : MonoBehaviour {

    [Header("Components of a room")]
    public Node[] nodes;
    public string[] roomTags;
    public RoomCollision colliders;

    public Node[] GetNodes()
    {
        return GetComponentsInChildren<Node>();
    }
    public void NameNodes()
    {
        for(int i = 0; i < nodes.Length; i++)
        {
            nodes[i].name = (this.name + "'s node: " + (i+1)); 
        }
    }
}
