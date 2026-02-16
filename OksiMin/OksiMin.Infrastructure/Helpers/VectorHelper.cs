namespace OksiMin.Infrastructure.Helpers
{
    /// <summary>
    /// Helper class for vector operations (embeddings storage and similarity calculations)
    /// </summary>
    public static class VectorHelper
    {
        /// <summary>
        /// Convert float array (vector) to byte array for SQL Server storage
        /// </summary>
        /// <param name="vector">Float array representing the embedding vector</param>
        /// <returns>Byte array for varbinary storage</returns>
        public static byte[] FloatArrayToBytes(float[] vector)
        {
            if (vector == null || vector.Length == 0)
                throw new ArgumentException("Vector cannot be null or empty", nameof(vector));

            byte[] bytes = new byte[vector.Length * sizeof(float)];
            Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// Convert byte array from SQL Server back to float array
        /// </summary>
        /// <param name="bytes">Byte array from varbinary column</param>
        /// <returns>Float array representing the vector</returns>
        public static float[] BytesToFloatArray(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return Array.Empty<float>();

            if (bytes.Length % sizeof(float) != 0)
                throw new ArgumentException("Invalid byte array length for float conversion", nameof(bytes));

            float[] vector = new float[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, vector, 0, bytes.Length);
            return vector;
        }

        /// <summary>
        /// Calculate cosine similarity between two vectors
        /// </summary>
        /// <param name="vector1">First vector</param>
        /// <param name="vector2">Second vector</param>
        /// <returns>Similarity score between -1 and 1 (1 = identical, 0 = orthogonal, -1 = opposite)</returns>
        public static double CosineSimilarity(float[] vector1, float[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must have the same dimension");

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += vector1[i] * vector1[i];
                magnitude2 += vector2[i] * vector2[i];
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// Generate a random vector for testing purposes
        /// </summary>
        /// <param name="dimensions">Number of dimensions (e.g., 768 for nomic-embed-text)</param>
        /// <param name="seed">Random seed for reproducibility</param>
        /// <returns>Random float array</returns>
        public static float[] GenerateRandomVector(int dimensions, int? seed = null)
        {
            var random = seed.HasValue ? new Random(seed.Value) : new Random();
            var vector = new float[dimensions];

            for (int i = 0; i < dimensions; i++)
            {
                // Generate random floats between -1 and 1
                vector[i] = (float)(random.NextDouble() * 2 - 1);
            }

            return vector;
        }

        /// <summary>
        /// Normalize a vector to unit length
        /// </summary>
        public static float[] Normalize(float[] vector)
        {
            double magnitude = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                magnitude += vector[i] * vector[i];
            }
            magnitude = Math.Sqrt(magnitude);

            if (magnitude == 0)
                return vector;

            var normalized = new float[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                normalized[i] = (float)(vector[i] / magnitude);
            }

            return normalized;
        }
    }
}