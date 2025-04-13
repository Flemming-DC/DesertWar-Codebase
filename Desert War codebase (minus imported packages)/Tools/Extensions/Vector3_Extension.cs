using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3_Extension
{
    
    public static Vector3 Horizontal(this Vector3 vector3)
    {
        return new Vector3(vector3.x, 0, vector3.z);
    }

}
