using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using P3DTool.DataModels.DataTypes;
using P3DTool.Views;

namespace P3DTool.DataModels.FileStructure
{
    public class MeshesChunk : P3DElement
    {
        internal P3D Parent { get; set; }
        internal TreeViewItem TreeItem { get; set; }

        public byte[] ChunkIdTemplate { get; } = Encoding.ASCII.GetBytes("MESHES");
        public Int32 Size { get; set; }
        public short MeshesNum { get; set; }

        public List<Mesh> Meshes { get; set; } = new List<Mesh>();

        public MeshesChunk(P3D parent)
        {
            Parent = parent;
            TreeItem = new TreeViewItem {Header = new P3DElementView(this, "Meshes chunk")};
            Parent.TreeItem.Items.Add(TreeItem);
        }

        public bool ReadChunk(BinaryReader reader)
        {
            for (int i = 0; i < MeshesNum; i++)
            {
                Mesh subMesh = new Mesh(this);
                bool success = subMesh.ParseMesh(reader);
                if (!success)
                {
                    return false;
                }
                Meshes.Add(subMesh);
                Parent.RaiseStatusUpdatedEvent(new StatusUpdatedEventArguments("Loading meshes from file", 40 + (40 / MeshesNum) * i));
            }
            return true;
        }

        public string WriteChunk(BinaryWriter writer)
        {
            if (Size == 0)
            {
                return "Meshes chunk size equals 0";
            }
            if (MeshesNum > 256)
            {
                return "P3D file format is limited to 32 submeshes.";
            }

            writer.Write(ChunkIdTemplate);
            writer.Write(Size);
            writer.Write(MeshesNum);

            foreach (Mesh mesh in Meshes)
            {
                string message = mesh.WriteChunk(writer);
                if (!message.Equals(String.Empty))
                {
                    return message;
                }
            }

            return String.Empty;
        }

        public override ArrayList GetItemInfo()
        {
            ArrayList itemInfo = new ArrayList
            {
                new InputText(this, "Meshes chunk size:", Size.ToString(), false, ""),
                new InputText(this, "Meshes count:", MeshesNum.ToString(), false, ""),
            };

            return itemInfo;
        }

    }
}