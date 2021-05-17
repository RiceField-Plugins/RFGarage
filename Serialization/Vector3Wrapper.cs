using System;
using UnityEngine;

namespace VirtualGarage.Serialization
{
    // Big thanks to AdamAdam
    [Serializable]
    public struct Vector3Wrapper
    {
        public float x;
        public float y;
        public float z;
        
        public Vector3Wrapper(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);
    }
}