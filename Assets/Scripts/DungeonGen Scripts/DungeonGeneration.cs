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
    public void GenDun()
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

        //Start of the Actual Generation, Start of the cluster logic
        for (int clusterIteration = 0; clusterIteration < clusterCount; clusterIteration++)
        {
            if(debugBuildMessages && clusterShow)
            {
                Debug.Log("<size=18><b>New Cluster</b></size>"); // Debugging
            }

            //Sets the startRoom to whatever you assigned to dungeonEntrance
            var startRoom = dungeonEntrance;
            //Creates a list of Nodes called "pendingNodes" that need rooms attached to them
            var pendingNodes = new List<Node>();

            //If this is the first cluster (or there are no clusters): Creates starting room in the middle of scene, names and tags room, and puts nodes from startRoom in pendingNodes
            if (clusterIteration == 0 )
            {
                Instantiate(startRoom, transform.position, transform.rotation);
                startRoom.name = ("Room: " + roomCount); //Names startRoom
                startRoom.tag = "startRoom"; //Tags startRoom
                pendingNodes = new List<Node>(startRoom.GetNodes()); //Puts empty nodes from startRoom in pendingNodes
            }
            //If you have clusters enabled, clears pendingNodes and puts the last Node left in pendingNodes
            else
            {
                //Empty pendingNode list
                pendingNodes.Clear();
                //If there is a last node in the cluster Logic
                if(clusterNode != null)
                {
                    //clusterNode grabbed at the end of the cluster logic. Adds the last Node to pendingNodes
                    pendingNodes.Add(clusterNode);
                    if (debugBuildMessages && clusterShow)
                    {
                        Debug.Log("<size=18>Cluster Beginning with Node: " + clusterNode.name + "</size>");
                    }
                }
            }
            //Creates variable to find how many pendingNodes are left
            int nodesLeft = pendingNodes.Count;

            //Main Section Dungeon Generation
            for (int iteration = 0; iteration < dungeonSizeIn; iteration++)
            {
                //Local variable made for holding any new exits that are created during a construction chunk (loop), this does not include the nodes in pendingNodes
                var newExits = new List<Node>();
                //updates nodesLeft count
                nodesLeft = pendingNodes.Count;

                //Local variable for clusters, determines whether or not a long Hall should be place (BAD Logic for creating more distance between clusters. 
                var placingLH = true;

                //goes through the list of pendingNodes
                foreach (var pendingNode in pendingNodes)
                {
                    //Local variable for getting tag based on what the currentNode allows
                    var newTag = GetRandom(pendingNode.roomTags);
                    //If last iteration
                    if (iteration == (dungeonSizeIn - 1)) 
                    {
                        //If it's the last iteration force tag to be "Dead End"
                        newTag = "DE";
                        //If there are more Clusters that need to be made, and placing Long Hall is true
                        if ((placingLH) && (clusterIteration < clusterCount-1))
                        {
                            newTag = "LH";
                        }
                    }
                    //Local Variable for checking collisions
                    var collisionLoop = false;
                    //Local Variable tracking room placement attempts
                    var roomPlacementAttempts = 0;
                    
                    do
                    {
                        //If it's part of a the cluster and it's the last node
                        if ((clusterIteration > 0) && (pendingNodes.Count == 1))
                        {
                            //new tag Cluster Friendly
                            newTag = "CF";
                        }
                        //If it's not the last interation, try to make the new tag something other than a Dead End
                        if (!(iteration == (dungeonSizeIn - 1))) //Make this occur less often
                        {
                            //Dead end correction method for checking the circumstances 
                            newTag = DeadEndFreqencyCorrection(newTag, nodesLeft, pendingNode);
                        }
                        
                        //Local Variable for holding the room object that will be attempted
                        var newRoomPrefab = GetRandomWithTag(roomsToUse, newTag);
                        //Local Variable for the actual Game Object
                        var newRoom = (Room)Instantiate(newRoomPrefab);
                        //Waits for a short moment to help with collisions (Looking for a fix)
                        yield return new WaitForSeconds(waitTime);
                        //Stores the nodes of the new Room
                        newRoomNodes = newRoom.GetNodes();
                        //Find a node in new Room to attach to pending Node
                        nodeToMatch = newRoomNodes.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newRoomNodes);
                        //Rotate the new Room to line up with pending Node
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
