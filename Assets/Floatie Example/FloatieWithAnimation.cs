using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatieWithAnimation : Floatie {

    public float waitBeforeDestroy = 1f;

    public override void Destroy()
    {
        drawLine = false;

        var anim = GetComponentInChildren<Animator>();
        if (anim)
        {
            anim.SetBool("Open", false);
        }

        Destroy(gameObject, waitBeforeDestroy);
    }

}