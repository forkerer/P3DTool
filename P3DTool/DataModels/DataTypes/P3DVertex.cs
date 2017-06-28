using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using P3DTool.Views;

namespace P3DTool.DataModels.DataTypes
{
    public class P3DVertex : P3DElement
    {
        public float x, y, z;
        public short? NextToCheck;
        public P3DVertex(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public P3DVertex(Mesh parent, float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            Application.Current.Dispatcher.BeginInvoke((Action)(() => addTreeItem(parent)));
        }

        private void addTreeItem(Mesh parent)
        {
            lock (this)
            {
                TreeViewItem TreeItem = new TreeViewItem { Header = new P3DElementView(this, "TexturePolygon") };
                parent.TreeItem.Items.Add(TreeItem);
            }
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
