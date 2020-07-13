﻿using System;
using UnityEngine;

public class TacticsMovementCamera : MonoBehaviour
{
    public FacesCamera foucus;
    public Vector3 foucusPoint;
    public float speed = .5f;
    public Camera cam;
    public float offsetFromFoucus = 4;
    public float offsetFromZ0 = 4;

    static bool cameraMoved;

    private static float desieredAngle = (float)Math.PI;
    public static float angle = 0;

    private void Start()
    {
        Implement.IsTurnEvent += SetAsFoucus;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerInputs();
        FoucusInputs();
        SetSortMode();
        cam.orthographic = true;
    }

    private void FoucusInputs()
    {
        float lerpAngle = Mathf.Abs(desieredAngle - angle) > .5f ? Mathf.Lerp(angle, desieredAngle, .1f) : desieredAngle;
        transform.position = CalcPostion(lerpAngle);
        transform.rotation = CalcRotation(transform.position,lerpAngle);
        angle = lerpAngle;
    }

    private void PlayerInputs()
    {
        Vector3 desierdPosition = transform.position;
        if(Input.GetAxisRaw("Horizontal") != 0 && !cameraMoved)
        {
            desieredAngle -= Input.GetAxisRaw("Horizontal") * Mathf.PI/2;
            cameraMoved = true;
            //clamp between 0 and 360
            if(desieredAngle > Mathf.PI * 2)
            {
                desieredAngle -= Mathf.PI * 2;
                angle -= Mathf.PI * 2;
            }
            else if (desieredAngle < 0)
            {
                desieredAngle += Mathf.PI * 2;
                angle += Mathf.PI * 2;
            }
            //just in case the top bit dosent work;
            desieredAngle = Mathf.Clamp(desieredAngle, 0, Mathf.PI * 2);
        }
        else if(Input.GetAxisRaw("Horizontal") == 0)
        {
            cameraMoved = false;
        }
        transform.position = desierdPosition;
    }

    private Vector3 CalcPostion(float angle)
    {
        Vector3 offset = new Vector3((float)Math.Sin(angle), (float)Math.Cos(angle));
        offset *= offsetFromFoucus;
        offset.z = -offsetFromZ0;
        if (foucus != null)
        {
            foucusPoint = Vector3.Distance(foucusPoint, foucus.transform.position) > .5f
                ? Vector3.Lerp(foucusPoint, foucus.transform.position, .1f)
                : foucus.transform.position;
        }
        return foucusPoint + offset;
    }

    private Quaternion CalcRotation(Vector3 position, float angle)
    {
        Vector3 rotation = Quaternion.LookRotation(foucusPoint - position).eulerAngles;
        rotation.z = -angle * Mathf.Rad2Deg;
        return Quaternion.Euler(rotation);
    }

    private void SetSortMode()
    {
        cam.transparencySortMode = TransparencySortMode.CustomAxis;
        cam.transparencySortAxis = transform.up - transform.forward;
    }

    void SetAsFoucus(Implement unit)
    {
        foucus = unit;
    }
}
