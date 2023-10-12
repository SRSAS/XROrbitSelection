using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    public float AngularSpeed { get; set; } = 1f;
    public float CircleRad { get; set; } = 4.5f;
    public float CurrentAngle { get; set; } = 0f;

    // Update is called once per frame
    void Update()
    {
        CurrentAngle += AngularSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Sin(CurrentAngle), Mathf.Cos(CurrentAngle), 0) * CircleRad;
        transform.localPosition = offset;
    }
}
