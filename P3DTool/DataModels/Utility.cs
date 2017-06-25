using System;
using System.Collections;
using System.Collections.Generic;
using P3DTool.DataModels.DataTypes;

namespace P3DTool.DataModels
{
    public static class Utility
    {
        public static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(a1, a2);
        }

        public static bool BitTest(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }

    class TexturePolygon_SortByMaterial : IComparer<TexturePolygon>
    {
        #region IComparer<TexturePolygon> Members

        public int Compare(TexturePolygon x, TexturePolygon y)
        {
            if (x.Material > y.Material) return 1;
            else if (x.Material < y.Material) return -1;
            else return 0;
        }

        #endregion
    }

    public class StatusUpdatedEventArguments : EventArgs
    {
        public string Message { get; }
        public int Value { get; }

        public StatusUpdatedEventArguments(string message, int value)
        {
            Message = message;
            Value = value;
        }

    }
    
}
