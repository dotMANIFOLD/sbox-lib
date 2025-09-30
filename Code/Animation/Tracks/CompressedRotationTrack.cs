using System;
using System.Linq;
using Sandbox;

namespace MANIFOLD.Animation {
    public class CompressedRotationTrack : CompressedTrack<Rotation> {
        public const int ELEMENT_SIZE = 6;
        
        public const float FLOAT_MIN = -0.707107f;
        public const float FLOAT_MAX = 0.707107f;
        
        public const byte COMPONENT_BIT_COUNT = 15;
        public const ushort COMPONENT_MIN = 0;
        public const ushort COMPONENT_MAX = 0b111_1111_1111_1111;
        
        protected override ByteStream CompressInternal() {
            var stream = ByteStream.Create(Data.Length * ELEMENT_SIZE);
            for (int i = 0; i < Data.Length; i++) {
                Rotation rotation = Data[i];
                var decomposed = Decompose(rotation);
                if (decomposed.largest < 0) decomposed.without *= -1;
                
                ushort a, b, c;
                a = ToUShort(decomposed.without.x);
                b = ToUShort(decomposed.without.y);
                c = ToUShort(decomposed.without.z);

                ulong temp = 0;
                temp |= c;
                temp |= (ulong)b << COMPONENT_BIT_COUNT;
                temp |= (ulong)a << (COMPONENT_BIT_COUNT * 2);
                temp |= (ulong)decomposed.index << (COMPONENT_BIT_COUNT * 3);
                byte[] tempBytes = BitConverter.GetBytes(temp);

                stream.Write(tempBytes, 0, ELEMENT_SIZE);
            }
            return stream;
        }

        protected override int DecompressInternal(ByteStream data) {
            int count = data.Length / ELEMENT_SIZE;
            byte[] buffer = new byte[8];
            for (int i = 0; i < count; i++) {
                data.Read(buffer, 0, ELEMENT_SIZE);
                
                ulong temp = BitConverter.ToUInt64(buffer, 0);
                uint index = (uint)(temp >> (COMPONENT_BIT_COUNT * 3));
                float a = ToFloat((ushort)((temp >> (COMPONENT_BIT_COUNT * 2)) & COMPONENT_MAX));
                float b = ToFloat((ushort)((temp >> COMPONENT_BIT_COUNT) & COMPONENT_MAX));
                float c = ToFloat((ushort)(temp & COMPONENT_MAX));
                
                float d = MathF.Sqrt(1 - a*a - b*b - c*c);

                Rotation result = index switch {
                    0 => new Rotation(d, a, b, c),
                    1 => new Rotation(a, d, b, c),
                    2 => new Rotation(a, b, d, c),
                    _ => new Rotation(a, b, c, d)
                };
                Data[i] = result;
            }
            return count;
        }

        private (uint index, float largest, Vector3 without) Decompose(Rotation value) {
            Vector4 abs = new Vector4(MathF.Abs(value.x), MathF.Abs(value.y), MathF.Abs(value.z), Math.Abs(value.w));
            
            uint largestIndex = 0;
            float largestValue = value.x;
            float largestAbs = abs.x;
            Vector3 withoutLargest = new Vector3(value.y, value.z, value.w);
            
            if (abs.y > largestAbs) {
                largestIndex = 1;
                largestValue = value.y;
                largestAbs = abs.y;
                withoutLargest = new Vector3(value.x, value.z, value.w);
            }
            if (abs.z > largestAbs) {
                largestIndex = 2;
                largestValue = value.z;
                largestAbs = abs.z;
                withoutLargest = new Vector3(value.x, value.y, value.w);
            }
            if (abs.w > largestAbs) {
                largestIndex = 3;
                largestValue = value.w;
                largestAbs = abs.w;
                withoutLargest = new Vector3(value.x, value.y, value.z);
            }
            
            return (largestIndex, largestValue, withoutLargest);
        }

        private ushort ToUShort(float value) {
            int targetRange = COMPONENT_MAX - COMPONENT_MIN;
            float valueRange = FLOAT_MAX - FLOAT_MIN;
            float valueRelative = value - FLOAT_MIN;
            return (ushort)(COMPONENT_MIN + (ushort)(valueRelative / valueRange * targetRange));
        }

        private float ToFloat(ushort value) {
            float targetRange = FLOAT_MAX - FLOAT_MIN;
            ushort valueRange = COMPONENT_MAX - COMPONENT_MIN;
            ushort valueRelative = (ushort)(value - COMPONENT_MIN);
            return FLOAT_MIN + (valueRelative / (float)valueRange * targetRange);
        }
    }
}
