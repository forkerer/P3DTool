using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
            Application.Current.Dispatcher.BeginInvoke((Action)(() => addTreeItem()));
        }

        private void addTreeItem()
        {
            lock (this)
            {
                TreeItem = new TreeViewItem
                {
                    Header = new P3DElementView(this, Name, new Bitmap(Properties.Resources.mesh))
                };
                Parent.TreeItem.Items.Add(TreeItem);
            } 
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
            Application.Current.Dispatcher.BeginInvoke((Action)(() => ((P3DElementView)TreeItem.Header).content.Text = Name));
           // ((P3DElementView)TreeItem.Header).content.Text = Name;
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
                Vertices.Add(new P3DVertex(this,vert.x,vert.z,vert.y));
            }

            foreach (Lib3dsFace face in mesh.faces)
            {
                if (face.index.Length > 3)
                {
                    //MessageBox.Show("SHIT.");
                }
                TexturePolygon texPoly = new TexturePolygon();

                texPoly.Texture = GetTextureFrom3dsID(file, face.material).Name;
                texPoly.Material = MaterialFrom3DS(file.materials[face.material], (face.smoothing_group != 0));
                texPoly.SGFrom3DS = face.smoothing_group;
                texPoly.U1 = mesh.texcos[face.index[0]].s;
                texPoly.U2 = mesh.texcos[face.index[2]].s;
                texPoly.U3 = mesh.texcos[face.index[1]].s;
                texPoly.V1 = mesh.texcos[face.index[0]].t;
                texPoly.V2 = mesh.texcos[face.index[2]].t;
                texPoly.V3 = mesh.texcos[face.index[1]].t;

                texPoly.P1 = Convert.ToInt16(face.index[0]);
                texPoly.P2 = Convert.ToInt16(face.index[2]);
                texPoly.P3 = Convert.ToInt16(face.index[1]);

                
                
                Polygons.Add(texPoly);
            }

            Name = mesh.name;
            Application.Current.Dispatcher.BeginInvoke((Action)(() => ((P3DElementView)TreeItem.Header).content.Text = Name));
            Size = 1;
            Flags = FlagFrom3DSName(mesh.name);
            NumVertices = Convert.ToInt16(Vertices.Count);
            NumPolys = Convert.ToInt16(Polygons.Count);
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

        private TextureName GetTextureFrom3dsID(Lib3dsFile file, int id)
        {
            foreach (TextureName tex in GetTextureList())
            {
                if (tex.Name == file.materials[id].texture1_map.name.ToLower())
                {
                    return tex;
                }
            }
            return GetTextureList()[0];
        }

        /// <summary>
        /// This one needs a bit of an explaination, it works this way:
        /// It goes over every texture info in mesh, for each texture and material type it looks for all edges in element.
        /// If the edge is found only once, it means it's border edge, and it should be replaced with hard edge
        /// Overall the algoritm separates all textures and materials with hard edges, thus making it work nicely in 3ds max
        /// Still leaves some vertices that have different uv's in different polygons, so that has to be fixed later on
        /// </summary>
        public void SeparateHardEdges()
        {
            Dictionary<Tuple<short,short>, int> Edges = new Dictionary<Tuple<short, short>, int>();
            Dictionary<short, short> VerticesToChange = new Dictionary<short, short>();
            short polyCounter = 0;
            for (int index = 0; index < Info.Count; index++)
            {
                TextureInfo texInfo = Info[index];
                //MessageBox.Show(texInfo.TextureStart.ToString());
                short curCounter = polyCounter;
                for (int i = 0; i < texInfo.NumFlat; i++)
                {
                    short P1 = Polygons[polyCounter].P1;
                    short P2 = Polygons[polyCounter].P2;
                    short P3 = Polygons[polyCounter].P3;

                    Tuple<short, short> Edge1 = new Tuple<short, short>(P1, P2);
                    Tuple<short, short> Edge2 = new Tuple<short, short>(P2, P3);
                    Tuple<short, short> Edge3 = new Tuple<short, short>(P3, P1);

                    if (Edges.ContainsKey(Edge1))
                    {
                        Edges[Edge1]++;
                    }
                    else
                    {
                        Edges[Edge1] = 1;
                    }

                    if (Edges.ContainsKey(Edge2))
                    {
                        Edges[Edge2]++;
                    }
                    else
                    {
                        Edges[Edge2] = 1;
                    }

                    if (Edges.ContainsKey(Edge3))
                    {
                        Edges[Edge3]++;
                    }
                    else
                    {
                        Edges[Edge3] = 1;
                    }
                    polyCounter++;
                }

                foreach (KeyValuePair<Tuple<short, short>, int> dic in Edges)
                {
                    if (dic.Value == 1)
                    {
                        if (!VerticesToChange.ContainsKey(dic.Key.Item1))
                        {
                            Vertices.Add(new P3DVertex(this, Vertices[dic.Key.Item1].x, Vertices[dic.Key.Item1].y,
                                Vertices[dic.Key.Item1].z));
                            VerticesToChange[dic.Key.Item1] = (short) (Vertices.Count - 1);
                            NumVertices++;
                        }
                        if (!VerticesToChange.ContainsKey(dic.Key.Item2))
                        {
                            Vertices.Add(new P3DVertex(this, Vertices[dic.Key.Item2].x, Vertices[dic.Key.Item2].y,
                                Vertices[dic.Key.Item2].z));
                            VerticesToChange[dic.Key.Item2] = (short)(Vertices.Count - 1);
                            NumVertices++;
                        }
                    }
                }

                for (int ind = curCounter; ind < polyCounter; ind++)
                {
                    if (VerticesToChange.ContainsKey(Polygons[ind].P1))
                    {
                        Polygons[ind].P1 = VerticesToChange[Polygons[ind].P1];
                    }
                    if (VerticesToChange.ContainsKey(Polygons[ind].P2))
                    {
                        Polygons[ind].P2 = VerticesToChange[Polygons[ind].P2];
                    }
                    if (VerticesToChange.ContainsKey(Polygons[ind].P3))
                    {
                        Polygons[ind].P3 = VerticesToChange[Polygons[ind].P3];
                    }
                }

                Edges.Clear();
                VerticesToChange.Clear();

                curCounter = polyCounter;
                for (int i = 0; i < texInfo.NumFlatMetal; i++)
                {
                    short P1 = Polygons[polyCounter].P1;
                    short P2 = Polygons[polyCounter].P2;
                    short P3 = Polygons[polyCounter].P3;

                    Tuple<short, short> Edge1 = new Tuple<short, short>(P1, P2);
                    Tuple<short, short> Edge2 = new Tuple<short, short>(P2, P3);
                    Tuple<short, short> Edge3 = new Tuple<short, short>(P3, P1);

                    if (Edges.ContainsKey(Edge1))
                    {
                        Edges[Edge1]++;
                    }
                    else
                    {
                        Edges[Edge1] = 1;
                    }

                    if (Edges.ContainsKey(Edge2))
                    {
                        Edges[Edge2]++;
                    }
                    else
                    {
                        Edges[Edge2] = 1;
                    }

                    if (Edges.ContainsKey(Edge3))
                    {
                        Edges[Edge3]++;
                    }
                    else
                    {
                        Edges[Edge3] = 1;
                    }
                    polyCounter++;
                }

                foreach (KeyValuePair<Tuple<short, short>, int> dic in Edges)
                {
                    if (dic.Value == 1)
                    {
                        if (!VerticesToChange.ContainsKey(dic.Key.Item1))
                        {
                            Vertices.Add(new P3DVertex(this, Vertices[dic.Key.Item1].x, Vertices[dic.Key.Item1].y,
                                Vertices[dic.Key.Item1].z));
                            VerticesToChange[dic.Key.Item1] = (short) (Vertices.Count - 1);
                            NumVertices++;
                        }
                        if (!VerticesToChange.ContainsKey(dic.Key.Item2))
                        {
                            Vertices.Add(new P3DVertex(this, Vertices[dic.Key.Item2].x, Vertices[dic.Key.Item2].y,
                                Vertices[dic.Key.Item2].z));
                            VerticesToChange[dic.Key.Item2] = (short)(Vertices.Count - 1);
                            NumVertices++;
                        }
                    }
                }

                for (int ind = curCounter; ind < polyCounter; ind++)
                {
                    if (VerticesToChange.ContainsKey(Polygons[ind].P1))
                    {
                        Polygons[ind].P1 = VerticesToChange[Polygons[ind].P1];
                    }
                    if (VerticesToChange.ContainsKey(Polygons[ind].P2))
                    {
                        Polygons[ind].P2 = VerticesToChange[Polygons[ind].P2];
                    }
                    if (VerticesToChange.ContainsKey(Polygons[ind].P3))
                    {
                        Polygons[ind].P3 = VerticesToChange[Polygons[ind].P3];
                    }
                }

                Edges.Clear();
                VerticesToChange.Clear();

                curCounter = polyCounter;
                for (int i = 0; i < texInfo.NumGouraud; i++)
                {
                    short P1 = Polygons[polyCounter].P1;
                    short P2 = Polygons[polyCounter].P2;
                    short P3 = Polygons[polyCounter].P3;

                    Tuple<short, short> Edge1 = new Tuple<short, short>(P1, P2);
                    Tuple<short, short> Edge2 = new Tuple<short, short>(P2, P3);
                    Tuple<short, short> Edge3 = new Tuple<short, short>(P3, P1);

                    if (Edges.ContainsKey(Edge1))
                    {
                        Edges[Edge1]++;
                    }
                    else
                    {
                        Edges[Edge1] = 1;
                    }

                    if (Edges.ContainsKey(Edge2))
                    {
                        Edges[Edge2]++;
                    }
                    else
                    {
                        Edges[Edge2] = 1;
                    }

                    if (Edges.ContainsKey(Edge3))
                    {
                        Edges[Edge3]++;
                    }
                    else
                    {
                        Edges[Edge3] = 1;
                    }
                    polyCounter++;
                }

                foreach (KeyValuePair<Tuple<short, short>, int> dic in Edges)
                {
                    if (dic.Value == 1)
                    {
                        if (!VerticesToChange.ContainsKey(dic.Key.Item1))
                        {
                            Vertices.Add(new P3DVertex(this, Vertices[dic.Key.Item1].x, Vertices[dic.Key.Item1].y,
                                Vertices[dic.Key.Item1].z));
                            VerticesToChange[dic.Key.Item1] = (short) (Vertices.Count - 1);
                            NumVertices++;
                        }
                        if (!VerticesToChange.ContainsKey(dic.Key.Item2))
                        {
                            Vertices.Add(new P3DVertex(this, Vertices[dic.Key.Item2].x, Vertices[dic.Key.Item2].y,
                                Vertices[dic.Key.Item2].z));
                            VerticesToChange[dic.Key.Item2] = (short)(Vertices.Count - 1);
                            NumVertices++;
                        }
                    }
                }

                for (int ind = curCounter; ind < polyCounter; ind++)
                {
                    if (VerticesToChange.ContainsKey(Polygons[ind].P1))
                    {
                        Polygons[ind].P1 = VerticesToChange[Polygons[ind].P1];
                    }
                    if (VerticesToChange.ContainsKey(Polygons[ind].P2))
                    {
                        Polygons[ind].P2 = VerticesToChange[Polygons[ind].P2];
                    }
                    if (VerticesToChange.ContainsKey(Polygons[ind].P3))
                    {
                        Polygons[ind].P3 = VerticesToChange[Polygons[ind].P3];
                    }
                }

                Edges.Clear();
                VerticesToChange.Clear();

                curCounter = polyCounter;
                for (int i = 0; i < texInfo.NumGouraudMetal; i++)
                {
                    short P1 = Polygons[polyCounter].P1;
                    short P2 = Polygons[polyCounter].P2;
                    short P3 = Polygons[polyCounter].P3;

                    Tuple<short, short> Edge1 = new Tuple<short, short>(P1, P2);
                    Tuple<short, short> Edge2 = new Tuple<short, short>(P2, P3);
                    Tuple<short, short> Edge3 = new Tuple<short, short>(P3, P1);

                    if (Edges.ContainsKey(Edge1))
                    {
                        Edges[Edge1]++;
                    }
                    else
                    {
                        Edges[Edge1] = 1;
                    }

                    if (Edges.ContainsKey(Edge2))
                    {
                        Edges[Edge2]++;
                    }
                    else
                    {
                        Edges[Edge2] = 1;
                    }

                    if (Edges.ContainsKey(Edge3))
                    {
                        Edges[Edge3]++;
                    }
                    else
                    {
                        Edges[Edge3] = 1;
                    }
                    polyCounter++;
                }

                foreach (KeyValuePair<Tuple<short, short>, int> dic in Edges)
                {
                    if (dic.Value == 1)
                    {
                        if (!VerticesToChange.ContainsKey(dic.Key.Item1))
                        {
                            Vertices.Add(new P3DVertex(this, Vertices[dic.Key.Item1].x, Vertices[dic.Key.Item1].y,
                                Vertices[dic.Key.Item1].z));
                            VerticesToChange[dic.Key.Item1] = (short) (Vertices.Count - 1);
                            NumVertices++;
                        }
                        if (!VerticesToChange.ContainsKey(dic.Key.Item2))
                        {
                            Vertices.Add(new P3DVertex(this, Vertices[dic.Key.Item2].x, Vertices[dic.Key.Item2].y,
                                Vertices[dic.Key.Item2].z));
                            VerticesToChange[dic.Key.Item2] = (short)(Vertices.Count - 1);
                            NumVertices++;
                        }
                    }
                }

                for (int ind = curCounter; ind < polyCounter; ind++)
                {
                    if (VerticesToChange.ContainsKey(Polygons[ind].P1))
                    {
                        Polygons[ind].P1 = VerticesToChange[Polygons[ind].P1];
                    }
                    if (VerticesToChange.ContainsKey(Polygons[ind].P2))
                    {
                        Polygons[ind].P2 = VerticesToChange[Polygons[ind].P2];
                    }
                    if (VerticesToChange.ContainsKey(Polygons[ind].P3))
                    {
                        Polygons[ind].P3 = VerticesToChange[Polygons[ind].P3];
                    }
                }

                Edges.Clear();
                VerticesToChange.Clear();

                curCounter = polyCounter;
                for (int i = 0; i < texInfo.NumGouraudMetalEnv; i++)
                {
                    short P1 = Polygons[polyCounter].P1;
                    short P2 = Polygons[polyCounter].P2;
                    short P3 = Polygons[polyCounter].P3;

                    Tuple<short, short> Edge1 = new Tuple<short, short>(P1, P2);
                    Tuple<short, short> Edge2 = new Tuple<short, short>(P2, P3);
                    Tuple<short, short> Edge3 = new Tuple<short, short>(P3, P1);

                    if (Edges.ContainsKey(Edge1))
                    {
                        Edges[Edge1]++;
                    }
                    else
                    {
                        Edges[Edge1] = 1;
                    }

                    if (Edges.ContainsKey(Edge2))
                    {
                        Edges[Edge2]++;
                    }
                    else
                    {
                        Edges[Edge2] = 1;
                    }

                    if (Edges.ContainsKey(Edge3))
                    {
                        Edges[Edge3]++;
                    }
                    else
                    {
                        Edges[Edge3] = 1;
                    }
                    polyCounter++;
                }

                foreach (KeyValuePair<Tuple<short, short>, int> dic in Edges)
                {
                    if (dic.Value == 1)
                    {
                        if (!VerticesToChange.ContainsKey(dic.Key.Item1))
                        {
                            Vertices.Add(new P3DVertex(this, Vertices[dic.Key.Item1].x, Vertices[dic.Key.Item1].y,
                                Vertices[dic.Key.Item1].z));
                            VerticesToChange[dic.Key.Item1] = (short) (Vertices.Count - 1);
                            NumVertices++;
                        }
                        if (!VerticesToChange.ContainsKey(dic.Key.Item2))
                        {
                            Vertices.Add(new P3DVertex(this, Vertices[dic.Key.Item2].x, Vertices[dic.Key.Item2].y,
                                Vertices[dic.Key.Item2].z));
                            VerticesToChange[dic.Key.Item2] = (short)(Vertices.Count - 1);
                            NumVertices++;
                        }
                    }
                }

                for (int ind = curCounter; ind < polyCounter; ind++)
                {
                    if (VerticesToChange.ContainsKey(Polygons[ind].P1))
                    {
                        Polygons[ind].P1 = VerticesToChange[Polygons[ind].P1];
                    }
                    if (VerticesToChange.ContainsKey(Polygons[ind].P2))
                    {
                        Polygons[ind].P2 = VerticesToChange[Polygons[ind].P2];
                    }
                    if (VerticesToChange.ContainsKey(Polygons[ind].P3))
                    {
                        Polygons[ind].P3 = VerticesToChange[Polygons[ind].P3];
                    }
                }

                Edges.Clear();
                VerticesToChange.Clear();

                curCounter = polyCounter;
                for (int i = 0; i < texInfo.NumShining; i++)
                {
                    short P1 = Polygons[polyCounter].P1;
                    short P2 = Polygons[polyCounter].P2;
                    short P3 = Polygons[polyCounter].P3;

                    Tuple<short, short> Edge1 = new Tuple<short, short>(P1, P2);
                    Tuple<short, short> Edge2 = new Tuple<short, short>(P2, P3);
                    Tuple<short, short> Edge3 = new Tuple<short, short>(P3, P1);

                    if (Edges.ContainsKey(Edge1))
                    {
                        Edges[Edge1]++;
                    }
                    else
                    {
                        Edges[Edge1] = 1;
                    }

                    if (Edges.ContainsKey(Edge2))
                    {
                        Edges[Edge2]++;
                    }
                    else
                    {
                        Edges[Edge2] = 1;
                    }

                    if (Edges.ContainsKey(Edge3))
                    {
                        Edges[Edge3]++;
                    }
                    else
                    {
                        Edges[Edge3] = 1;
                    }
                    polyCounter++;
                }

                foreach (KeyValuePair<Tuple<short, short>, int> dic in Edges)
                {
                    if (dic.Value == 1)
                    {
                        if (!VerticesToChange.ContainsKey(dic.Key.Item1))
                        {
                            Vertices.Add(new P3DVertex(this, Vertices[dic.Key.Item1].x, Vertices[dic.Key.Item1].y,
                                Vertices[dic.Key.Item1].z));
                            VerticesToChange[dic.Key.Item1] = (short) (Vertices.Count - 1);
                            NumVertices++;
                        }
                        if (!VerticesToChange.ContainsKey(dic.Key.Item2))
                        {
                            Vertices.Add(new P3DVertex(this, Vertices[dic.Key.Item2].x, Vertices[dic.Key.Item2].y,
                                Vertices[dic.Key.Item2].z));
                            VerticesToChange[dic.Key.Item2] = (short) (Vertices.Count - 1);
                            NumVertices++;
                        }
                    }
                }

                for (int ind = curCounter; ind < polyCounter; ind++)
                {
                    if (VerticesToChange.ContainsKey(Polygons[ind].P1))
                    {
                        Polygons[ind].P1 = VerticesToChange[Polygons[ind].P1];
                    }
                    if (VerticesToChange.ContainsKey(Polygons[ind].P2))
                    {
                        Polygons[ind].P2 = VerticesToChange[Polygons[ind].P2];
                    }
                    if (VerticesToChange.ContainsKey(Polygons[ind].P3))
                    {
                        Polygons[ind].P3 = VerticesToChange[Polygons[ind].P3];
                    }
                }

                Edges.Clear();
                VerticesToChange.Clear();
            }
        }


        public void SeparateUVVertices()
        {
            Dictionary<short, Tuple<float, float>> VerticeToUV = new Dictionary<short, Tuple<float, float>>();
            foreach (TexturePolygon poly in Polygons)
            {
                if (VerticeToUV.ContainsKey(poly.P1))
                {
                    var UVS = VerticeToUV[poly.P1];
                    if (!(Math.Abs(UVS.Item1 - poly.U1) < 0.0001f && Math.Abs(UVS.Item2 - poly.V1) < 0.0001f))
                    {
                        poly.P1 = GetVerticeForPolyUV(poly.P1, poly.U1, poly.V1, VerticeToUV);
                    }
                }

                if (VerticeToUV.ContainsKey(poly.P2))
                {
                    var UVS = VerticeToUV[poly.P2];
                    if (!(Math.Abs(UVS.Item1 - poly.U2) < 0.0001f && Math.Abs(UVS.Item2 - poly.V2) < 0.0001f))
                    {
                        poly.P2 = GetVerticeForPolyUV(poly.P2, poly.U2, poly.V2, VerticeToUV);
                    }
                }

                if (VerticeToUV.ContainsKey(poly.P3))
                {
                    var UVS = VerticeToUV[poly.P3];
                    if (!(Math.Abs(UVS.Item1 - poly.U3) < 0.0001f && Math.Abs(UVS.Item2 - poly.V3) < 0.0001f))
                    {
                        poly.P3 = GetVerticeForPolyUV(poly.P3, poly.U3, poly.V3, VerticeToUV);
                    }
                }
            }
        }

        private short GetVerticeForPolyUV(short P, float U, float V, Dictionary<short, Tuple<float, float>> VerticeToUV)
        {
            P3DVertex orgVertex = Vertices[P];
            if (orgVertex.NextToCheck != null)
            {
                var UVS = VerticeToUV[orgVertex.NextToCheck.Value];
                if ((Math.Abs(UVS.Item1 - U) < 0.0001f && Math.Abs(UVS.Item2 - V) < 0.0001f))
                {
                    return orgVertex.NextToCheck.Value;
                }
                else
                {
                    return GetVerticeForPolyUV(orgVertex.NextToCheck.Value, U, V, VerticeToUV);
                }
            }

            P3DVertex newVert = new P3DVertex(orgVertex.x, orgVertex.y, orgVertex.z);
            orgVertex.NextToCheck = (short) Vertices.Count;
            VerticeToUV[(short)Vertices.Count] = new Tuple<float, float>(U,V);
            Vertices.Add(newVert);
            NumVertices++;
            return (short) (Vertices.Count - 1);
        }

        //private int GetVertexForMaterial(float x, float y, float u, float v)

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

        public void CalculateExtent()
        {
            float minX = Vertices[0].x;
            float maxX = Vertices[0].x;
            float minY = Vertices[0].y;
            float maxY = Vertices[0].y;
            float minZ = Vertices[0].z;
            float maxZ = Vertices[0].z;
            foreach (P3DVertex vert in Vertices)
            {
                minX = Math.Min(minX, vert.x);
                maxX = Math.Max(maxX, vert.x);
                minY = Math.Min(minY, vert.y);
                maxY = Math.Max(maxY, vert.y);
                minZ = Math.Min(minZ, vert.z);
                maxZ = Math.Max(maxZ, vert.z);
            }
            Length = maxX - minX;
            Height = maxY - minY;
            Depth = maxZ - minZ;
            LocalPos.x = ((maxX + minX) / 2);
            LocalPos.y = ((maxY + minY) / 2);
            LocalPos.z = ((maxZ + minZ) / 2);
            
        }

        public void CalculateLocalPos(P3DVertex origin)
        {
            LocalPos.x -= origin.x;
            LocalPos.y -= origin.y;
            LocalPos.z -= origin.z;
        }

        public void MoveVerticesToOrigin()
        {
            foreach (P3DVertex vert in Vertices)
            {
                vert.x -= LocalPos.x;
                vert.y -= LocalPos.y;
                vert.z -= LocalPos.z;
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

        private uint FlagFrom3DSName(string name)
        {
            uint flag = (uint) MeshFlags.VISIBLE;
            if (name.ToLower() == "main")
            {
                flag += (uint) MeshFlags.MAIN;
            }
            if (name.ToLower() == "mainshad")
            {
                flag += (uint) MeshFlags.TRACING_SHAPE;
                flag -= (uint)MeshFlags.VISIBLE;
            }
            if (name.ToLower() == "maincoll")
            {
                flag += (uint) MeshFlags.COLLISION_SHAPE;
                flag -= (uint) MeshFlags.VISIBLE;
            }
            return flag;
        }

        public Int32 GetMeshSize()
        {
            //Int32 size = 7; // SUBMESH id
            Int32 size = Name.Length + 1; //Name + NULL
            size += 4; //FLAGS
            size += 12; //POSITION
            size += 12; //EXTENT
            size += (Info.Count * 14); //TEXTURES INFO
            size += 2; //VerticesCount
            size += Vertices.Count * 12; //Vertices size
            size += 2; //Polygons count
            size += Polygons.Count * 30; //Polygons size

            Size = size; //Set submesh size

            size += 7; //SUBMESH id
            size += 4; //Submesh size

            return size;
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
                new InputText(this, "Size:", Size.ToString(), false, ""),
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
