using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            Application.Current.Dispatcher.BeginInvoke((Action)(() => addTreeItem()));
        }

        private void addTreeItem()
        {
            lock (this)
            {
                TreeItem = new TreeViewItem { Header = new P3DElementView(this, "Meshes chunk") };
                Parent.TreeItem.Items.Add(TreeItem);
            }
        }

        public async Task<bool> ReadChunk(BinaryReader reader)
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
                Parent.RaiseStatusUpdatedEvent(new StatusUpdatedEventArguments("Loading meshes from file", (40 + (40 / MeshesNum) * i)));
                await Task.Delay(0).ConfigureAwait(false);
            }
            return true;
        }

        public void SeparateSubMeshesEdges()
        {
            foreach (Mesh mesh in Meshes)
            {
                mesh.SeparateHardEdges();
            }
        }

        public void SeparateUVVertices()
        {
            foreach (Mesh mesh in Meshes)
            {
                mesh.SeparateUVVertices();
            }
        }

        public void ClearUnusedVertices()
        {
            foreach (Mesh mesh in Meshes)
            {
                mesh.ClearUnusedVertices();
            }
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

        /// <summary>
        /// Checks if there's main,tracing,collision flag in any mesh, if not then it automatically assigns those to one
        /// </summary>
        public void CheckFlagsValidity()
        {
            bool foundMain = false;
            for (int i = 0; i < MeshesNum; i++)
            {
                if ((Meshes[i].Flags & (uint)MeshFlags.MAIN) == (uint)MeshFlags.MAIN)
                {
                    foundMain = true;
                    if (i != 0)
                    {
                        Utility.Swap(Meshes, 0, i);
                    }
                }
            }
            if (!foundMain)
            {
                Meshes[0].Flags += (uint) MeshFlags.MAIN;
            }

            bool foundTracing = false;
            for (int i = 0; i < MeshesNum; i++)
            {
                if ((Meshes[i].Flags & (uint)MeshFlags.TRACING_SHAPE) == (uint)MeshFlags.TRACING_SHAPE)
                {
                    foundTracing = true;
                }
            }
            if (!foundTracing)
            {
                Meshes[0].Flags += (uint) MeshFlags.TRACING_SHAPE;
            }

            bool foundColl = false;
            for (int i = 0; i < MeshesNum; i++)
            {
                if ((Meshes[i].Flags & (uint)MeshFlags.COLLISION_SHAPE) == (uint)MeshFlags.COLLISION_SHAPE)
                {
                    foundColl = true;
                }
            }
            if (!foundColl)
            {
                Meshes[0].Flags += (uint)MeshFlags.COLLISION_SHAPE;
            }
        }

        public void CalculateMeshChunkSize()
        {
            Size = 2;
            foreach (Mesh mesh in Meshes)
            {
                Size += mesh.GetMeshSize();
            }
        }

        public P3DVertex CalculateMeshesLocalPos()
        {
            P3DVertex origin = new P3DVertex(Meshes[0].LocalPos.x, Meshes[0].LocalPos.y, Meshes[0].LocalPos.z);
            foreach (Mesh mesh in Meshes)
            {
                mesh.CalculateLocalPos(origin);
            }
            return origin;
        }

        public void MoveMeshesToOrigin()
        {
            foreach (Mesh mesh in Meshes)
            {
                mesh.MoveVerticesToOrigin();
            }
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