using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoarding : MonoBehaviour
{

    void Start()
    {
        transform.forward = Camera.main.transform.forward;
    }
    void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }
}
