﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LBlock : Block
{
    protected override void RotateBlock()
    {
        transform.Rotate(Vector3.forward * 90.0f);
    }

}