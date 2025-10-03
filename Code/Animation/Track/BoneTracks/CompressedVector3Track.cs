using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.Animation {
    public class CompressedVector3Track : CompressedTrack<Vector3> {
        protected override ByteStream CompressInternal() {
            var stream = ByteStream.Create(Data.Length * Half3.SIZE);
            for (int i = 0; i < Data.Length; i++) {
                stream.Write((Half3)Data[i]);
            }
            return stream;
        }

        protected override int DecompressInternal(ByteStream data) {
            int halfCount = data.Length / Half3.SIZE;
            for (int i = 0; i < halfCount; i++) {
                Data[i] = data.Read<Half3>();
            }
            return halfCount;
        }
    }
}
