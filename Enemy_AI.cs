using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_AI : Unit_Base
{
    public Transform myOutpost;

    public override void Start()
    {
        sprWater = objWater.GetComponent<SpriteRenderer>();
        waterPos = objWater.transform.localPosition;
        myAudio = GetComponentInChildren<AudioSource>();

        InvokeRepeating("Thirst", 1f, 1f);
    }

    public override void UpdateWater()
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
            Die();
        }
    }

    private void Die()
    {
        Outpost_AI scrOP = myOutpost.GetComponent<Outpost_AI>();
        scrOP.RemoveUnit(transform);

        Destroy(gameObject);
    }
}
