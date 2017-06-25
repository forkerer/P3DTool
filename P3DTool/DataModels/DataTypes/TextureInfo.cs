using System.Collections;
using System.Drawing;
using System.Windows.Controls;
using P3DTool.Views;

namespace P3DTool.DataModels.DataTypes
{
    public class TextureInfo : P3DElement
    {
        public Mesh Parent { get; set; }
        internal TreeViewItem TreeItem { get; set; }

        public short TextureStart { get; set; }

        public short NumFlat { get; set; }
        public short NumFlatMetal { get; set; }
        public short NumGouraud { get; set; }
        public short NumGouraudMetal { get; set; }
        public short NumGouraudMetalEnv { get; set; }
        public short NumShining { get; set; }

        public TextureInfo(Mesh parent)
        {
            Parent = parent;
            TreeItem = new TreeViewItem
            {
                Header = new P3DElementView(this, "TextureInfo", new Bitmap(Properties.Resources.texture))
            };
            Parent.TreeItem.Items.Add(TreeItem);
        }

        public override ArrayList GetItemInfo()
        {
            ArrayList itemInfo = new ArrayList
            {
                new InputText(this, "TextureStart:", TextureStart.ToString(), false, ""),
                new InputText(this, "NumFlat:", NumFlat.ToString(), false, "NumFlat"),
                new InputText(this, "NumFlatMetal:", NumFlatMetal.ToString(), false, "NumFlatMetal"),
                new InputText(this, "NumGouraud:", NumGouraud.ToString(), false, "NumGouraud"),
                new InputText(this, "NumGouraudMetal:", NumGouraudMetal.ToString(), false, "NumGouraudMetal"),
                new InputText(this, "NumGouraudMetalEnv:", NumGouraudMetalEnv.ToString(), false, "NumGouraudMetalEnv"),
                new InputText(this, "NumShining:", NumShining.ToString(), false, "NumShining"),
            };

            return itemInfo;
        }
    }
}
