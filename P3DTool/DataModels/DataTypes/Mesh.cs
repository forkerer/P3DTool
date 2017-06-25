using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using lib3ds.Net;
using P3DTool.DataModels.FileStructure;
using P3DTool.Views;

namespace P3DTool.DataModels.DataTypes
{
    public enum MeshFlags
    {
        MAIN = 1,          // Mesh is main mesh
        VISIBLE = 2,         // Mesh is visible
        TRACING_SHAPE = 4,         // Objekt ist Tracing-Silhouette
        COLLISION_SHAPE = 8,         // Objekt ist Kollisionsobjekt
        DETACHABLE_PART = 16,         // Objekt ist abfallendes Teil bei einem Auto
        BREAKABLE_GLASS = 32,         // Objekt ist zerbrechliches Glass
        BREAKABLE_PLASTIC = 64,         // Objekt ist zerbrechliche Plastik
        BREAKABLE_WOOD = 128,         // Objekt ist zerbrechliches Holzteil
        BREAKABLE_METAL = 256,         // Objekt ist zerbrechliches Metallteil
        NUMBER_PLATE = 512,         // Objekt ist ein Nummernschild
        HEADLIGHT = 1024,         // Objekt ist ein Scheinwerfer
        BRAKELIGHT = 2048          // Objekt ist ein Bremslicht
    }
    public class Mesh : P3DElement
    {
        public byte[] ChunkIdTemplate { get; } = Encoding.ASCII.GetBytes("SUBMESH");
        internal MeshesChunk Parent { get; set; }
        internal TreeViewItem TreeItem { get; set; }

        public string Name { get; set; }
        public Int32 Size { get; set; }
        public uint Flags { get; set; }
        public short NumVertices { get; set; }
        public short NumPolys { get; set; }

        public List<TextureInfo> Info { get; set; } = new List<TextureInfo>();

        public List<P3DVertex> Vertices { get; set; } = new List<P3DVertex>();
        public List<TexturePolygon> Polygons { get; set; } = new List<TexturePolygon>();

        public P3DVertex LocalPos { get; set; }
        public float Length { get; set; }
        public float Height { get; set; }
        public float Depth { get; set; }

        public Mesh(MeshesChunk parent)
        {
            Parent = parent;
            TreeItem = new TreeViewItem
            {
                Header = new P3DElementView(this, Name, new Bitmap(Properties.Resources.mesh))
            };
            Parent.TreeItem.Items.Add(TreeItem);
        }
        public bool ParseMesh(BinaryReader reader)
        {
            byte[] submeshHeader = reader.ReadBytes(7);
            if (!Utility.ByteArrayCompare(submeshHeader, ChunkIdTemplate))
            {
                MessageBox.Show("Expected SUBMESH chunk header, got " + Encoding.ASCII.GetString(submeshHeader));
                return false;
            }
            Size = reader.ReadInt32();

            StringBuilder builder = new StringBuilder();
            byte readen = reader.ReadByte();
            while (readen != 0)
            {
                builder.Append(Convert.ToChar(readen));
                readen = reader.ReadByte();
            }
            Name = builder.ToString();
            ((P3DElementView)TreeItem.Header).content.Text = Name;
            builder.Clear();

            Flags = reader.ReadUInt32();

            LocalPos = new P3DVertex(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Length = reader.ReadSingle();
            Height = reader.ReadSingle();
            Depth = reader.ReadSingle();

            for (int i = 0; i < Parent.Parent.TextureChunk.TexNum; i++)
            {
                Info.Insert(i, new TextureInfo(this));
                Info[i].TextureStart = reader.ReadInt16();
                Info[i].NumFlat = reader.ReadInt16();
                Info[i].NumFlatMetal = reader.ReadInt16();
                Info[i].NumGouraud = reader.ReadInt16();
                Info[i].NumGouraudMetal = reader.ReadInt16();
                Info[i].NumGouraudMetalEnv = reader.ReadInt16();
                Info[i].NumShining = reader.ReadInt16();
            }

            NumVertices = reader.ReadInt16();
            if (NumVertices < 1)
            {
                MessageBox.Show("Mesh doesn't have vertices");
                return false;
            }

            for (int i = 0; i < NumVertices; i++)
            {
                Vertices.Insert(i, new P3DVertex(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
            }

            NumPolys = reader.ReadInt16();
            if (NumPolys < 1)
            {
                MessageBox.Show("Mesh doesn't have polygons");
                return false;
            }

            for (int i = 0; i < NumPolys; i++)
            {
                Polygons.Insert(i, new TexturePolygon());

                Polygons[i].P1 = reader.ReadInt16();
                Polygons[i].U1 = reader.ReadSingle();
                Polygons[i].V1 = reader.ReadSingle();

                Polygons[i].P2 = reader.ReadInt16();
                Polygons[i].U2 = reader.ReadSingle();
                Polygons[i].V2 = reader.ReadSingle();

                Polygons[i].P3 = reader.ReadInt16();
                Polygons[i].U3 = reader.ReadSingle();
                Polygons[i].V3 = reader.ReadSingle();
            }

            for (int i = 0; i < Parent.Parent.TextureChunk.TexNum; i++)
            {
                short polyInTex = Info[i].TextureStart;

                for (int j = 0; j < Info[i].NumFlat; j++)
                {
                    Polygons[polyInTex + j].Material = P3DMaterial.MAT_FLAT;
                    Polygons[polyInTex + j].Texture = GetTextureList()[i].Name;
                }
                polyInTex += Info[i].NumFlat;

                for (int j = 0; j < Info[i].NumFlatMetal; j++)
                {
                    Polygons[polyInTex + j].Material = P3DMaterial.MAT_FLAT_METAL;
                    Polygons[polyInTex + j].Texture = GetTextureList()[i].Name;
                }
                polyInTex += Info[i].NumFlatMetal;

                for (int j = 0; j < Info[i].NumGouraud; j++)
                {
                    Polygons[polyInTex + j].Material = P3DMaterial.MAT_GORAUD;
                    Polygons[polyInTex + j].Texture = GetTextureList()[i].Name;
                }
                polyInTex += Info[i].NumGouraud;

                for (int j = 0; j < Info[i].NumGouraudMetal; j++)
                {
                    Polygons[polyInTex + j].Material = P3DMaterial.MAT_GORAUD_METAL;
                    Polygons[polyInTex + j].Texture = GetTextureList()[i].Name;
                }
                polyInTex += Info[i].NumGouraudMetal;

                for (int j = 0; j < Info[i].NumGouraudMetalEnv; j++)
                {
                    Polygons[polyInTex + j].Material = P3DMaterial.MAT_GORAUD_METAL_ENV;
                    Polygons[polyInTex + j].Texture = GetTextureList()[i].Name;
                }
                polyInTex += Info[i].NumGouraudMetalEnv;

                for (int j = 0; j < Info[i].NumShining; j++)
                {
                    Polygons[polyInTex + j].Material = P3DMaterial.MAT_SHINING;
                    Polygons[polyInTex + j].Texture = GetTextureList()[i].Name;
                }
            }

            return true;
        }

        public bool Parse3DSMesh(Lib3dsFile file, Lib3dsMesh mesh)
        {
            foreach (Lib3dsVertex vert in mesh.vertices)
            {
                Vertices.Add(new P3DVertex(vert.x,vert.z,vert.y));
            }

            foreach (Lib3dsFace face in mesh.faces)
            {
                TexturePolygon texPoly = new TexturePolygon();
                texPoly.P1 = Convert.ToInt16(face.index[0]);
                texPoly.P2 = Convert.ToInt16(face.index[2]);
                texPoly.P3 = Convert.ToInt16(face.index[1]);
                texPoly.U1 = mesh.texcos[face.index[0]].s;
                texPoly.U2 = mesh.texcos[face.index[2]].s;
                texPoly.U3 = mesh.texcos[face.index[1]].s;
                texPoly.V1 = mesh.texcos[face.index[0]].t;
                texPoly.V2 = mesh.texcos[face.index[2]].t;
                texPoly.V3 = mesh.texcos[face.index[1]].t;
                texPoly.Texture = GetTextureList()[face.material-1].Name;
                texPoly.Material = MaterialFrom3DS(file.materials[face.material], (face.smoothing_group != 0));
                Polygons.Add(texPoly);
            }

            Name = mesh.name;
            ((P3DElementView)TreeItem.Header).content.Text = Name;
            Size = 1;
            Flags = 1;
            NumVertices = Convert.ToInt16(mesh.nvertices);
            NumPolys = Convert.ToInt16(mesh.nfaces);
            LocalPos = new P3DVertex(mesh.matrix[3, 0], mesh.matrix[3, 1], mesh.matrix[3, 2]);
            Length = 0;
            Height = 0;
            Depth = 0;
            return true;
        }

        public string WriteChunk(BinaryWriter writer)
        {
            if (Size == 0)
            {
                return "Mesh size equals 0";
            }
            if (NumVertices == 0)
            {
                return "Vertices number in mesh equals 0";
            }
            if (NumPolys == 0)
            {
                return "Polygons number in mesh equals 0";
            }
            if (Info.Count == 0)
            {
                return "No texture info in mesh";
            }

            writer.Write(ChunkIdTemplate);
            writer.Write(Size);
            writer.Write(Encoding.ASCII.GetBytes(Name));
            writer.Write((byte)0);
            writer.Write(Flags);
            writer.Write(LocalPos.x);
            writer.Write(LocalPos.y);
            writer.Write(LocalPos.z);
            writer.Write(Length);
            writer.Write(Height);
            writer.Write(Depth);

            foreach (TextureInfo textureInfo in Info)
            {
                writer.Write(textureInfo.TextureStart);
                writer.Write(textureInfo.NumFlat);
                writer.Write(textureInfo.NumFlatMetal);
                writer.Write(textureInfo.NumGouraud);
                writer.Write(textureInfo.NumGouraudMetal);
                writer.Write(textureInfo.NumGouraudMetalEnv);
                writer.Write(textureInfo.NumShining);
            }

            writer.Write(NumVertices);
            foreach (P3DVertex vert in Vertices)
            {
                writer.Write(vert.x);
                writer.Write(vert.y);
                writer.Write(vert.z);
            }

            writer.Write(NumPolys);
            foreach (TexturePolygon poly in Polygons)
            {
                writer.Write(poly.P1);
                writer.Write(poly.U1);
                writer.Write(poly.V1);

                writer.Write(poly.P2);
                writer.Write(poly.U2);
                writer.Write(poly.V2);

                writer.Write(poly.P3);
                writer.Write(poly.U3);
                writer.Write(poly.V3);
            }

            return String.Empty;
        }

        private List<TextureName> GetTextureList()
        {
            return Parent.Parent.TextureChunk.TextureNames;
        }

        public void SortPolygonsAndGenerateTextureInfo()
        {
            List<TexturePolygon> newPolygons = new List<TexturePolygon>();
            short totalCount = 0;
            foreach (TextureName tex in GetTextureList())
            {
                TextureInfo info = new TextureInfo(this);
                info.TextureStart = totalCount;
                List<TexturePolygon> tempList = Polygons.Where(pol => (pol.Texture == tex.Name)).ToList();
                TexturePolygon_SortByMaterial sorter = new TexturePolygon_SortByMaterial();
                tempList.Sort(sorter);
                foreach (TexturePolygon poly in tempList)
                {
                    if (poly.Material == P3DMaterial.MAT_FLAT)
                    {
                        info.NumFlat++;
                    }
                    else if (poly.Material == P3DMaterial.MAT_FLAT_METAL)
                    {
                        info.NumFlatMetal++;
                    }
                    else if (poly.Material == P3DMaterial.MAT_GORAUD)
                    {
                        info.NumGouraud++;
                    }
                    else if (poly.Material == P3DMaterial.MAT_GORAUD_METAL)
                    {
                        info.NumGouraudMetal++;
                    }
                    else if (poly.Material == P3DMaterial.MAT_GORAUD_METAL_ENV)
                    {
                        info.NumGouraudMetalEnv++;
                    }
                    else if (poly.Material == P3DMaterial.MAT_SHINING)
                    {
                        info.NumShining++;
                    }
                    totalCount++;
                }
                newPolygons.AddRange(tempList);
                Info.Add(info);
            }
        }

        private P3DMaterial MaterialFrom3DS(Lib3dsMaterial material, bool hasSmoothing)
        {
            if (!hasSmoothing)
            {
                return P3DMaterial.MAT_FLAT;
            }
            if ((Math.Abs(material.self_illum) > 0.0001) ||
                (material.ambient[0] == 1.0 && material.ambient[1] == 1.0 && material.ambient[2] == 1.0))
            {
                return P3DMaterial.MAT_SHINING;
            }
            if (material.reflection_map.name != String.Empty)
            {
                return P3DMaterial.MAT_GORAUD_METAL_ENV;
            }
            return P3DMaterial.MAT_GORAUD;
        }

        public override ArrayList GetItemInfo()
        {
//            ArrayList itemInfo = new ArrayList { "Mesh flags: " };
//            GetFlagsInfo(itemInfo);
//            itemInfo.Add("Vertices number: " + NumVertices);
//            itemInfo.Add("Polys number: " + NumPolys);
//            itemInfo.Add("Length: " + Length);
//            itemInfo.Add("Height: " + Height);
//            itemInfo.Add("Depth: " + Depth);
//            itemInfo.Add("Local pos:\n  X: " + LocalPos.x + "\n  Y: " + LocalPos.y + "\n  Z: " + LocalPos.z);
//            itemInfo.Add("TextureInfo count: " + Info.Count);
            ArrayList itemInfo = new ArrayList
            {
                new InputText(this, "Name:" ,Name, true, "name"),
                new InputText(this, "Vertices count:" ,NumVertices.ToString(), false, ""),
                new InputText(this, "Polygons count:" ,NumPolys.ToString(), false, ""),
                new InputText(this,"Length:",Length.ToString(),false, ""),
                new InputText(this,"Height:",Height.ToString(),false, ""),
                new InputText(this,"Depth:",Depth.ToString(),false, ""),
                new InputText(this,"X:",LocalPos.x.ToString(),true, "posX"),
                new InputText(this,"Y:",LocalPos.y.ToString(),true, "posY"),
                new InputText(this,"Z:",LocalPos.z.ToString(),true, "posZ")
            };
            GetFlagsInfo(itemInfo);
            return itemInfo;
        }

        private void GetFlagsInfo(ArrayList itemInfo)
        {
            foreach (int value in Enum.GetValues(typeof(MeshFlags)))
            {
                bool isSet = ((Flags & value) == value);
                string flag = Enum.GetName(typeof(MeshFlags), value);
                itemInfo.Add(new InputBool(this, flag, isSet, true, flag));
            }
        }

        
    }
}
