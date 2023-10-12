using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TargetManager : MonoBehaviour
{
    [SerializeField]
    private SelectionManager selectionManager;

    [SerializeField]
    private GameObject targetPrefab;

    public List<Selectable> selectables = new();

    // Target customizer variables
    [SerializeField]
    [Tooltip("The offset in radians between the angle of each target. Set to -1 for an offset of 2 * pi / number of targets")]
    private float angleOffset = -1f;
    [SerializeField]
    [Tooltip("Base target speed")]
    private float targetSpeed = 1f;
    [SerializeField]
    [Tooltip("Speed of target with index i is calculated as: (targetSpeed + speedOffset * i) * speedMultiplier ^ i")]
    private float speedOffset = 0f;
    [SerializeField]
    [Tooltip("Speed of target with index i is calculated as: (targetSpeed + speedOffset * i) * speedMultiplier ^ i")]
    private float speedMultiplier = 1f;


    // Display Variables
    public float selectionDisplayTime = 2f;
    public Color defaultColor = Color.white;
    public Color hoverColor = Color.blue;
    public Color selectedColor = Color.green;
    
    // Hover and selection memory
    public int currentHoverId = -1;
    public int latestSelectId = -1;

    // Child and motion memory
    private int lastFrameChildCount = 0;
    private float lastFrameTargetSpeed = 1f;
    private float lastFrameSpeedOffset = 0f;
    private float lastFrameSpeedMultiplier = 1f;

    // Start is called before the first frame update
    void Start()
    {
        // If selection manager is null, get it from this object, if it doesn't exist, create it
        if (selectionManager == null && !TryGetComponent(out selectionManager))
            selectionManager = gameObject.AddComponent<SelectionManager>();

        ParseChildrenToSelectables();
        lastFrameChildCount = transform.childCount;

        if (angleOffset == -1)
            angleOffset = Mathf.PI * 2 / selectables.Count;

        SetTargetMotions(angleOffset, targetSpeed, speedOffset, speedMultiplier);

    }

    // Update is called once per frame
    void Update()
    {
        // If the number of children in the hierarchy has changed, add any new objects to the selectables
        if (transform.childCount != lastFrameChildCount)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.gameObject.GetComponent<Selectable>() == null)
                {
                    // Position each new object on either side of the row
                    child.position = new Vector3(4 * ((i + 1) / 2) * Mathf.Pow(-1, i), 0, 0);

                    Selectable s = child.gameObject.AddComponent<Selectable>();
                    s.targetPrefab = targetPrefab;
                    s.targetManager = GetComponent<TargetManager>();
                    
                    // Set the new target's motion, according to the other targets
                    s.currentAngle = selectables[^1].currentAngle + angleOffset;
                    s.angularSpeed = (selectables[^1].angularSpeed + speedOffset) * speedMultiplier;
                    
                    selectables.Add(s);
                    s.ListenForSelection(selectionManager);
                }
            }
            lastFrameChildCount = transform.childCount;
        }
        // If the speed parameters have changed, update the motions of the targets
        if (targetSpeed != lastFrameTargetSpeed || speedOffset != lastFrameSpeedOffset || speedMultiplier != lastFrameSpeedMultiplier)
        {
            for (int i = 0; i < selectables.Count; i++)
            {
                selectables[i].angularSpeed = (targetSpeed + speedOffset * i) * Mathf.Pow(speedMultiplier, i);
            }

            lastFrameTargetSpeed = targetSpeed;
            lastFrameSpeedOffset = speedOffset;
            lastFrameSpeedMultiplier = speedMultiplier;
        }
    }

    private void SetTargetMotions(float angOffset, float tarSpeed, float speOffset, float speMultiplier)
    {
        for (int i = 0; i < selectables.Count; i++)
        {
            selectables[i].currentAngle = i * angOffset;
            selectables[i].angularSpeed = (tarSpeed + speOffset * i) * Mathf.Pow(speMultiplier, i);
        }
    }

    /* For all children of the target manager, if they aren't a selectable object, turn them into one.
    Then, place them in line.
    Finally, make them listen to the selection manager */
    private void ParseChildrenToSelectables()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            // Position each object on either side of the row
            child.position = new Vector3(4 * ((i + 1) / 2) * Mathf.Pow(-1, i) , 0, 0);
            Selectable s = child.gameObject.GetComponent<Selectable>();
            if (s != null)
            {
                s.targetPrefab = targetPrefab;
                s.targetManager = GetComponent<TargetManager>();
                selectables.Add(s);
            }
            else
            {
                s = child.gameObject.AddComponent<Selectable>();
                s.targetPrefab = targetPrefab;
                s.targetManager = GetComponent<TargetManager>();
                selectables.Add(s);
            }
            s.ListenForSelection(selectionManager);
        }
    }

    public void ResolveHover(double xThreshHold, double yThreshHold, double averageThreshHold, bool rememberX, bool rememberY, bool rememberAverage)
    {
        int targetID = HighestCorrelationTarget(xThreshHold, yThreshHold, averageThreshHold, rememberX, rememberY, rememberAverage);
        if (targetID == -1)
            targetID = currentHoverId;

        HoverTarget(targetID);
        currentHoverId = targetID;
    }

    public void ResolveSelect()
    {
        latestSelectId = currentHoverId;
        currentHoverId = -1;
    }

    // Get the the target with the highest correlation that passes the threshold in the given parameters
    private int HighestCorrelationTarget(double xThreshHold, double yThreshHold, double averageThreshHold, bool rememberX, bool rememberY, bool rememberAverage)
    {
        int latest_id = -1;
        for (int i = 0; i < selectables.Count; i++)
        {
            double averageCor = (selectables[i].Xcorrelation + selectables[i].Ycorrelation) / 2;
            if (selectables[i].Xcorrelation > xThreshHold && selectables[i].Ycorrelation > yThreshHold && averageCor > averageThreshHold)
            {
                if (rememberX)
                    xThreshHold = selectables[i].Xcorrelation;
                if (rememberY)
                    yThreshHold = selectables[i].Ycorrelation;
                if (rememberAverage)
                    averageThreshHold = averageCor;
                latest_id = i;
            }
        }
        return latest_id;
    }

    private void HoverTarget(int targetID)
    {
        if (targetID >= 0 && targetID < selectables.Count)
            selectables[targetID].Hover();
    }
}
