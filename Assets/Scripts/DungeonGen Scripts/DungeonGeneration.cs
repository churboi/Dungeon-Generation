using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DungeonGeneration : MonoBehaviour {

    [Header("Dungeon Variables")]
    public int dungeonSize;
    public int clusterSize;
    public int randomPlacementThreshold;

    [Header("Dungeon Rooms")]
    public Room[] dungeonRooms;
    public Room dungeonEntrance;

    [Header("Testing Variables")]
    public bool testMode = false;
    public Room[] testRooms;

    [Header("Debugging & Console")]
    public float waitTime;

    [HideInInspector]
    public bool debugBuildMessages;
    [HideInInspector]
    public bool pendingNodesShow;
    [HideInInspector]
    public bool newExitNodesShow;
    [HideInInspector]
    public bool nodeSubtraction;
    [HideInInspector]
    public bool wallBlockerShow;
    [HideInInspector]
    public bool DEFCShow;
    [HideInInspector]
    public bool clusterShow;
    [HideInInspector]
    public bool collisionShow;

    private Node[] newRoomNodes;
    private Node nodeToMatch;
    private Node clusterNode;
    private bool finishedGeneration = false;

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
        if (debugBuildMessages)
        {
            Debug.Log("<b>Cluster Count: " + clusterCount + ", Dungeon Size: " + dungeonSize + "</b>");
        }

        for (int clusterIteration = 0; clusterIteration < clusterCount; clusterIteration++)
        {
            if(debugBuildMessages && clusterShow)
            {
                Debug.Log("<size=18><b>New Cluster</b></size>"); // Debugging
            }

            var startRoom = dungeonEntrance;
            var pendingNodes = new List<Node>();

            if (clusterIteration == 0 )
            {
                Instantiate(startRoom, transform.position, transform.rotation);
                startRoom.name = ("Room: " + roomCount);
                startRoom.tag = "startRoom";
                pendingNodes = new List<Node>(startRoom.GetNodes());
            }

            else
            {
                pendingNodes.Clear();
                if(clusterNode != null)
                {
                    pendingNodes.Add(clusterNode);
                    if (debugBuildMessages && clusterShow)
                    {
                        Debug.Log("<size=18>Cluster Beginning with Node: " + clusterNode.name + "</size>");
                    }
                }
            }

            int nodesLeft = pendingNodes.Count;

            //Main  Section
            for (int iteration = 0; iteration < dungeonSizeIn; iteration++)
            {
                var newExits = new List<Node>();
                nodesLeft = pendingNodes.Count;

                var placingLH = true;

                foreach (var pendingNode in pendingNodes)
                {
                    var newTag = GetRandom(pendingNode.roomTags);

                    if (iteration == (dungeonSizeIn - 1)) //If last iteration
                    {
                        newTag = "DE";
                        if ((placingLH) && (clusterIteration < clusterCount-1))
                        {
                            newTag = "LH";
                        }
                    }
                    
                    var collisionLoop = false;
                    var roomPlacementAttempts = 0;
                    
                    do
                    {
                        if ((clusterIteration > 0) && (pendingNodes.Count == 1))
                        {
                            newTag = "CF";
                        }
                        //Make this occur less often
                        if (!(iteration == (dungeonSizeIn - 1)))
                        {
                            newTag = DeadEndFreqencyCorrection(newTag, nodesLeft, pendingNode);
                        }
                        
                        var newRoomPrefab = GetRandomWithTag(roomsToUse, newTag);

                        var newRoom = (Room)Instantiate(newRoomPrefab);

                        //yield return new WaitForSeconds(waitTime);

                        newRoomNodes = newRoom.GetNodes();

                        nodeToMatch = newRoomNodes.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newRoomNodes);

                        MatchExits(pendingNode, nodeToMatch);

                        yield return new WaitForSeconds(waitTime);

                        if (newRoom.colliders.isCollided())
                        {
                            if (newRoom != null)
                            {
                                if (collisionShow)
                                {
                                    Debug.Log("<size=12><b>Collision at: </b></size>" + newRoom.transform.position + ", <size=12><b>With:</b></size> " + newRoom.gameObject.name);
                                }
                                Destroy(newRoom.gameObject);
                            }
                            collisionLoop = true;
                            roomPlacementAttempts++;
                            if (!(iteration == (dungeonSizeIn - 1)))
                            {
                                newTag = GetRandom(pendingNode.roomTags);
                            }
                            if (roomPlacementAttempts > randomPlacementThreshold)
                            {
                                newTag = "WB";
                                
                                if (roomPlacementAttempts > randomPlacementThreshold+1)
                                {
                                    if (debugBuildMessages && wallBlockerShow)
                                    {
                                        Debug.Log("<color=red><size=16>DELETING WALL BLOCKER</size></color>, nodes left: <color=purple>" + (nodesLeft) + "</color>"); //Debugging
                                    }
                                    collisionLoop = false;
                                    roomPlacementAttempts = 0;
                                    Destroy(newRoom.gameObject);
                                }
                            }
                        }
                        else
                        {
                            var tempName = newRoom.name; // Debugging
                            
                            roomCount++;
                            newRoom.name = ("Room: " + roomCount);

                            if (debugBuildMessages)
                            {
                                Debug.Log("<b>Cluster Iteration:</b> " + (clusterIteration+1) + ", Iteration: " + (iteration + 1) + ", <color=red>Nodes left:</color> <color=purple>" + nodesLeft + "</color>, Tag: " + newTag + ", New Room Placed:<color=blue> " + tempName + "</color>, as: <color=blue>" + newRoom.name + "</color>, Attached to: <color=olive>" + pendingNode.transform.parent.name + "</color>", newRoom);
                                //Debug.Log("New Room Placed:<color=blue> " + tempName + "</color>, as: <color=blue>" + newRoom.name + "</color>, Attached to: <color=olive>" + pendingNode.transform.parent.name + "</color>", newRoom); // Debugging
                                newRoom.NameNodes(); // Debugging
                            }

                            if(newTag == "LH")
                            {
                                placingLH = false;
                                clusterNode = pendingNode;
                                if (debugBuildMessages && clusterShow)
                                {
                                    Debug.Log("<size=16>Cluster starting from: <color=red>" + clusterNode.name + "</color></size>");
                                }
                            }

                            collisionLoop = false;
                            roomPlacementAttempts = 0;
                        }
                    } while (collisionLoop);
                    if (debugBuildMessages && nodeSubtraction)
                    {
                        Debug.Log("<size=14><color=red>-1 Node</color></size>"); // Debugging
                    }
                    nodesLeft--;

                    newExits.AddRange(newRoomNodes.Where(e => e != nodeToMatch)); //Add new nodes that weren't used into node pool

                    if (debugBuildMessages && newExitNodesShow)
                    {
                        Debug.Log("New Exits: (<color=purple>" + newExits.Count + "</color>): " + (ArrayToString<Node>(newExits.ToArray()))); // Debugging
                    }
                }
                pendingNodes = newExits;

                if (iteration == (dungeonSizeIn - 1) && (pendingNodes.Count == 1))
                {
                    clusterNode = pendingNodes[0];
                }

                if (debugBuildMessages && pendingNodesShow)
                {
                    Debug.Log("<color=navy>Pending Nodes: </color>(<color=purple>" + pendingNodes.Count + "</color>): " + (ArrayToString<Node>(pendingNodes.ToArray())));
                }
            }
            
        }
        Debug.Log("<b><size=16>Finished Gen</size></b>"); // Debugging
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

    private  string DeadEndFreqencyCorrection(string tagIn, int nodeCount, Node pendingNode)
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
                    if (debugBuildMessages && DEFCShow)
                    {
                        Debug.Log("DeadEnd Frequency Correction: tagIn; " + tagIn + ", nodeCount; " + nodeCount + ", NewTag: " + newTag);
                    }
                        deadEndFrequencyLoop = true;
                }
            }
            else { deadEndFrequencyLoop = false; }
        } while (deadEndFrequencyLoop);
        return newTag;
    }
     
    private Node RandomHallway(Node pendingNodeIn, int hallSizeIn)
    {
        var newTag = "SH";
        var oldNode = pendingNodeIn;

        for (int i = 0; i <= hallSizeIn; i++)
        {
            var newExits = new List<Node>();
            var newRoomPrefab = GetRandomWithTag(dungeonRooms, newTag);

            var newRoom = (Room)Instantiate(newRoomPrefab);
       
            var newRoomNodes = newRoom.GetNodes();

            nodeToMatch = newRoomNodes.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newRoomNodes);

            MatchExits(oldNode, nodeToMatch); //Match current room node with newly instantiated room node

            if (newRoom.colliders.isCollided())
            {
                if (newRoom != null)
                {
                    Destroy(newRoom.gameObject);
                    return oldNode;
                }
            }
            newExits.AddRange(newRoomNodes.Where(e => e != nodeToMatch));
            oldNode = newExits[0];
        }
        return oldNode;
    }
}
