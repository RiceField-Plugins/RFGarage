using System;
using UnityEngine;

namespace RFGarage.Serialization
{
    // Big thanks to AdamAdam
    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;
        
        public SerializableVector3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);
    }
}