using System;
using System.Collections.Generic;

namespace P3DTool.DataModels.DataTypes
{
    public class Extent
    {
        public float XMax;
        public float XMin;
        public float YMax;
        public float YMin;
        public float ZMax;
        public float ZMin;
        public float XSize;
        public float YSize;
        public float ZSize;
        public bool IsImportedSize = false;

        public float GetXSize()
        {
            if (IsImportedSize)
            {
                return XSize;
            }
            return XMax - XMin;
        }

        public float GetYSize()
        {
            if (IsImportedSize)
            {
                return YSize;
            }
            return YMax - YMin;
        }

        public float GetZSize()
        {
            if (IsImportedSize)
            {
                return ZSize;
            }
            return ZMax - ZMin;
        }

        public void CalculateExtentFromMeshes(List<Mesh> meshes)
        {
            foreach (Mesh mesh in meshes)
            {
                XMin = Math.Min(XMin, mesh.LocalPos.x - (mesh.Length / 2));
                XMax = Math.Max(XMax, mesh.LocalPos.x + (mesh.Length / 2));

                YMin = Math.Min(YMin, mesh.LocalPos.y - (mesh.Height / 2));
                YMax = Math.Max(YMax, mesh.LocalPos.y + (mesh.Height / 2));

                ZMin = Math.Min(ZMin, mesh.LocalPos.z - (mesh.Depth / 2));
                ZMax = Math.Max(ZMax, mesh.LocalPos.z + (mesh.Depth / 2));
            }
        }
    }
}
