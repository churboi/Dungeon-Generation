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



    /*
    public GameObject[] GetNodesInRoom()
    {
        List<GameObject> roomNodes = new List<GameObject>();
        for(int i = 0; i < nodes.Length;i++)
        {
            roomNodes.Add(nodes[i]);
        }
        GameObject[] roomNodesArray = roomNodes.ToArray();
        return roomNodesArray;
    }
    public override string ToString()
    {
        var String = "Room: " + transform.name + "\n";
        String += "Nodes: " + nodes.Length + "\n";
        for (int i = 0; i < nodes.Length; i++)
        {
            String += "Node [" + i + "], Position:" + nodes[i].transform.position + ", Tag: " + nodes[i].tag +"\n";
        }
        return String;
    }
    */
}
