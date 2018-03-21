using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGeneration : MonoBehaviour {

    [Header("Dungeon Variables")]
    public int dungeonSize;
    public int clusterSize;

    [Header("Dungeon Rooms")]
    public Room[] dungeonRooms;
    public Room dungeonEntrance;

    [Header("Private Build Timers")]
    public float waitTime;
    public float deadEndSlowDown;
    public bool testMode = false;
    public Room[] testRooms;

    private Node[] newRoomNodes;
    private Node nodeToMatch;
    private Node clusterNode;
    private bool finishedGeneration = false;
    private bool clusterStich = false;

    public bool FinishedGen
    {
        get { return finishedGeneration; }
    }

    private void Start()
    {
        StartCoroutine(DungeonGen(dungeonSize, clusterSize));
    }

    IEnumerator DungeonGen(int dungeonSizeIn, int clusterCount)
    {
        var roomCount = 0;
        var roomsToUse = dungeonRooms;
        if (testMode)
        {
            roomsToUse = testRooms;
        }

        Debug.Log("Cluster Count: " + clusterCount + ", Dungeon Size: " + dungeonSize);

        for (int clusterIteration = 0; clusterIteration < clusterCount; clusterIteration++)
        {
            var startRoom = dungeonEntrance;
            var pendingNodes = new List<Node>();

            if (clusterIteration == 0 )
            {
                Instantiate(startRoom, transform.position, transform.rotation);
                startRoom.name = ("Room: " + roomCount);
                startRoom.tag = "startRoom";
                pendingNodes = new List<Node>(startRoom.GetNodes());
            }
            /*
            else
            {
                pendingNodes.Clear();
                pendingNodes.Add(clusterNode);
                //Debug.Log("Else: pendingNodes.Add(clusterNode)");
            }
            */
            int nodesLeft = pendingNodes.Count;

            //Main  Section
            for (int iteration = 0; iteration < dungeonSizeIn; iteration++)
            {
                //Debug.Log("Iteration: " + iteration);
                var newExits = new List<Node>();
                nodesLeft = pendingNodes.Count;

                foreach (var pendingNode in pendingNodes)
                {
                    var newTag = GetRandom(pendingNode.roomTags);
                    if (iteration == (dungeonSizeIn - 1))
                    {
                        newTag = "DE";
                        /*
                        if(nodesLeft == 1)
                        {
                            clusterNode = pendingNode;
                            //Debug.Log("Added node to clusterNode");
                            clusterStich = true;
                        }
                        */
                    }
                    Debug.Log("Cluster Iteration: " + clusterIteration + ", Iteration: " + (iteration + 1) + ", Nodes left: " + nodesLeft + ", Tag: " + newTag);
                    var collisionLoop = false;
                    var roomPlacementAttempts = 0;
                    yield return new WaitForSeconds(waitTime);
                    
                    do
                    {
                        //Make this occur less often
                        if (!(iteration == (dungeonSizeIn - 1)))
                        {
                            newTag = DeadEndFreqencyCorrection(newTag, nodesLeft, pendingNode);
                        }

                        var newRoomPrefab = GetRandomWithTag(roomsToUse, newTag);

                        /*
                        if(!(clusterNode == null) && clusterStich)
                        {
                            newRoomPrefab = GetRandomWithTag(roomsToUse, "LH");
                            Debug.Log("Grabbing Long Hall for last node");
                            clusterStich = false;
                        }
                        */

                        var newRoom = (Room)Instantiate(newRoomPrefab);
                        yield return new WaitForSeconds(waitTime);

                        newRoomNodes = newRoom.GetNodes();

                        nodeToMatch = newRoomNodes.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newRoomNodes);

                        MatchExits(pendingNode, nodeToMatch);

                        yield return new WaitForSeconds(waitTime / 2f);

                        if (newRoom.colliders.isCollided())
                        {
                            if (newRoom != null)
                            {
                                Destroy(newRoom.gameObject);
                            }
                            collisionLoop = true;
                            roomPlacementAttempts++;
                            if (!(iteration == (dungeonSizeIn - 1)))
                            {
                                newTag = GetRandom(pendingNode.roomTags);
                            }
                            if (roomPlacementAttempts > 15)
                            {
                                newTag = "WB";
                                //Debug.Log("TAG WB, RPA: " + roomPlacementAttempts);
                                if (roomPlacementAttempts > 16)
                                {
                                    //Debug.Log("DELETING WALL BLOCKER");
                                    collisionLoop = false;
                                    roomPlacementAttempts = 0;
                                    Destroy(newRoom.gameObject);
                                }
                            }
                        }
                        else
                        {
                            roomCount++;
                            newRoom.name = ("Room: " + roomCount);
                            collisionLoop = false;
                            roomPlacementAttempts = 0;
                        }
                    } while (collisionLoop);
                    nodesLeft--;

                    newExits.AddRange(newRoomNodes.Where(e => e != nodeToMatch)); //Add new nodes that weren't used into node pool
                }
                pendingNodes = newExits;
            }
            Debug.Log("New Cluster");
        }
        Debug.Log("Finished Gen");
        yield return null;
    }

    private static TItem GetRandom<TItem>(TItem[] array)
    {
        var randomItem = array[Random.Range(0, array.Length)];
        return randomItem;
    }

    private static Room GetRandomWithTag(IEnumerable<Room> modules, string tagToMatch)
    {
        var matchingRoom = modules.Where(m => m.roomTags.Contains(tagToMatch)).ToArray();
        //returns random "Module" room from array of rooms with matching tag in "matchingModule"
        return GetRandom(matchingRoom);
    }

    private void MatchExits(Node oldExit, Node newExit)
    {
        //grabs new room as "newModule" by taking in the ModuleConnector inside the new room and finding it's parent
        var newNode = newExit.transform.parent;

        //Creates a Vector3 local variable that is the opposite of the "oldExit"'s forward facing direction 
        var forwardVectorToMatch = -oldExit.transform.forward;

        var correctiveRotation = Azimuth(forwardVectorToMatch) - Azimuth(newExit.transform.forward);

        //Rotate the room "Module so that the "newModule" faces toward the exit
        newNode.RotateAround(newExit.transform.position, Vector3.up, correctiveRotation);

        //grabs the oldExit's position and subtracts the newExits position to get the corrct position
        var correctiveTranslation = oldExit.transform.position - newExit.transform.position;

        newNode.transform.position += correctiveTranslation;
    }

    private static float Azimuth(Vector3 vector)
    {
        return Vector3.Angle(Vector3.forward, vector) * Mathf.Sign(vector.x);
    }

    private static string ArrayToString<TItem>(TItem[] arrayIn)
    {
        var itemsInArray = arrayIn.ToString();
        itemsInArray += ":";
        for (int i = 0; i < arrayIn.Length; i++)
        {
            itemsInArray += ", " + arrayIn[i].ToString();
        }
        return itemsInArray;
    }

    private static string DeadEndFreqencyCorrection(string tagIn, int nodeCount, Node pendingNode)
    {
        var deadEndFrequencyLoop = false;
        var newTag = tagIn;
        do
        {
            if (newTag == "DE")
            {
                if (nodeCount <= 6)
                {
                    newTag = GetRandom(pendingNode.roomTags);
                    Debug.Log("DeadEnd Frequency Correction: tagIn; " + tagIn + ", nodeCount; " + nodeCount + ", NewTag: " + newTag);
                    deadEndFrequencyLoop = true;
                }
            }
            else { deadEndFrequencyLoop = false; }
        } while (deadEndFrequencyLoop);
        return newTag;
    }
     
    private void RandomHallway(Node pendingNode, int hallSizeIn)
    {
        
    }
}
