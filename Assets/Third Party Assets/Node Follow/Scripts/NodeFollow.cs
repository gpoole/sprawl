using System.Collections;
using UnityEngine;

public class NodeFollow : MonoBehaviour
{
    #region Private variables
    private Quaternion originalRotation;            // Reference to the original rotation of the movingObject

    private Vector3 targetDirection;                // Passing the movingObject facing way

    private bool reverse;                           // Used for reverse movement

    private Vector3 currentPoint;                   // The position the "movingObject" is currently moving towards to

    private bool startTriggerEntered;               // The trigger zone that starts the whole "movingObject"

    private bool stopMoving;                        // Should the "movingObject" stop moving?
    #endregion

    #region Public variables
    [HideInInspector]                               // Used to change gizmo icon to the same as handles color
    public bool useWhiteGizmo;
    [HideInInspector]
    public bool useBlueGizmo;
    [HideInInspector]
    public bool useRedGizmo;
    [HideInInspector]
    public bool useGreenGizmo;
    [HideInInspector]
    public bool useYellowGizmo;

    public enum Direction                           // Ways that the movingObject can be facing ( depends how you have made the sprite)
    {
        up,
        down,
        left,
        right
    }

    public Direction objectDirection;               // Setting which way the gameObject is facing

    public enum SetHandlesColor                     // Colors for handles
    {
        white,
        blue,
        red,
        green,
        yellow
    }

    public SetHandlesColor HandlesColor;            // Colors for handles

    public GameObject movingObject;                 // The object that should move

    public int pointSelection;                      // Node number the movingObject is currently moving towards to

    public float moveSpeed;                         // How fast the movingObject should move

    public float stopTime = 2f;                     // The time to stop moving for

    public bool useTriggerForStart;                 // Start moving object at start instead of onTriggerEnter

    public bool moveToStartAtEnd;                   // Should the movingObject instantly move back to the start once end has been reached

    public bool rotateTowardsNextNode;              // Should the movingObject rotate towards next node

    public float rotationSpeed;                     // How fast the rotation should be

    public bool loop;                               // Should the movement loop                 

    public bool drawLines = true;                   // Draw lines in editor mode at all?

    public bool drawDotLine;                        // Draw dot lines

    public bool drawLastToFirst;                    // Draw line from last point to the first

    public bool showHandles = true;                 // Show handles

    public float handlesSize = 1;                   // Size of the node handles

    public Vector3[] nodes;                         // List of all nodes

    public int[] stopNodes;                         // List of points where to stop moving for stopTime amount

    public Color backgroundColor = Color.white;     // Custom editor background color

    public Vector3 currentPosition;                 // Used to store transforms current position

    public Vector3 lastPosition;                    // Used to compare transform position and and it's last position

    public float newX;                              // Used to store new x value if transform is moved   

    public float newY;                              // Used to store new y value if transform is moved 

    public float newZ;                              // Used to store new z value if transform is moved 
    #endregion

    void Start()
    {
        #region Null check
        if (movingObject == null)
        {
            Debug.LogError("Moving object not set in inspector");
            return;
        }
        #endregion

        #region Reference to the original rotation of moving object
        originalRotation = movingObject.transform.rotation;
        #endregion

        #region Start at first node
        movingObject.transform.position = nodes[0];
        currentPoint = nodes[0];
        pointSelection = 0;
        #endregion
    }

    void Update()
    {
        #region Null Check
        if (movingObject == null)
        {
            return;
        }
        #endregion

        #region Setting the direction of moving object
        switch (objectDirection)
        {
            case Direction.up:
                targetDirection = Vector3.up;
                break;
            case Direction.down:
                targetDirection = Vector3.down;
                break;
            case Direction.left:
                targetDirection = Vector3.back;
                break;
            case Direction.right:
                targetDirection = Vector3.forward;
                break;
        }
        #endregion

        #region Allow movement only if
        if ((startTriggerEntered == true || useTriggerForStart == false) && stopMoving == false)
        {
            #endregion

            #region Moving object moves from current point towards to currentpoint
            movingObject.transform.position = Vector3.MoveTowards(movingObject.transform.position, currentPoint, Time.deltaTime * moveSpeed);
            #endregion

            #region Rotate towards Next Node
            if (rotateTowardsNextNode == true)
            {
                if (movingObject.transform.position != currentPoint)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(movingObject.transform.position - currentPoint, targetDirection);
                    targetRotation.x = 0.0f;
                    targetRotation.y = 0.0f;
                    movingObject.transform.rotation = Quaternion.Slerp(movingObject.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }
            }

            else
            {
                movingObject.transform.rotation = originalRotation;
            }
            #endregion

            #region Require new trigger enter if moving object is at the first node
            if (movingObject.transform.position == nodes[0])
            {
                startTriggerEntered = false;
            }
            #endregion

            #region Check if moving object reached current point
            if (movingObject.transform.position == currentPoint)
            {
                #endregion

                #region Stop for stop time at each stop given in stop nodes array
                foreach (int stop in stopNodes)
                {
                    if (pointSelection == stop)
                    {
                        StartCoroutine("WaitBeforeMoving");
                    }
                }
                #endregion

                #region Check if looping is allowed. Change to reverse at the end and back to forward movement in start
                if(loop == false)
                {
                    pointSelection++;
                }
                if (loop == true && pointSelection == nodes.Length -1)
                {
                    reverse = true;
                }
                if (loop == true && movingObject.transform.position == nodes[0])
                {
                    reverse = false;
                }
                if (loop == true && reverse == false)
                {
                    pointSelection++;
                }

                if (loop == true && reverse == true)
                {
                    pointSelection--;
                }
                #endregion

                #region Last node reached. If not looping move back to start normally or instantly move to start
                else if (pointSelection == nodes.Length && loop == false)
                {
                    if (moveToStartAtEnd == true)
                    {
                        movingObject.transform.position = nodes[0];
                        movingObject.transform.rotation = originalRotation;
                        pointSelection = 0;
                    }
                    else
                    {
                        pointSelection = 0;
                    }
                }
                #endregion

                #region Update currentPoint
                currentPoint = nodes[pointSelection];
                #endregion
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        #region Check if "Player" entered trigger zone
        if (other.gameObject.name == "Player")
        {
            startTriggerEntered = true;
        }
        #endregion
    }

    IEnumerator WaitBeforeMoving()
    {
        #region Stop for stop time at each stop given in stop nodes array
        stopMoving = true;
        yield return new WaitForSeconds(stopTime);
        stopMoving = false;
        #endregion
    }

    void OnDrawGizmosSelected()
    {
        #region If using trigger to start movement of moving object, add BoxCollider2D if there is no already one. Remove the BoxCollider2D if there is one and not using trigger to start movement of moving object
        if (gameObject.GetComponent<BoxCollider2D>() == null)
        {
            if (useTriggerForStart == true)
            {
                gameObject.AddComponent<BoxCollider2D>();
                gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
            }
        }
        else if(gameObject.GetComponent<BoxCollider2D>() != null)
        {
            if (useTriggerForStart == false)
            {
                DestroyImmediate(gameObject.GetComponent<BoxCollider2D>());
            }
        }
        #endregion

        #region Check if lines should be drawn
        if (drawLines == false || nodes == null)
        {
            return;
        }
        #endregion

        #region Check what icong should be drawn
        for (int i = 1; i < nodes.Length; i++)
        {
            if (useWhiteGizmo == true)
            {
                Gizmos.DrawIcon(nodes[i], "Icon White", false);
                Gizmos.DrawIcon(nodes[i - 1], "Icon White", false);
            }
            if (useBlueGizmo == true)
            {
                Gizmos.DrawIcon(nodes[i], "Icon Blue", false);
                Gizmos.DrawIcon(nodes[i - 1], "Icon Blue", false);
            }
            if (useRedGizmo == true)
            {
                Gizmos.DrawIcon(nodes[i], "Icon Red", false);
                Gizmos.DrawIcon(nodes[i - 1], "Icon Red", false);
            }
            if (useGreenGizmo == true)
            {
                Gizmos.DrawIcon(nodes[i], "Icon Green", false);
                Gizmos.DrawIcon(nodes[i - 1], "Icon Green", false);
            }
            if (useYellowGizmo == true)
            {
                Gizmos.DrawIcon(nodes[i], "Icon Yellow", false);
                Gizmos.DrawIcon(nodes[i - 1], "Icon Yellow", false);
            }
        }
        #endregion
    }
}