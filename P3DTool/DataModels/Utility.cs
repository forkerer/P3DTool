using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using lib3ds.Net;
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

    public class _3dsMaterials
    {
        private static _3dsMaterials instance;
        Dictionary<Tuple<string, P3DMaterial>, int> Materials = new Dictionary< Tuple<string, P3DMaterial>, int>();
        private int counter = 0;

        public int GetMaterial(string name, P3DMaterial type, Lib3dsFile file)
        {
            if (!Materials.ContainsKey(new Tuple<string, P3DMaterial>(name,type)))
            {
                Materials.Add(new Tuple<string, P3DMaterial>(name, type), GetMaterialForNameAndType(name,type, file));
                
                counter++;
            }
            return Materials[new Tuple<string, P3DMaterial>(name, type)];
        }

        private int GetMaterialForNameAndType(string name, P3DMaterial type, Lib3dsFile file)
        {
            Lib3dsMaterial mat = LIB3DS.lib3ds_material_new(name);
            mat.texture1_map = new Lib3dsTextureMap();
            mat.texture1_map.name = name;
            mat.diffuse[0] = 0.9f;
            mat.diffuse[1] = 0.9f;
            mat.diffuse[2] = 0.9f;
            //            if (type == P3DMaterial.MAT_FLAT)
            //            {
            //                mat.diffuse[0] = 0.9f;
            //                mat.diffuse[1] = 0.9f;
            //                mat.diffuse[2] = 0.9f;
            //            }
            //            else if (type == P3DMaterial.MAT_GORAUD)
            //            {
            //                mat.diffuse[0] = 0.9f;
            //                mat.diffuse[1] = 0f;
            //                mat.diffuse[2] = 0f;
            //            }
            //            else if (type == P3DMaterial.MAT_GORAUD_METAL_ENV)
            //            {
            //                mat.diffuse[0] = 0f;
            //                mat.diffuse[1] = 0f;
            //                mat.diffuse[2] = 0.9f;
            //            }
            //            else
            //            {
            //                mat.diffuse[0] = 0f;
            //                mat.diffuse[1] = 0.9f;
            //                mat.diffuse[2] = 0f;
            //            }
            LIB3DS.lib3ds_file_insert_material(file, mat, -1);
            return counter;
        }

        private _3dsMaterials() { }

        public static _3dsMaterials Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new _3dsMaterials();
                }
                return instance;
            }
        }

    }

    public class Edge
    {
        public short v1;
        public short v2;

        public Edge(short v1, short v2)
        {
            v1 = Math.Min(v1, v2);
            v2 = Math.Max(v2, v2);
        }
    }

}
