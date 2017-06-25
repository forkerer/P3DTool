using System;
using System.Collections;
using System.Drawing;
using System.Windows.Controls;
using P3DTool.DataModels.FileStructure;
using P3DTool.Views;

namespace P3DTool.DataModels.DataTypes
{
    public class Light : P3DElement
    {
        public LightsChunk Parent { get; set; }
        internal TreeViewItem TreeItem { get; set; }

        public string Name { get; set; }
        public P3DVertex Position { get; set; }
        public float Radius { get; set;  }
        public int Color { get; set; }
        public bool ShowCorona { get; set; }
        public bool ShowLensFlare { get; set; }
        public bool LightUpEnvivornment { get; set; }

        public Light(LightsChunk parent, string name, float x, float y, float z, float radius, Int32 color, bool showCorona, bool showLensFlare, bool lightUpEnvivornment)
        {
            Parent = parent;

            Name = name;
            Position = new P3DVertex(x, y, z);
            Radius = radius;
            Color = color;
            ShowCorona = showCorona;
            ShowLensFlare = showLensFlare;
            LightUpEnvivornment = lightUpEnvivornment;

            TreeItem = new TreeViewItem
            {
                Header = new P3DElementView(this, Name, new Bitmap(Properties.Resources.light))
            };
            Parent.TreeItem.Items.Add(TreeItem);
        }

        public int ColorFromRGB(int r, int g, int b)
        {
            return (r << 16) + (g << 8) + b;
        }

        public void SetColorRed(int col)
        {
            Color = ColorFromRGB(col, (Color >> 8) & 0x0ff, (Color) & 0x0ff);
        }

        public void SetColorGreen(int col)
        {
            Color = ColorFromRGB((Color >> 16) & 0x0ff, col, (Color) & 0x0ff);
        }

        public void SetColorBlue(int col)
        {
            Color = ColorFromRGB((Color >> 16) & 0x0ff, (Color >> 8) & 0x0ff, col);
        }

        public override ArrayList GetItemInfo()
        {
            ArrayList itemInfo = new ArrayList
            {
//                "Name: " + Name,
//                "X: " + Position.x,
//                "Y: " + Position.y,
//                "Z: " + Position.z,
//                "Radius: " + Radius,
//                "showCorona: " + ShowCorona,
//                "showLensFlare: " + ShowLensFlare,
//                "lightUpEnvivornment: " + LightUpEnvivornment,
                new InputText(this, "Name:" ,Name, true, "name"),

                new InputText(this,"X:",Position.x.ToString(),true, "posX"),
                new InputText(this,"Y:",Position.y.ToString(),true, "posY"),
                new InputText(this,"Z:",Position.z.ToString(),true, "posZ"),

                new InputText(this,"Radius:",Radius.ToString(),true, "radius"),

                new InputText(this,"R:",( (Color>>16) & 0x0ff).ToString(),true, "colR"),
                new InputText(this,"G:",( (Color>>8) & 0x0ff).ToString(),true, "colG"),
                new InputText(this,"B:",( (Color) & 0x0ff).ToString(),true, "colB"),

                new InputBool(this,"showCorona",ShowCorona,true, "corona"),
                new InputBool(this,"showLensFlare",ShowLensFlare, true, "lensFlare"),
                new InputBool(this,"lightUpEnvivornment",LightUpEnvivornment, true, "lightUpEnvivornment"),


            };

            return itemInfo;
        }


    }
}
