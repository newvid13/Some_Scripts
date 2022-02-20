using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitState
{
    Default,
    Retreat,
    Captured
}

public class Unit_Base : MonoBehaviour
{
    public float aiPriority;
    public UnitState myState = UnitState.Default;

    public int water;
    public float waterOffset;
    public Transform objWater;
    public SpriteRenderer sprWater;
    public Vector3 waterPos;
    public Color waterBlue, waterRed;

    public Unit_Move scrMove;
    public Transform objManager;
    public Main_Manager scrManager;
    public Transform visBody;
    public MeshRenderer visionCone;

    public AudioSource myAudio;
    public AudioClip[] audioClips;

    public virtual void Start()
    {
        sprWater = objWater.GetComponent<SpriteRenderer>();
        waterPos = objWater.transform.localPosition;
        scrMove = GetComponent<Unit_Move>();
        scrManager = objManager.GetComponent<Main_Manager>();
        myAudio = GetComponentInChildren<AudioSource>();

        UpdateWater();
        InvokeRepeating("Thirst", 1f, 1f);
    }

    public virtual void Drink()
    {
        water += 5;
        UpdateWater();
    }

    public virtual void Thirst()
    {
        if(myState == UnitState.Default)
        {
            water-=3;
            UpdateWater();
        }
    }

    public virtual void UpdateWater()
    {
        water = Mathf.Clamp(water, -100, 101);

        if (water < 0)
        {
            sprWater.color = waterRed;
            waterPos.y = -4.3f + (float)water * -waterOffset;
        }
        else
        {
            sprWater.color = waterBlue;
            waterPos.y = -4.3f + (float)water * waterOffset;
        }

        objWater.transform.localPosition = waterPos;

        if (water < -99)
        {
            if(aiPriority < 4)
            {
                scrManager.RemoveUnit(transform);
            }

            scrMove.Die();
        }
    }

    public virtual void Damage(int dmg)
    {
        water -= dmg;
        UpdateWater();
    }

    public virtual int PathCalc()
    {
        int reWater = 100 + water;
        return reWater;
    }

    public void PrisonRelease()
    {
        myState = UnitState.Default;
        transform.tag = "Player";
        visBody.gameObject.layer = 0;

        if (aiPriority < 4 && aiPriority != 0)
        {
            scrManager.AddUnit(transform);
            visionCone.enabled = true;
        }
    }

    public void PlayS(int c)
    {
        scrManager.SoundUI(c);
    }

    public void UnitSound(int type)
    {
        switch (type)
        {
            case 1:
                //move
                myAudio.pitch = Random.Range(0.95f, 1.05f);
                myAudio.PlayOneShot(audioClips[Random.Range(0,2)]);
                break;
            case 2:
                //fire
                myAudio.pitch = Random.Range(0.95f, 1f);
                myAudio.PlayOneShot(audioClips[2]);
                break;
            case 3:
                //special
                myAudio.pitch = Random.Range(0.95f, 1.05f);
                myAudio.PlayOneShot(audioClips[3]);
                break;
            default:
                break;
        }
    }
}
