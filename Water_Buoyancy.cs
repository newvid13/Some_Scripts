/*
 * Simple buoyancy simulation
 * This script gets added to your water object 
 * Water has to have a collider which is set to TRIGGER
 * Also added water drag
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water_Buoyancy : MonoBehaviour
{
    [SerializeField] float waterDensity = 1f, waterDrag = 0.1f;

    Rigidbody rig;
    Object_Rigid scrObj;
    Vector3[] vertices;
    Vector3 verticesWorld, newVel;
    Mesh mesh;
    float vertexWeight, submergedVolume, waterSurfaceLine;

    private void Start()
    {
        waterSurfaceLine = transform.position.y + (transform.localScale.y / 2f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody)
            AddForces(other.transform);
    }

    private void AddForces(Transform obj)
    {
        if (obj.GetComponent<Object_Rigid>() == null)
            return;

        scrObj = obj.GetComponent<Object_Rigid>();
        rig = obj.GetComponent<Rigidbody>();
        mesh = obj.GetComponent<MeshFilter>().mesh;

        vertices = mesh.vertices;
        vertexWeight = scrObj.objVolume / vertices.Length;

        //Calculate volume of submerged part
        submergedVolume = 0f;
        for (int i = 0; i < vertices.Length; i++)
        {
            verticesWorld = obj.transform.TransformPoint(vertices[i]);

            if (verticesWorld.y < waterSurfaceLine)
                submergedVolume += vertexWeight;
        }

        //Add buoyancy force
        rig.AddForce(Vector3.up * waterDensity * scrObj.objFg * submergedVolume, ForceMode.Force);

        //Apply drag
        newVel = rig.velocity;
        newVel *= 1f - waterDrag * (submergedVolume/ scrObj.objVolume);
        rig.velocity = newVel;
    }
}
