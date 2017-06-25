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
    }
}
