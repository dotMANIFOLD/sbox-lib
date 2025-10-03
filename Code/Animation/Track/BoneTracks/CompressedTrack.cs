using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.Animation {
    public abstract class CompressedTrack<T> : BoneTrack<T> {
        [ReadOnly, JsonIgnore]
        public T[] Data { get; set; }
        [ReadOnly]
        public int ElementCount { get; set; }
        [ReadOnly]
        public string ContentString { get; set; }
        
        [JsonIgnore]
        public override int FrameCount => Data?.Length ?? ElementCount;
        [JsonIgnore]
        public override bool Loaded => Data != null;
        
        public override T Get(int frame) {
            if (!Loaded) throw new InvalidOperationException("Data is not ready");
            ArgumentOutOfRangeException.ThrowIfLessThan(frame, 0, nameof(frame));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(frame, Data.Length, nameof(frame));

            return Data[frame];
        }

        public override T GetNext(int frame) {
            if (!Loaded) throw new InvalidOperationException("Data is not ready");
            ArgumentOutOfRangeException.ThrowIfLessThan(frame, 0, nameof(frame));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(frame, Data.Length, nameof(frame));
            
            return Data[Math.Min(frame + 1, Data.Length - 1)];
        }

        public override void CompressData() {
            if (Data == null) return;
            
            ElementCount = Data.Length;

            var stream = CompressInternal();
            ContentString = Convert.ToBase64String(stream.ToArray());
        }

        public override void Load() {
            byte[] byteArray = Convert.FromBase64String(ContentString);
            ByteStream stream = ByteStream.CreateReader(byteArray);

            Data = new T[ElementCount];
            int countResult = DecompressInternal(stream);
            
            if (countResult != ElementCount) {
                Log.Error($"Compressed {typeof(T).Name} span conversion length mismatch. Expected {ElementCount}, got {countResult}");
            }
        }

        public override void Unload() {
            Data = null;
        }

        protected abstract ByteStream CompressInternal();
        protected abstract int DecompressInternal(ByteStream data);
    }
}
