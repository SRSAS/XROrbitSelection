using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class TestManager : MonoBehaviour
{
    [SerializeField]
    private SelectionManager selectionManager;
    [SerializeField]
    private TargetManager targetManager;
    [SerializeField]
    private Selectable target;
    private int targetId;
    [SerializeField]
    private TMP_Text text;

    private List<SwipeData> swipes = new();
    private SwipeData currentSwipe;
    private int swipeNumber = 1;

    private int lastHovered = -1;
    private float hoverStartTime = 0f;
    private float hoverTime = 0f;

    private float swipeStartTime = 0f;
    private float swipeTime = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        // If either selection manager or target manager are null, get them from this object, if they don't exist, create them
        if (selectionManager == null && !TryGetComponent(out selectionManager))
            selectionManager = gameObject.AddComponent<SelectionManager>();

        if (targetManager == null && !TryGetComponent(out targetManager))
            targetManager = gameObject.AddComponent<TargetManager>();

        SelectTarget();
        currentSwipe = new SwipeData(swipeNumber, targetId);
        swipeNumber++;

        selectionManager.onSwipe.AddListener(SwipeBegin);
        selectionManager.testEvent.AddListener(HoverCheck);
        selectionManager.onSwipeEnd.AddListener(SwipeEnd);
    }

    void SelectTarget()
    {
        // Select a random target from the selectionManager's list of targets
        targetId = Random.Range(0, targetManager.selectables.Count);
        target = targetManager.selectables[targetId];
        if (text != null)
            text.text = System.String.Format("Please select the {0}", target.name);
    }

    void SwipeBegin(double _, double __)
    {
        if (swipeStartTime == 0)
            swipeStartTime = Time.realtimeSinceStartup;
    }

    void HoverCheck()
    {
        if (lastHovered != targetManager.currentHoverId)
        {
            if (lastHovered != -1) {
                hoverTime = Time.realtimeSinceStartup - hoverStartTime;
                // Register hover
                currentSwipe.hoveredIds.Add(lastHovered);
                currentSwipe.hoveredTimes.Add(hoverTime);
            }
            lastHovered = targetManager.currentHoverId;
            if (targetManager.currentHoverId != -1) {
                hoverStartTime = Time.realtimeSinceStartup;
            }
        }
    }
    
    void SwipeEnd()
    {
        if (lastHovered != -1)
        {
            hoverTime = Time.realtimeSinceStartup - hoverStartTime;
            // Register hover
            currentSwipe.hoveredIds.Add(lastHovered);
            currentSwipe.hoveredTimes.Add(hoverTime);
        }
        swipeTime = Time.realtimeSinceStartup - swipeStartTime;
        currentSwipe.swipeTime = swipeTime;
        currentSwipe.selectedId = targetManager.latestSelectId;

        lastHovered = -1;
        swipeStartTime = 0;

        Debug.Log(currentSwipe.ToString());

        swipes.Add(currentSwipe);
        
        if (currentSwipe.selectedId == targetId)
            SelectTarget();
            if (text != null)
                text.text = System.String.Format("Please select the {0}", target.name);

        currentSwipe = new SwipeData(swipeNumber, targetId);
        swipeNumber++;
    }
}

public struct SwipeData
{
    public int id;
    public int targetId;
    public List<int> hoveredIds;
    public List<float> hoveredTimes;
    public int selectedId;
    public float swipeTime;

    public SwipeData(int swipeId, int target)
    {
        id = swipeId;
        targetId = target;
        hoveredIds = new List<int>();
        hoveredTimes = new List<float>();
        selectedId = -1;
        swipeTime = 0;
    }

    public override readonly string ToString()
    {
        StringBuilder sb = new(System.String.Format("SWIPE REPORT\nSwipe number: {0}\nTarget Goal: {1}\nHovered Over {2} Targets\n", id, targetId, hoveredIds.Count));
        int count = 0;
        float timeOverTarget = 0f;
        for (int i = 0; i < hoveredIds.Count; i++)
        {
            if (hoveredIds[i] == targetId)
            {
                count++;
                timeOverTarget += hoveredTimes[i];
            }
        }
        sb.AppendLine(System.String.Format("Hovered Over the Goal {0} Times", count));
        sb.AppendLine(System.String.Format("Hovered Over the Goal for {0} Seconds", timeOverTarget));
        sb.AppendLine(System.String.Format("Swipe Lasted {0} Seconds", swipeTime));
        if (selectedId == targetId)
            sb.AppendLine("The CORRECT Target Was Selected");
        else
        {
            sb.AppendLine("The WRONG Target Was Selected");
            sb.AppendLine(System.String.Format("The Selected Target Was: {0}", selectedId));
        }
        sb.AppendLine("All Hovers:");
        for (int i = 0; i < hoveredIds.Count; i++)
        {
            sb.AppendLine(System.String.Format("Hovered Over Target Number {0} For {1} Seconds", hoveredIds[i], hoveredTimes[i]));
        }
        sb.AppendLine("----- END -----");
            return sb.ToString();
    }
}
