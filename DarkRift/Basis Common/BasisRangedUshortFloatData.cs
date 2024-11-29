﻿using System;
namespace Basis.Scripts.Networking.Compression
{
    [Serializable]
    public class BasisRangedUshortFloatData
    {
        public  float Precision;
        public  float InversePrecision;
        public  float MinValue;
        public  float MaxValue;
        public  int RequiredBits;
        public  ushort Mask;
        public BasisRangedUshortFloatData(float minValue, float maxValue, float precision)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            Precision = precision;
            InversePrecision = 1.0f / precision;
            RequiredBits = CalculateRequiredBits();
            Mask = (ushort)((1 << RequiredBits) - 1);
        }
        public ushort Compress(float value)
        {
            value = Clamp(value, MinValue, MaxValue);
            float normalizedValue = (value - MinValue) * InversePrecision;
            return (ushort)((ushort)(normalizedValue + 0.5f) & Mask);
        }
        public float Decompress(ushort compressedValue)
        {
            float decompressedValue = ((float)compressedValue * Precision) + MinValue;
            return Clamp(decompressedValue, MinValue, MaxValue);
        }
        private int CalculateRequiredBits()
        {
            float range = MaxValue - MinValue;
            float maxValueInRange = range * InversePrecision;
            return FastLog2((uint)(maxValueInRange + 0.5f)) + 1;
        }
        public static int FastLog2(uint value)
        {
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return deBruijnLookup[(value * 0x07C4ACDDU) >> 27];
        }

        private static readonly int[] deBruijnLookup = new int[32]
        {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
        };
        /// <summary>
        /// Clamps a value between a minimum and a maximum.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            return value;
        }
    }
}
