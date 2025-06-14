using System;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    public class LinearBlendingJob : BlendingJob {
        private float[] blendPoints;
        private Parameter<float> parameter;
        private float blend;
        
        public Parameter<float> BlendParameter {
            get => parameter;
            set {
                if (parameter != null) {
                    parameter.OnChanged -= OnParameterChanged;
                    if (value == null) {
                        blend = parameter.Value;
                    }
                }
                parameter = value;
                if (parameter != null) {
                    parameter.OnChanged += OnParameterChanged;
                }
                RecalculateWeights();
            }
        }
        public float BlendValue {
            get => parameter?.Value ?? blend;
            set {
                blend = value;
                parameter = null;
                RecalculateWeights();
            }
        }
        
        public LinearBlendingJob(int layerCount) : base(layerCount) {
            blendPoints = new float[layerCount];
        }

        public LinearBlendingJob(Guid id, int layerCount) : base(id, layerCount) {
            blendPoints = new float[layerCount];
        }

        public LinearBlendingJob(Guid id, float[] blendPoints) : base(id, blendPoints.Length) {
            this.blendPoints = blendPoints;
        }

        public void SetBlendPoint(int index, float value) {
            if (index < 0 || index >= blendPoints.Length) throw new IndexOutOfRangeException();
            blendPoints[index] = value;
            RecalculateWeights();
        }
        
        public void RecalculateWeights() {
            if (weights.Length > 1) {
                float closestLeft = -1000000;
                int leftIndex = 0;
                float closestRight = 1000000;
                int rightIndex = weights.Length - 1;

                for (int i = 0; i < weights.Length; i++) {
                    weights[i] = 0;
                    float blendPoint = blendPoints[i];
                    if (BlendValue > blendPoint && blendPoint > closestLeft) {
                        leftIndex = i;
                        closestLeft = blendPoint;
                    } else if (BlendValue < blendPoint && blendPoint < closestRight) {
                        rightIndex = i;
                        closestRight = blendPoint;
                    }
                }
                float rightWeight = BlendValue.LerpInverse(blendPoints[leftIndex], blendPoints[rightIndex]);
                weights[rightIndex] = rightWeight;
                weights[leftIndex] = 1;
            }
        }

        private void OnParameterChanged() {
            RecalculateWeights();
        }
    }
}
