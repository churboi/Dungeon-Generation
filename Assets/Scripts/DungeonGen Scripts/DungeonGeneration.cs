using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGeneration : MonoBehaviour {

    [Header("Dungeon Variables")]
    public int dungeonSize;

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
    private bool finishedGeneration = false;

    public bool FinishedGen
    {
        get { return finishedGeneration; }
    }

    private void Start()
    {
        StartCoroutine(DungeonGen(dungeonSize));
    }

    IEnumerator DungeonGen(int dungeonSizeIn)
    {
        var roomCount = 0;

        var roomsToUse = dungeonRooms;

        if (testMode)
        {
            roomsToUse = testRooms;
        }
        var startRoom = (Room)Instantiate(dungeonEntrance, transform.position, transform.rotation);
        startRoom.name = ("Room: " + roomCount);
        startRoom.tag = "startRoom";

        yield return new WaitForSeconds(waitTime);

        var pendingNodes = new List<Node>(startRoom.GetNodes());
        int nodesLeft = pendingNodes.Count;

        //Main  Section
        for (int iteration = 0; iteration < dungeonSizeIn; iteration++)
        {
            Debug.Log("Iteration: " + iteration);
            var newExits = new List<Node>();
            nodesLeft = pendingNodes.Count;

            foreach (var pendingNode in pendingNodes)
            {
                //Debug.Log("[@] [@] Pending Nodes Left: " + pendingNodes.Count + " [@] [@]");

                var newTag = GetRandom(pendingNode.roomTags);

                //Make it very unlikely for DeadEnds to spawn (check)

                //Collision loop actually starts here
                var collisionLoop = false;
                var roomPlacementAttempts = 0;

                do
                {
                    //Don't place dead ends when there are few open nodes
                    //Make this occur less often
                    newTag = DeadEndFreqencyCorrection(newTag, nodesLeft, pendingNode);

                    var newRoomPrefab = GetRandomWithTag(roomsToUse, newTag);

                    var newRoom = (Room)Instantiate(newRoomPrefab);
                    yield return new WaitForSeconds(waitTime);

                    //Debug.Log("New Room Created: " + newRoom.name);

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
                        newTag = GetRandom(pendingNode.roomTags);
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
                        Debug.Log(newRoom.name + " Placed at: " + newRoom.transform.position);
                        nodesLeft--;
                    }
                } while (collisionLoop);
                /*
                 * End loop here
                 */

                newExits.AddRange(newRoomNodes.Where(e => e != nodeToMatch)); //Add new nodes that weren't used into node pool
            }

            pendingNodes = newExits;
        }

        nodesLeft = pendingNodes.Count;

        //DeadEnd Placing
        Debug.Log("Entering dead end placing"); 

        foreach (var pendingNode in pendingNodes)
        {
            Debug.Log("Nodes Left: " + nodesLeft);
            if (nodesLeft == 1)
            {
                //Place the final room
                Debug.Log("ON LAST NODE");
            }
            else
            {
                //Put the new tag DE in here
            }
            //Debug.Log("|_| |_|  Closing the pending exits, Count: " + pendingNodes.Count + " |_| |_|");

            var newTag = "DE";
            var collisionLoop = false;
            var roomPlacementAttempts = 0;

            do
            {
                var newRoomPrefab = GetRandomWithTag(roomsToUse, newTag);

                var newRoom = (Room)Instantiate(newRoomPrefab);
                yield return new WaitForSeconds(waitTime * deadEndSlowDown);

                //Debug.Log("New Room Created: " + newRoom.name);

                newRoomNodes = newRoom.GetNodes();

                nodeToMatch = newRoomNodes.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newRoomNodes);

                MatchExits(pendingNode, nodeToMatch);

                yield return new WaitForSeconds(waitTime / 2f);

                if (newRoom.colliders.isCollided())
                {
                    //Debug.Log(newRoom.name + ", is collided at: " + newRoom.transform.position);
                    if (newRoom != null)
                    {
                        Destroy(newRoom.gameObject);
                    }
                    collisionLoop = true;
                    roomPlacementAttempts++;
                    if (roomPlacementAttempts > 8)
                    {
                        newTag = "WB";
                        //Debug.Log("TAG WB, RPA: " + roomPlacementAttempts);

                        if (roomPlacementAttempts > 9)
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
                    
                    //Subtract one from the amount of nodes left to determine when wer're on the last node
                }
            } while (collisionLoop);
            nodesLeft--;
            yield return new WaitForSeconds(waitTime*deadEndSlowDown);
        }
        finishedGeneration = true;
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
                if (nodeCount <= 3)
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

        //Likely total trash
        /*
        Debug.Log("HALLWAY METHOD");
        var newExits = new List<Node>();
        var newTag = "SH";
        var collisionLoop = false;
        var roomPlacementAttempts = 0;
        var roomCount = 0;
        do
        {
            var newRoomPrefab = GetRandomWithTag(dungeonRooms, newTag);
            var newRoom = (Room)Instantiate(newRoomPrefab);
            newRoomNodes = newRoom.GetNodes();
            nodeToMatch = newRoomNodes.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newRoomNodes);
            MatchExits(pendingNode, nodeToMatch);
            if (newRoom.colliders.isCollided())
            {
                //Debug.Log(newRoom.name + ", is collided at: " + newRoom.transform.position);
                if (newRoom != null)
                {
                    Destroy(newRoom.gameObject);
                }
                collisionLoop = true;
                roomPlacementAttempts++;
                if (roomPlacementAttempts > 8)
                {
                    newTag = "WB";
                    //Debug.Log("TAG WB, RPA: " + roomPlacementAttempts);

                    if (roomPlacementAttempts > 9)
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
                newRoom.name = ("Hall: " + roomCount);
                collisionLoop = false;
                newExits.AddRange(newRoomNodes.Where(e => e != nodeToMatch));
                pendingNode = newExits[0];
            }
        } while (collisionLoop);
        */
    }
}
