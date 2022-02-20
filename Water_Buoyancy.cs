using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water_Buoyancy : MonoBehaviour
{
    [SerializeField] private float waterDensity = 1f, waterDrag = 1f;
    [SerializeField] private Transform waterLine;

    private Rigidbody rig;
    private Object_Rigid scrObj;
    private Vector3[] vertices;
    private Vector3 verticesWorld, newVel;
    private Mesh mesh;
    private float vertexWeight, submergedVolume;

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

            if (verticesWorld.y < waterLine.position.y)
                submergedVolume += vertexWeight;
        }

        //Add buoyancy force
        rig.AddForce(Vector3.up * waterDensity * scrObj.objFg * scrObj.objFloatiness * submergedVolume, ForceMode.Force);

        //Apply drag
        newVel = rig.velocity;
        newVel *= 1f - waterDrag * (submergedVolume/ scrObj.objVolume);
        rig.velocity = newVel;
    }
}
