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
    public class TextureChunk : P3DElement
    {
        public P3D Parent { get; set; }
        internal TreeViewItem TreeItem { get; set; }
        public byte[] ChunkIdTemplate { get; } = Encoding.ASCII.GetBytes("TEX");
        public Int32 Size { get; set; }
        public byte TexNum { get; set; }

        public List<TextureName> TextureNames { get; } = new List<TextureName>();

        public TextureChunk(P3D parent)
        {
            Parent = parent;
            Application.Current.Dispatcher.BeginInvoke((Action) (() => addTreeItem()));
//            TreeItem = new TreeViewItem {Header = new P3DElementView(this, "Textures chunk")};
//            Parent.TreeItem.Items.Add(TreeItem);
        }

        private void addTreeItem()
        {
            lock (this)
            {
                TreeItem = new TreeViewItem { Header = new P3DElementView(this, "Textures chunk") };
                Parent.TreeItem.Items.Add(TreeItem);
            }
        }

        public bool ReadChunk(BinaryReader reader)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < Size - 1; i++) //size-1 because 1 byte is used to save number of textures
                {
                    byte readen = reader.ReadByte();
                    if (readen == 0)
                    {
                        TextureNames.Add(new TextureName(parent: this, name: builder.ToString().ToLower()));
                        builder.Clear();
                        continue;
                    }
                    builder.Append(Convert.ToChar(readen));
                }
                return true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                return false;
            }
            
        }

        public string WriteChunk(BinaryWriter writer)
        {
            if (Size == 0)
            {
                return "Size of chunk is equal to 0";
            }

            if (TexNum > 255)
            {
                return "Too many textures";
            }

            writer.Write(ChunkIdTemplate);
            writer.Write(Size);
            writer.Write(TexNum);
            foreach (TextureName texture in TextureNames)
            {
                writer.Write(Encoding.ASCII.GetBytes(texture.Name));
                writer.Write((byte)0);
            }
            return String.Empty;
        }

        public long CalculateSizeFromTexturesList()
        {
            int size = 1; //texture number is byte - 1
            foreach (TextureName tex in TextureNames)
            {
                size += tex.Name.Length + 1;
            }

            Size = size;
            return size;
        }

        public byte CalculateTexNum()
        {
            TexNum = Convert.ToByte(TextureNames.Count);
            return TexNum;
        }

        public bool IsTextureInList(string texture)
        {
            foreach (TextureName tex in TextureNames)
            {
                if (tex.Name.Equals(texture))
                {
                    return true;
                }
            }
            return false;
        }

        public override ArrayList GetItemInfo()
        {
            ArrayList itemInfo = new ArrayList
            {
                new InputText(this, "Textures chunk size:", Size + " Bytes", false, ""),
                new InputText(this, "Textures count:", TexNum.ToString(), false, ""),
            };

            return itemInfo;
        }
    }
}
