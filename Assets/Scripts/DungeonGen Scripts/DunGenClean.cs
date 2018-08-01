using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DunGenClean : MonoBehaviour {

    [Header("Dungeon Variables")]
    public int dungeonSize;
    public int clusterSize;
    public int randomPlacementThreshold;

    [Header("Dungeon Rooms")]
    public Room[] dungeonRooms;
    public Room dungeonEntrance;

    private Node[] newRoomNodes;
    private Node nodeToMatch;
    private Node clusterNode;
    private bool finishedGeneration = false;

    IEnumerator DungeonGen(int dungeonSizeIn, int clusterCount)
    {
        var roomCount = 0;
        var roomsToUse = dungeonRooms;

        for (int clusterIteration = 0; clusterIteration < clusterCount; clusterIteration++)
        {
            var startRoom = dungeonEntrance;
            var pendingNodes = new List<Node>();

            if (clusterIteration == 0)
            {
                Instantiate(startRoom, transform.position, transform.rotation);
                startRoom.name = ("Room: " + roomCount);
                startRoom.tag = "startRoom";
                pendingNodes = new List<Node>(startRoom.GetNodes());
            }

            else
            {
                pendingNodes.Clear();
                if (clusterNode != null)
                {
                    pendingNodes.Add(clusterNode);
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
                        if ((placingLH) && (clusterIteration < clusterCount - 1))
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
                            //newTag = DeadEndFreqencyCorrection(newTag, nodesLeft, pendingNode);
                        }

                        var newRoomPrefab = GetRandomWithTag(roomsToUse, newTag);

                        var newRoom = (Room)Instantiate(newRoomPrefab);

                        //yield return new WaitForSeconds(waitTime);

                        newRoomNodes = newRoom.GetNodes();

                        nodeToMatch = newRoomNodes.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newRoomNodes);

                        MatchExits(pendingNode, nodeToMatch);

                       // yield return new WaitForSeconds(waitTime);

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
                            if (roomPlacementAttempts > randomPlacementThreshold)
                            {
                                newTag = "WB";

                                if (roomPlacementAttempts > randomPlacementThreshold + 1)
                                {
                                    collisionLoop = false;
                                    roomPlacementAttempts = 0;
                                    Destroy(newRoom.gameObject);
                                }
                            }
                        }
                        else
                        {
                            roomCount++;
                            if (newTag == "LH")
                            {
                                placingLH = false;
                                clusterNode = pendingNode;
                            }
                            collisionLoop = false;
                            roomPlacementAttempts = 0;
                        }
                    } while (collisionLoop);
                    nodesLeft--;
                    newExits.AddRange(newRoomNodes.Where(e => e != nodeToMatch)); //Add new nodes that weren't used into node pool
                }
                pendingNodes = newExits;

                if (iteration == (dungeonSizeIn - 1) && (pendingNodes.Count == 1))
                {
                    clusterNode = pendingNodes[0];
                }
            }

        }
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
}
