using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEditor;

// Class that defines a selectable object. Gets all necessary information about user input from selection manager through Unity Events
public class Selectable : MonoBehaviour
{
    public TargetManager targetManager;

    public GameObject targetPrefab;
    private bool hasTarget;

    private Transform targetBall;

    // Target Parameters
    public float angularSpeed = 1f;
    public float circleRad = 4.5f;
    public float currentAngle = 0f;
    
    public double Xcorrelation { get; private set; }
    public double Ycorrelation { get; private set; }

    private List<double> targetXs = new();
    private List<double> targetYs = new();
    private List<double> inputXs = new();
    private List<double> inputYs = new();

    private Renderer painter;

    private bool hovered = false;

    public bool selected = false;
    private float timeSelected = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // Check if this object already has a target, if not, add one
        hasTarget = CheckChildrenForTarget(targetPrefab);
        if (!hasTarget)
            AddTargetComponent(targetPrefab);

        painter = GetComponent<Renderer>();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (targetManager == null)
            targetManager = FindObjectOfType<TargetManager>();

        // Make the target spin
        OrbitTarget(angularSpeed, circleRad);

        if (selected)
        {
            Paint(targetManager.selectedColor);
            timeSelected += Time.deltaTime;
            if (timeSelected > targetManager.selectionDisplayTime)
                Unselect();
        }
        else if (hovered)
            Paint(targetManager.hoverColor);
        else
            Paint(targetManager.defaultColor);
    }

    private bool CheckChildrenForTarget(GameObject prefab)
    {
        if (transform.childCount != 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).childCount == 1 && transform.GetChild(i).gameObject.name == "Target")
                {
                    targetBall = transform.GetChild(i).GetChild(0);
                    return true;
                }
            }
        }
        return false;
    }

    private void AddTargetComponent(GameObject prefab)
    {
        GameObject target = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
        float height = GetComponent<Renderer>().bounds.size.y;
        target.transform.localPosition = new Vector3(0, height + 0.5f, 0);
        targetBall = target.transform.GetChild(0);
    }

    public void ListenForSelection(SelectionManager selectionManager)
    {
        selectionManager.onSwipe.AddListener(AddCoordinates);
        selectionManager.onCorrelationTime.AddListener(Correlate);
        selectionManager.onSwipeEnd.AddListener(Select);
    }

    private void OrbitTarget (float angSpeed, float cirRad)
    {
        currentAngle += angSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle), 0) * cirRad;
        targetBall.localPosition = offset;
    }

    private void Unhover() {
        hovered = false;
    }

    public void Hover()
    {
        Unselect();
        hovered = true;
        Paint(targetManager.hoverColor);
    }

    private void Unselect()
    {
        timeSelected = 0f;
        selected = false;
    }

    void Select()
    {
        if (hovered)
        {
            hovered = false;
            selected = true;
        }
        ClearData();
    }
    
    private void Paint(Color color)
    {
        painter.material.color = color;
    }

    private void Correlate()
    {
        Xcorrelation = Pearsons(targetXs, inputXs);
        Ycorrelation = Pearsons(targetYs, inputYs);
        ClearData();
    }

    // Add coordintates of user input and this target's movement to the datasets
    private void AddCoordinates(double x, double y)
    {
        targetXs.Add(targetBall.localPosition.x);
        targetYs.Add(targetBall.localPosition.y);
        inputXs.Add(x);
        inputYs.Add(y);
    }

    private void ClearData()
    {
        targetXs.Clear();
        targetYs.Clear();
        inputXs.Clear();
        inputYs.Clear();
        Unhover();
    }

    // Calculate Pearsons Correlation Coefficient between two double lists
    private double Pearsons(List<double> touch, List<double> target)
    {
        double sx = 0.0, sy = 0.0, sxx = 0.0, syy = 0.0, sxy = 0.0;
        int n = touch.Count;
        //Debug.Log(n);

        for (int i = 0; i < n; ++i)
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
        double sigmax = Math.Sqrt(sxx / n - sx * sx / n / n);     // standard error of x
        double sigmay = Math.Sqrt(syy / n - sy * sy / n / n);     // standard error of y
        return cov / sigmax / sigmay;                              // correlation is just a normalized covariation
    }
}
