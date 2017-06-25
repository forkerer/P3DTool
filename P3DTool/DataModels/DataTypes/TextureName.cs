using System.Collections;
using System.Drawing;
using System.Windows.Controls;
using P3DTool.DataModels.FileStructure;
using P3DTool.Views;

namespace P3DTool.DataModels.DataTypes
{
    public class TextureName : P3DElement
    {
        public TextureChunk Parent { get; set; }
        internal TreeViewItem TreeItem { get; set; }

        public string Name { get; set; }

        public TextureName(TextureChunk parent, string name)
        {
            Parent = parent;
            Name = name;

            TreeItem = new TreeViewItem
            {
                Header = new P3DElementView(this, Name, new Bitmap(Properties.Resources.texture))
            };
            Parent.TreeItem.Items.Add(TreeItem);

        }

        public override ArrayList GetItemInfo()
        {
            ArrayList itemInfo = new ArrayList
            {
                new InputText(this, "Texture name:" ,Name, true, "name"),
            };

            return itemInfo;
        }
    }
}
