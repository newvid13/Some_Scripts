/*
 * Added to any rigidbody object
 * Calculates object Volume and Gravitational Force(Fg)
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Object_Rigid : MonoBehaviour
{
    private Rigidbody myRig;
    public float objVolume, objFg, objFloatiness;

    void Start()
    {
        myRig = GetComponent<Rigidbody>();
        SetupValues();
    }

    private void SetupValues()
    {
        float orgMass = myRig.mass;
        myRig.SetDensity(3f);
        objVolume = myRig.mass / 3f;
        myRig.mass = orgMass;

        objFg = Mathf.Abs(myRig.mass * Physics.gravity.y);
    }
}
