using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCollision : MonoBehaviour {

    public Room self;

    private bool collided = false;
    private string theOther;

    private void Start()
    {
        collided = false;
    }

    public bool isCollided()
    {
        return collided;
    }

    private void OnTriggerStay(Collider other)
    {
        if(self != null)
        {
            if(!(this.transform.root.position.x == 0 && this.transform.root.position.z == 0))
            {
                if (other.gameObject.GetComponentInParent<Room>() != null)
                {
                    if (!other.transform.IsChildOf(self.transform))
                    { 
                        if (other.transform.root.GetInstanceID() != self.GetInstanceID())
                        {
                            //Debug.Log("Other name: " + other.name + ", other parent name: " + other.transform.root.name + ",\nThis collider: " + this.name + ", this root parent: " + this.transform.root.name);
                            theOther = other.transform.root.name;
                            collided = true;
                        }
                    }
                }
            }
            /*
            else if (other.transform.root.tag == "startRoom")
            {
                collided = true;
            }
            */
        }
        //Debug.Log("### COLLISION TIME ###");
    }

}
