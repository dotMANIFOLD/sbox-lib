using System;
using System.Runtime.InteropServices;

namespace MANIFOLD {
    [StructLayout(LayoutKind.Sequential, Size = 6)]
    public struct Half3 {
        public const int SIZE = 6;
        
        public Half X { get; set; }
        public Half Y { get; set; }
        public Half Z { get; set; }

        public Half3(float x, float y, float z) {
            X = (Half)x;
            Y = (Half)y;
            Z = (Half)z;
        }

        public static implicit operator Half3(Vector3 v) => new Half3(v.x, v.y, v.z);
        public static implicit operator Vector3(Half3 v) => new Vector3((float)v.X, (float)v.Y, (float)v.Z);
    }
}
