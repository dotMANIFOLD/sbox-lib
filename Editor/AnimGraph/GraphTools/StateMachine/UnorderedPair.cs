using System;

namespace MANIFOLD.AnimGraph.GraphTools {
    public readonly struct UnorderedPair<T> : IEquatable<UnorderedPair<T>> where T : IEquatable<T> {
        public T A { get; }
        public T B { get; }

        public UnorderedPair(T a, T b) {
            A = a;
            B = b;
        }

        public bool Equals(UnorderedPair<T> other) {
            return A.Equals(other.A) && B.Equals(other.B) || A.Equals(other.B) && B.Equals(other.A);
        }

        public override int GetHashCode() {
            return A.GetHashCode() ^ B.GetHashCode();
        }
    }
}
