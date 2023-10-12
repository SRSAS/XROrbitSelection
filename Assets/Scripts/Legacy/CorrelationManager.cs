using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class CorrelationManager : MonoBehaviour
{
    public SocketManagerThreading socketManager;

    // Target pool
    private List<Transform> targets;
    private int target_count;

    // Data
    private List<float> targetInitialAngles;
    private List <float> targetFinalAngles;
    private List<List<double>> targetXs;
    private List<List<double>> targetYs;
    private List<double> touchX;
    private List<double> touchY;
    
    // Target variables
    [SerializeField]
    private float angleOffset = 3;
    [SerializeField]
    private float targetSpeed = 4;
    private float circleRad;

    // Correlation variables
    [SerializeField]
    private float SELECT_THRESHOLD = 0.3f;
    [SerializeField]
    private float SELECTION_TIME = 0.6f;
    private int latest_target_id = -1;
    private float latest_attempt = 0.0f;
    private float time_since_movement = 0f;

    // State for managing the display of the highlighted target
    private bool highlighted = false;
    private int targetHighlighted = -1;

    // State for managing the display of the selected target
    private bool selected = false;
    private int targetSelected = -1;
    private float timeSelected = 0f;
    [SerializeField]
    private float SELECTION_SHOW_TIME = 2f;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize lists
        targets = new List<Transform>();
        targetInitialAngles = new List<float>();
        targetFinalAngles = new List<float>();
        targetXs = new List<List<double>>();
        targetYs = new List<List<double>>();
        touchX = new List<double>();
        touchY = new List<double>();

        // Get target count
        target_count = transform.childCount;
        // Get targets
        for(int i = 0; i < target_count; i++){
            GameObject ball = transform.GetChild(i).GetChild(0).GetChild(0).gameObject;
            ball.GetComponent<Rotation>().CurrentAngle = i * angleOffset;
            ball.GetComponent<Rotation>().AngularSpeed = targetSpeed;
            targets.Add(ball.transform);     // Add all balls' tranforms to target list (containing x y coordinates)
            // Initialize lists
            targetXs.Add(new List<double>());
            targetYs.Add(new List<double>());
        }
        // Get circle radius
        circleRad = targets[0].gameObject.GetComponent<Rotation>().CircleRad;
    }

    // Update is called once per frame
    void Update()
    {
        // Countdown to stopshowing last selected object
        if(selected) {
            timeSelected += Time.deltaTime;
            if(timeSelected > SELECTION_SHOW_TIME) {
                removeSelection();
            }
        }
        // Get access to async input data
        lock (socketManager.obj) {
            // If user is performing a swipe, start registering input and target angles, and countdown until correlation calculation
            if (socketManager.touching) {
                time_since_movement += Time.deltaTime;

                touchX.Add(socketManager.x);
                touchY.Add(socketManager.y);
                for(int i = 0; i < target_count; i++) {
                    targetXs[i].Add(targets[i].localPosition.x);
                    targetYs[i].Add(targets[i].localPosition.y);
                }
                // If enough time has elapsed since last attempt, calculate correlations
                if (time_since_movement > latest_attempt + SELECTION_TIME) {
                    //double smallestX = SELECT_THRESHOLD;
                    //double smallestY = SELECT_THRESHOLD;
                    double highestCor = SELECT_THRESHOLD;
                    
                    // Calculate correlations for each target and check which pass threshold and have highest correlation
                    for(int i = 0; i < target_count; i++) {
                        double xCor = pearsons(touchX, targetXs[i]);
                        double yCor = pearsons(touchY, targetYs[i]);
                        double average = (xCor + yCor) / 2.0;

                        if(average > highestCor) {
                            latest_target_id = i;
                            highestCor = average;
                        }
                    }

                    // Highlight the correct target
                    if( latest_target_id != targetHighlighted) {
                        removeHighlight();
                        highlightTarget();
                    }

                    // Register attempt time
                    latest_attempt = time_since_movement;

                    clearAllSeries();
                }
            }
            // User has stopped pressing
            else {
                // If a target was highlighted, select it
                if(latest_target_id != -1) {
                    selectTarget();
                }

                // Reset everything
                latest_attempt = 0f;
                time_since_movement = 0f;
                latest_target_id = -1;
                clearAllSeries();
            }
        }

    }

    // Reset the selected object to the normal color
    private void removeSelection() {
        if(selected) {
            transform.GetChild(targetSelected).gameObject.GetComponent<Renderer>().material.color = Color.white;
            selected = false;
            targetSelected = -1;
        }
    }

    // Set the color of the highlighted object to green
    private void selectTarget() {
        transform.GetChild(targetHighlighted).gameObject.GetComponent<Renderer>().material.color = Color.green;
        highlighted = false;
        selected = true;
        targetSelected = targetHighlighted;
        targetHighlighted = -1;
        timeSelected = 0f;
    }

    // Reset the highlighted object to the normal color
    private void removeHighlight() {
        if(highlighted){
            transform.GetChild(targetHighlighted).gameObject.GetComponent<Renderer>().material.color = Color.white;
            highlighted = false;
            targetHighlighted = -1;
        }
    }

    // Set the color of the target with the highest correlation's object to blue
    private void highlightTarget() {
        transform.GetChild(latest_target_id).gameObject.GetComponent<Renderer>().material.color = Color.blue;
        targetHighlighted = latest_target_id;
        highlighted = true;
    }

    // Generate target coordinates based on the targets' initial and final angles and the number of input datapoints
    private void generateTargetSeries() {
        int n = touchX.Count;
        Debug.Log(n);
        for (int i = 0; i < target_count; i++) {
            float angleIncrement = (targetFinalAngles[i] - targetInitialAngles[i]) / (n - 1);
            for (int j = 0; j < n; j++) {
                targetXs[i].Add(Mathf.Sin(targetInitialAngles[i] + angleIncrement*j) * circleRad);
                targetYs[i].Add(Mathf.Cos(targetInitialAngles[i] + angleIncrement*j) * circleRad);
            }
        }
    }

    // Clear all data
    private void clearAllSeries() {
        for(int i = 0; i < target_count; i++) {
            targetXs[i].Clear();
            targetYs[i].Clear();
        }
        targetInitialAngles.Clear();
        targetFinalAngles.Clear();
        touchX.Clear();
        touchY.Clear();
        /*lock (socketManager.obj) {
            socketManager.register = false;
            socketManager.delete = true;
        }*/
    }

    // Calculate Pearsons Correlation Coefficient between two double lists
    private double pearsons(List<double> touch, List<double> target)
    {
        double sx = 0.0, sy = 0.0, sxx = 0.0, syy = 0.0, sxy = 0.0;
        int n = touch.Count;
        //Debug.Log(n);

        for(int i = 0; i < n; ++i)
        {
            double x = touch[i];
            double y = target[i];

            sx += x;
            sy += y;
            sxx += x * x;
            syy += y * y;
            sxy += x * y;
        }

        double cov = sxy / n - sx * sy / n / n;                    // covariation
        double sigmax = Math.Sqrt(sxx / n -  sx * sx / n / n);     // standard error of x
        double sigmay = Math.Sqrt(syy / n -  sy * sy / n / n);     // standard error of y
        return cov / sigmax / sigmay;                              // correlation is just a normalized covariation
    }

}
