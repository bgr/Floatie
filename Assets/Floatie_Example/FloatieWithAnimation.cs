using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatieWithAnimation : Floatie {

    public override void OnAboutToBeDestroyed()
    {
        drawLine = false;

        var anim = GetComponentInChildren<Animator>();
        if (anim)
        {
            anim.SetBool("Open", false);
        }
    }
}