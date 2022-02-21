/*
 * Simulates wind force on leaves in windRadius
 * Moves randomly around the level
 * Wrote this during a game jam so it needs some work
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RealWind : MonoBehaviour
{
    public LayerMask rigidMask;
    private Collider[] Cols;
    [SerializeField] private float windForce, windRadius;
    private bool isOff = true, audioReady = true;
    private Rigidbody leafRig;
    private Vector3 windDirection;

    public LayerMask colMask;
    private Vector3 rot, newPos;
    private Quaternion newRot = Quaternion.identity;
    private float time, dist;
    public float minWait, maxWait;

    private AudioSource myAudio;
    public AudioClip[] windClips;

    void Start()
    {
        myAudio = GetComponent<AudioSource>();
        
        InvokeRepeating("CheckRadius", 0.1f, 0.1f);
        StartCoroutine(waitMove());
    }
    
    //gets called 10 times a second
    private void CheckRadius()
    {
        if (isOff)
            return;

        windDirection = (transform.forward*0.5f) + Vector3.up;
        Cols = Physics.OverlapSphere(transform.position, windRadius, rigidMask);

        if(audioReady && Cols.Length > 3)
        {
            StartCoroutine(waitAudio(Cols.Length));
        }

        foreach (var hitCollider in Cols)
        {
            if (hitCollider.GetComponent<Leaf>() != null)
            {
                leafRig = hitCollider.GetComponent<Rigidbody>();
                leafRig.AddForce(windDirection * windForce, ForceMode.Force);
            }
        }
    }

    //moves the wind object around the level
    IEnumerator waitMove()
    {
        time = Random.Range(minWait, maxWait);
        yield return new WaitForSeconds(time);

        rot = new Vector3(0, Random.Range(-180, 180), 0);
        newRot.eulerAngles = rot;
        transform.rotation = newRot;

        RaycastHit Hit;
        if (Physics.Raycast(transform.position, transform.forward, out Hit, 2.5f, colMask, QueryTriggerInteraction.Ignore))
        {
            dist = Vector3.Distance(transform.position, Hit.point);
            dist -= 0.2f;
        }
        else
        {
            dist = 2.3f;
        }

        time = Random.Range(1f, 2.5f);
        newPos = transform.position + transform.forward * dist;
        transform.DOMove(newPos, time).SetEase(Ease.InCubic);
        isOff = false;

        yield return new WaitForSeconds(time);
        isOff = true;
        StartCoroutine(waitMove());
    }
    
    //plays sound depending on how many leaves are within radius 
    IEnumerator waitAudio(int amount)
    {
        float vol = 0.6f;
        if(amount > 15)
        {
            vol = 1f;
        }
        audioReady = false;
        myAudio.PlayOneShot(windClips[Random.Range(0, windClips.Length)], Random.Range(0.8f, 1f) * vol);
        yield return new WaitForSeconds(6f);
        audioReady = true;
    }
}
