using System;
using System.Collections.Generic;
using System.Linq;

namespace MANIFOLD.AnimGraph.Jobs {
    public class PlanarBlendJob : BlendingJob {
        private Vector2[] blendPoints;
        private float[][] blendMatrix;
        private float[] queryDistances;
        
        private Parameter<float> xParameter;
        private Parameter<float> yParameter;

        public Vector2 BlendPosition => new Vector2(xParameter?.Value ?? 0f, yParameter?.Value ?? 0f);
        
        public PlanarBlendJob(int layerCount) : base(layerCount) {
            blendPoints = new Vector2[layerCount];
            
            blendMatrix = new float[layerCount][];
            for (int i = 0; i < layerCount; i++) {
                blendMatrix[i] = new float[layerCount];
            }
            queryDistances = new float[layerCount];
        }

        public PlanarBlendJob(Guid id, int layerCount) : base(id, layerCount) {
            blendPoints = new Vector2[layerCount];
            
            blendMatrix = new float[layerCount][];
            for (int i = 0; i < layerCount; i++) {
                blendMatrix[i] = new float[layerCount];
            }
            queryDistances = new float[layerCount];
        }

        public PlanarBlendJob(Guid id, IReadOnlyList<Vector2> points) : base(id, points.Count) {
            blendPoints = points.ToArray();
            
            blendMatrix = new float[points.Count][];
            for (int i = 0; i < points.Count; i++) {
                blendMatrix[i] = new float[points.Count];
            }
            queryDistances = new float[points.Count];
        }

        public Parameter<float> XParameter {
            get => xParameter;
            set {
                if (xParameter != null) {
                    xParameter.OnChanged -= OnParameterChanged;
                }
                xParameter = value;
                if (xParameter != null) {
                    xParameter.OnChanged += OnParameterChanged;
                }
            }
        }

        public Parameter<float> YParameter {
            get => yParameter;
            set {
                if (yParameter != null) {
                    yParameter.OnChanged -= OnParameterChanged;
                }
                yParameter = value;
                if (yParameter != null) {
                    yParameter.OnChanged += OnParameterChanged;   
                }
            }
        }

        public override void SetLayerCount(int count) {
            int delta = count - weights.Length;
            if (delta == 0) return;
            
            base.SetLayerCount(count);
            Array.Resize(ref blendPoints, count);
            Array.Resize(ref blendMatrix, count);
            Array.Resize(ref queryDistances, count);
            
            if (delta > 0) {
                for (int i = count - delta - 1; i < count; i++) {
                    blendMatrix[i] = new float[count];
                }
            }
            for (int i = 0; i < count; i++) {
                Array.Resize(ref blendMatrix[i], count);
            }
        }

        public void SetBlendPoint(int index, Vector2 value) {
            if (index < 0 || index >= blendPoints.Length) throw new IndexOutOfRangeException();
            blendPoints[index] = value;
            RecalculateWeights();
        }
        
        public void RecalculateWeights() {
            UpdateBlendMatrix();
            UpdateQueryDistances();
            MatrixXVector(weights, blendMatrix, queryDistances);
            ClampNormalizeWeights();
        }

        private void UpdateBlendMatrix() {
            int animCount = blendPoints.Length;
            
            // Compute pair wise distances
            float[,] distances = new float[animCount, animCount];
            for (int i = 0; i < animCount; i++) {
                for (int j = 0; j < animCount; j++) {
                    distances[i, j] = Vector2.Distance(blendPoints[i], blendPoints[j]);
                }
            }

            // Stabilise decomposition by subtracting with epsilon
            for (int i = 0; i < animCount; i++) {
                distances[i, i] -= 1e-4f;
            }
            
            int[] rowOrder = new int[animCount];
            float[] rowScale = new float[animCount];
            
            bool success = LUDecomposeInplace(distances, rowOrder, rowScale);
            if (!success) throw new Exception("Decompose failed");
            
            // Reset blend matrix
            for (int i = 0; i < animCount; i++) {
                for (int j = 0; j < animCount; j++) {
                    blendMatrix[i][j] = i == j ? 1.0f : 0.0f;
                }
            }

            for (int i = 0; i < animCount; i++) {
                LUSolveInplace(blendMatrix[i], distances, rowOrder);
            }
        }

        private void UpdateQueryDistances() {
            for (int i = 0; i < blendPoints.Length; i++) {
                queryDistances[i] = Vector2.Distance(blendPoints[i], BlendPosition);
            }
        }
        
        /// <summary>
        /// LU in place decomposition
        /// </summary>
        /// <param name="matrix">Must be row major</param>
        /// <param name="rowOrder">Output row order</param>
        /// <param name="rowScale">Temp row scaling buffer</param>
        /// <returns>Success</returns>
        /// <exception cref="ArgumentException"></exception>
        private bool LUDecomposeInplace(float[,] matrix, int[] rowOrder, float[] rowScale) {
            int matrixRows = matrix.GetLength(0);
            int matrixColumns = matrix.GetLength(1);
            
            if (matrixColumns != matrixRows) throw new ArgumentException("matrixColumns != matrixRows");
            if (matrixRows != rowOrder.Length) throw new ArgumentException("matrixRows != rowOrder.Length");
            if (matrixColumns != rowScale.Length) throw new ArgumentException("matrixColumns != rowScale.Length");

            int n = matrixRows;
            
            // Compute scaling
            for (int row = 0; row < n; row++) {
                float vMax = 0.0f;
                for (int column = 0; column < n; column++) {
                    vMax = MathF.Max(vMax, MathF.Abs(matrix[row, column]));
                }
                if (vMax == 0.0f) return false;
                rowScale[row] = 1.0f / vMax;
            }
            
            // Loop using Crout's method
            for (int j = 0; j < n; j++) {
                for (int i = 0; i < j; i++) {
                    float sum = matrix[i, j];
                    for (int k = 0; k < i; k++) {
                        sum -= matrix[i, k] * matrix[k, j];
                    }
                    matrix[i, j] = sum;
                }
                
                // Search for largest pivot
                float vMax = 0.0f;
                int iMax = -1;
                for (int i = j; i < n; i++) {
                    float sum = matrix[i, j];
                    for (int k = 0; k < j; k++) {
                        sum -= matrix[i, k] * matrix[k, j];
                    }
                    matrix[i, j] = sum;
                    float val = rowScale[i] * MathF.Abs(sum);
                    if (val >= vMax) {
                        vMax = val;
                        iMax = i;
                    }
                }
                if (vMax == 0.0f) return false;
                
                // Interchange rows
                if (j != iMax) {
                    for (int k = 0; k < n; k++) {
                        float val = matrix[iMax, k];
                        matrix[iMax, k] = matrix[j, k];
                        matrix[j, k] = val;
                    }
                    rowScale[iMax] = rowScale[j];
                }
                
                // Divide by pivot
                rowOrder[j] = iMax;
                if (matrix[j, j] == 0.0f) return false;

                if (j != n - 1) {
                    float val = 1.0f / matrix[j, j];
                    for (int i = j + 1; i < n; i++) {
                        matrix[i, j] *= val;
                    }
                }
            }
            return true;
        }

        private void LUSolveInplace(float[] vector, in float[,] decomp, in int[] rowOrder) {
            int decompRows = decomp.GetLength(0);
            int decompColumns = decomp.GetLength(1);
            
            if (decompRows != decompColumns) throw new ArgumentException("decompRows != vector.Length");
            if (decompRows != rowOrder.Length) throw new ArgumentException("decompRows != rowOrder.Length");
            if (decompRows != vector.Length) throw new ArgumentException("decompRows != vector.Length");

            int n = decompRows;
            int ii = -1;
            
            // Forward Substitution
            for (int i = 0; i < n; i++) {
                float sum = vector[rowOrder[i]];
                vector[rowOrder[i]] = vector[i];

                if (ii != -1) {
                    for (int j = ii; j <= i - 1; j++) {
                        sum -= decomp[i, j] * vector[j];
                    }
                } else if (sum > 0.0f) {
                    ii = i;
                }

                vector[i] = sum;
            }
            
            // Backward Substituion
            for (int i = n - 1; i >= 0; i--) {
                float sum = vector[i];
                for (int j = i + 1; j < n; j++) {
                    sum -= decomp[i, j] * vector[j];
                }
                vector[i] = sum / decomp[i, i];
            }
        }

        private void MatrixXVector(float[] output, float[][] matrix, float[] vector) {
            int numRows = matrix.Length;
            int numColumns = matrix[0].Length;
            
            if (numColumns != vector.Length) throw new ArgumentException("numColumns != vector.Length");

            for (int row = 0; row < numRows; row++) {
                float sum = 0;
                for (int col = 0; col < numColumns; col++) {
                    sum += matrix[row][col] * vector[col];
                }
                output[row] = sum;
            }
        }

        private void ClampNormalizeWeights() {
            float total = 0.0f;
            int firstActive = -1;
            for (int i = 0; i < weights.Length; i++) {
                weights[i] = MathF.Max(0, weights[i]);
                if (firstActive == -1 && weights[i] > 0) firstActive = i;
                total += weights[i];
            }
            for (int i = 0; i < weights.Length; i++) {
                weights[i] /= total;
            }
            weights[firstActive] = 1;
        }
        
        // CALLBACKS
        private void OnParameterChanged() {
            RecalculateWeights();
        }
    }
}
