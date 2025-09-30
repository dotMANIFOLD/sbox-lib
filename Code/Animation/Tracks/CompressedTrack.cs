using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.Animation {
    public abstract class CompressedTrack<T> : Track<T> {
        [JsonIgnore]
        public T[] Data { get; set; }
        public int ElementCount { get; set; }
        public string ContentString { get; set; }
        
        [JsonIgnore]
        public override int FrameCount => Data?.Length ?? ElementCount;
        [JsonIgnore]
        public override bool Ready => Data != null;
        
        public override T Get(int frame) {
            if (!Ready) throw new InvalidOperationException("Data is not ready");
            ArgumentOutOfRangeException.ThrowIfLessThan(frame, 0, nameof(frame));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(frame, Data.Length, nameof(frame));

            return Data[frame];
        }

        public override T GetNext(int frame) {
            if (!Ready) throw new InvalidOperationException("Data is not ready");
            ArgumentOutOfRangeException.ThrowIfLessThan(frame, 0, nameof(frame));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(frame, Data.Length, nameof(frame));
            
            return Data[Math.Min(frame + 1, Data.Length - 1)];
        }

        public override void Compress() {
            if (Data == null) return;
            
            ElementCount = Data.Length;

            var stream = CompressInternal();
            ContentString = Convert.ToBase64String(stream.ToArray());
        }

        public override void Decompress() {
            byte[] byteArray = Convert.FromBase64String(ContentString);
            ByteStream stream = ByteStream.CreateReader(byteArray);

            Data = new T[ElementCount];
            int countResult = DecompressInternal(stream);
            
            if (countResult != ElementCount) {
                Log.Error($"Compressed {typeof(T).Name} span conversion length mismatch. Expected {ElementCount}, got {countResult}");
            }
        }

        public override void Reset() {
            Data = null;
        }

        protected abstract ByteStream CompressInternal();
        protected abstract int DecompressInternal(ByteStream data);
    }
}
