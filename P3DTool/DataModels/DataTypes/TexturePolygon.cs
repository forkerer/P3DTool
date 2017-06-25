using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using P3DTool.Views;

namespace P3DTool.DataModels.DataTypes
{
    public enum P3DMaterial
    {
        MAT_FLAT = 0,
        MAT_FLAT_METAL = 1,
        MAT_GORAUD = 2,
        MAT_GORAUD_METAL = 3,
        MAT_GORAUD_METAL_ENV = 4,
        MAT_SHINING = 5
    }

    public class TexturePolygon : P3DElement
    {
        public Mesh Parent { get; }
        internal TreeViewItem TreeItem { get; set; }

        public string Texture { get; set; }
        public P3DMaterial Material { get; set; }

        public short P1 { get; set; }
        public short P2 { get; set; }
        public short P3 { get; set; }

        public float U1 { get; set; }
        public float V1 { get; set; }
        public float U2 { get; set; }
        public float V2 { get; set; }
        public float U3 { get; set; }
        public float V3 { get; set; }

        public TexturePolygon() { }

        public TexturePolygon(Mesh parent)
        {
            Parent = parent;
            Application.Current.Dispatcher.BeginInvoke((Action)(() => addTreeItem()));
        }

        private void addTreeItem()
        {
            lock (this)
            {
                TreeItem = new TreeViewItem { Header = new P3DElementView(this, "TexturePolygon") };
                Parent.TreeItem.Items.Add(TreeItem);
            }
        }

        public override ArrayList GetItemInfo()
        {
            ArrayList itemInfo = new ArrayList
            {
                new InputText(this, "P1:", P1.ToString(), true, "p1"),
                new InputText(this, "U1:", U1.ToString(), true, "u1"),
                new InputText(this, "V1:", V1.ToString(), true, "v1"),

                new InputText(this, "P2:", P2.ToString(), true, "p2"),
                new InputText(this, "U2:", U2.ToString(), true, "u2"),
                new InputText(this, "V2:", V2.ToString(), true, "v2"),

                new InputText(this, "P3:", P3.ToString(), true, "p3"),
                new InputText(this, "U3:", U3.ToString(), true, "u3"),
                new InputText(this, "V3:", V3.ToString(), true, "v3"),

                new InputText(this, "Material:", Enum.GetName(typeof(P3DMaterial), Material), false, ""),

                
            };

            return itemInfo;
        }
    }
}
