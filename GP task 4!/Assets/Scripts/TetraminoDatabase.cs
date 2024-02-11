using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TetraminoProperty
{
    //The main use here is that we need an anchor point that the blocks will rotate around.

    public string name;

    public Vector2 rotationAnchorPoint;

    public Vector3 spawnOffset;
}

public class TetraminoDatabase : MonoBehaviour
{

    public TetraminoProperty[] tetraminoProperties;
 
}
