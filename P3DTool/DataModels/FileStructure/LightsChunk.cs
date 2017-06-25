using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using P3DTool.DataModels.DataTypes;
using P3DTool.Views;

namespace P3DTool.DataModels.FileStructure
{
    public class LightsChunk : P3DElement
    {
        public P3D Parent { get; set; }
        internal TreeViewItem TreeItem { get; set; }
        public byte[] ChunkIdTemplate { get; } = Encoding.ASCII.GetBytes("LIGHTS");
        public Int32 Size { get; set; }
        public short LightsNum { get; set; }

        public List<Light> Lights { get; set; } = new List<Light>();

        public LightsChunk(P3D parent)
        {
            Parent = parent;
            Application.Current.Dispatcher.BeginInvoke((Action)(() => addTreeItem()));
        }

        private void addTreeItem()
        {
            lock (this)
            {
                TreeItem = new TreeViewItem { Header = new P3DElementView(this, "Lights chunk") };
                Parent.TreeItem.Items.Add(TreeItem);
            }
        }

        public bool ReadChunk(BinaryReader reader)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < LightsNum; i++)
            {
                byte readen = reader.ReadByte();
                while (readen != 0)
                {

                    builder.Append(Convert.ToChar(readen));
                    readen = reader.ReadByte();
                }
                string name = builder.ToString();
                builder.Clear();

                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                float radius = reader.ReadSingle();
                int color = reader.ReadInt32();
                bool corona = reader.ReadBoolean();
                bool lensFlare = reader.ReadBoolean();
                bool lightEnvivornment = reader.ReadBoolean();

                Lights.Add(new Light(this, name, x, y, z, radius, color, corona, lensFlare, lightEnvivornment));
            }
            return true;
        }

        public string WriteChunk(BinaryWriter writer)
        {
            if (Size == 0)
            {
                return "Lights chunk size equals 0";
            }
            if (LightsNum > 256)
            {
                return "Lights count is bigger than 256";
            }

            writer.Write(ChunkIdTemplate);
            writer.Write(Size);
            writer.Write(LightsNum);

            foreach (Light light in Lights)
            {
                writer.Write(Encoding.ASCII.GetBytes(light.Name));
                writer.Write((byte)0);
                writer.Write(light.Position.x);
                writer.Write(light.Position.y);
                writer.Write(light.Position.z);
                writer.Write(light.Radius);
                writer.Write(light.Color);
                writer.Write(light.ShowCorona);
                writer.Write(light.ShowLensFlare);
                writer.Write(light.LightUpEnvivornment);
            }

            return String.Empty;
        }

        public void CalculateLightsPostionRelativeToOrigin(P3DVertex origin)
        {
            foreach (Light light in Lights)
            {
                light.CalculateLocalPos(origin);
            }
        }

        public long CalculateSizeFromLightsList()
        {
            int size = 2; // Lights number is short - 2
            foreach (Light light in Lights)
            {
                size += light.Name.Length + 1; // NULL terminated name
                size += 12; //Position
                size += 4; //Radius
                size += 4; //Color
                size += 3; //Flags
            }

            Size = size;
            return size;
        }

        public short CalculateLightsNum()
        {
            LightsNum = Convert.ToInt16(Lights.Count);
            return LightsNum;
        }

        public int ColorFromRGB(int r, int g, int b)
        {
            return (r << 16) + (g << 8) + b;
        }

        public void AddLightFromContextMenu(MainWindow ParentWindow)
        {
            //AddLightWindow window = new AddLightWindow();
            //window.Left = ParentWindow.Left + ParentWindow.ActualWidth;
            //window.Top = ParentWindow.Top;
            //window.Show();
            //ParentWindow.ActiveToolWindow = window;
        }

        public override ArrayList GetItemInfo()
        {
            ArrayList itemInfo = new ArrayList
            {
                new InputText(this,"Lights chunk size:",Size + " Bytes",false, ""),
                new InputText(this,"Lights count:",LightsNum.ToString(),false, ""),

            };

            return itemInfo;
        }

    }
}
