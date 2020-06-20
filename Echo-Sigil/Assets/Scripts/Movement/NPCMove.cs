﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMove : TacticsMove
{
    private Vector3 target;

    // Update is called once per frame
    void Update()
    {
        //Tactitics movement
        if (isTurn)
        {
            if (!moveing)
            {
                FindSelectableTiles();
                FindNearestTargetUnit();
                CalculatePath();
            }
            else
            {
                Move();
            }
        }
    }

    private void FindNearestTargetUnit()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

        GameObject nearest = null;
        float distance = Mathf.Infinity;

        foreach (GameObject obj in targets)
        {
            if (obj.GetComponent<Implement>())
            { 

                float d = Vector3.Distance(transform.position, obj.transform.position);

                if (d < distance)
                {
                    distance = d;
                    nearest = obj;
                }
            }
        }

        target = nearest.transform.position;
    }

    private void CalculatePath()
    {
        Tile targetTile = GetTargetTile(target);
        FindPath(targetTile);
    }
}
