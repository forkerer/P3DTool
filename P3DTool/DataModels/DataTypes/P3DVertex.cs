using System.Collections;
using P3DTool.Views;

namespace P3DTool.DataModels.DataTypes
{
    public class P3DVertex : P3DElement
    {
        public float x, y, z;
        public P3DVertex(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override ArrayList GetItemInfo()
        {
            ArrayList itemInfo = new ArrayList
            {
                new InputText(this,"X:",x.ToString(),true, "posX"),
                new InputText(this,"Y:",y.ToString(),true, "posY"),
                new InputText(this,"Z:",z.ToString(),true, "posZ")
            };

            return itemInfo;
        }
    }
}
