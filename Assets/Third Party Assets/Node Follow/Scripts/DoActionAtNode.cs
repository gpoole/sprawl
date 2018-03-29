using System.Collections;
using UnityEngine;

// This script is used to do some action when the movingObject is at specific node.
public class DoActionAtNode : MonoBehaviour
{
    #region Private variables
    private NodeFollow nodeFollow;          // The script itself
    private bool wait;                      // Used to stop some action from happening many times
    #endregion

    #region Public Variables
    public GameObject targetGameObject;     // The gameObject where the NodeFollow script is
    public int node;                        // The node number
    public float waitBeforeNewAction;       // Wait this amount before can do the some action again
    #endregion

    void Start ()
    {
        if(targetGameObject == null)
        {
            Debug.LogError("Target gameObject for Node Follow script is not set");
            return;
        }
        // Getting the NodeFollow component in targetGameObject
        nodeFollow = targetGameObject.GetComponent<NodeFollow>();
	}
	
	void Update ()
    {
        if (targetGameObject == null)
        {
            return;
        }
        // If the specified node number is too big (not in array range) give warning error and the some action won't happen before valid node number is given
        if (node >= nodeFollow.nodes.Length)
        {
            Debug.LogError("Node with the specified number is out of range");
            return;
        }

        if(nodeFollow.movingObject == null)
        {
            Debug.LogError("Moving object not set in inspector");
            return;
        }
        // If the movingObject is at the [Node number], do something
        // StartCoroutine("Wait") is used to stop the action for happening many time
        if (nodeFollow.movingObject.transform.position == nodeFollow.nodes[node] && wait == false)
        {
            StartCoroutine("Wait");
            Debug.Log("At target node now. Doing something here");
            // Do something here (Play sound, instantiate something, make the movingObject stop(must set stopMoving to public in NodeFollow script for this)... or some other interesting action)

        }
    }

    IEnumerator Wait()
    {
        // Used for not doing the some action many times in case there is Stop Point in this node
        // Need to set to desired WaitForSeconds(waitBeforeNewAction) amount in inspector. (Should be bigger than stop time)

        wait = true;
        yield return new WaitForSeconds(waitBeforeNewAction);
        wait = false;
    }
}