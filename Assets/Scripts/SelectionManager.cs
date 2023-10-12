using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Class to manage the interaction between selectable targets and the user input, as well as to differentiate between the many targets
public class SelectionManager : MonoBehaviour
{
    [SerializeField]
    private SocketManagerThreading socketManager;
    [SerializeField]
    private TargetManager targetManager;
    
    // Event for test information
    public UnityEvent testEvent = new();

    // Events
    public UnityEvent<double, double> onSwipe = new();
    public UnityEvent onSwipeEnd = new();
    public UnityEvent onCorrelationTime = new();

    private bool hasTouched = false;

    // Correlation Variables
    [SerializeField]
    private float xThreshold = 0.8f;
    [SerializeField]
    private float yThreshold = 0.8f;
    [SerializeField]
    private float avgThreshold = 0.8f;
    [SerializeField]
    private bool rememberX = true;
    [SerializeField]
    private bool rememberY = true;
    [SerializeField]
    private bool rememberAVG = true;
    [SerializeField]
    private float selectionTime = 0.5f;
    private float swipeTime = 0f;
    private float latestAttempt = 0f;


    // Start is called before the first frame update
    void Start()
    {
        // If either socket manager or target manager are null, get them from this object, if they don't exist, create them
        if (socketManager == null && !TryGetComponent(out socketManager))
            socketManager = gameObject.AddComponent<SocketManagerThreading>();
        
        if (targetManager == null && !TryGetComponent(out targetManager))
            targetManager = gameObject.AddComponent<TargetManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get access to async input data
        lock (socketManager.obj)
        {
            if (socketManager.touching)
            {
                hasTouched = true;
                // If the user is doing the swipe, send the coordinates to each target
                onSwipe.Invoke(socketManager.x, socketManager.y);

                swipeTime += Time.deltaTime;
                // If enough time has passed, tell each target to calculate the correlation with the user's swipe
                if (swipeTime > latestAttempt + selectionTime)
                {
                    latestAttempt = swipeTime;
                    onCorrelationTime.Invoke();

                    targetManager.ResolveHover(xThreshold, yThreshold, avgThreshold, rememberX, rememberY, rememberAVG);
                    
                    testEvent.Invoke();

                }
            }
            else
            {
                // If the user has stopped swiping, select a hovered object if there are any, and reset everything
                if (hasTouched) {
                    onSwipeEnd.Invoke();
                    targetManager.ResolveSelect();
                    hasTouched = false;
                }
                swipeTime = 0f;
                latestAttempt = 0f;
            }
        }
    }
}
