using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using lib3ds.Net;
using P3DTool.DataModels.DataTypes;
using P3DTool.DataModels.FileStructure;
using P3DTool.Properties;
using P3DTool.Views;

namespace P3DTool.DataModels
{
    public delegate void StatusUpdatedEventHandler(StatusUpdatedEventArguments args);

    public class P3D : P3DElement
    {
        public MainWindow Parent { get; }
        public TreeViewItem TreeItem { get; } = new TreeViewItem();
        public event StatusUpdatedEventHandler StatusUpdated;

        public byte[] MagicWord { get; } = Encoding.ASCII.GetBytes("P3D");

        public byte Version { get; set; }
        public Extent Size { get; set; }

        public TextureChunk TextureChunk { get; set; }
        public LightsChunk LightsChunk { get; set; }
        public MeshesChunk MeshesChunk { get; set; }
        public UserDataChunk UserDataChunk { get; set; }

        public P3D(MainWindow parent)
        {
            Parent = parent;
            lock (TreeItem)
            {
                TreeItem.Header = new P3DElementView(this, "P3D Model", new Bitmap(Resources.car));
//                = new TreeViewItem
//                {
//                    Header = new P3DElementView(this, "P3D Model", new Bitmap(Resources.car))
//                };
                Parent.P3DViewItems.Add(TreeItem);
            } 
        }

        public async Task LoadP3D(string path)
        {
            await Task.Run(() => LoadP3D(File.Open(path, FileMode.Open)));
        }

        public async Task LoadP3D(Stream data)
        {
            using (var reader = new BinaryReader(data))
            {
                reader.BaseStream.Position = 0;
                bool success;

                byte[] magicWord = reader.ReadBytes(3);
                if (!Utility.ByteArrayCompare(magicWord, MagicWord))
                {
                    MessageBox.Show("Magic word isn't valid, got " + Encoding.ASCII.GetString(magicWord) + " Should be: " + Encoding.ASCII.GetString(MagicWord));
                    return;
                }

                Version = reader.ReadByte();
                //MessageBox.Show("Version: " + version.ToString());

                Size = new Extent
                {
                    XSize = reader.ReadSingle(),
                    YSize = reader.ReadSingle(),
                    ZSize = reader.ReadSingle(),
                    IsImportedSize = true
                };

                //MessageBox.Show(size.xSize.ToString() + " - " + size.ySize.ToString() + " - " + size.zSize.ToString());

                StatusUpdated(new StatusUpdatedEventArguments("Loading textures info from file", 25));
                await Task.Delay(0).ConfigureAwait(false);
                TextureChunk = new TextureChunk(this);
                byte[] texHeader = reader.ReadBytes(3);
                if (!Utility.ByteArrayCompare(texHeader, TextureChunk.ChunkIdTemplate))
                {
                    StatusUpdated(new StatusUpdatedEventArguments("Failed to load file", 0));
                    MessageBox.Show("Expected TEX chunk header, got " + Encoding.ASCII.GetString(texHeader));
                    return;
                }

                TextureChunk.Size = reader.ReadInt32();
                TextureChunk.TexNum = reader.ReadByte();

                
                success = TextureChunk.ReadChunk(reader);
                if (!success)
                {
                    StatusUpdated(new StatusUpdatedEventArguments("Failed to load file", 0));
                    MessageBox.Show("Couldn't parse Textures chunk");
                    return;
                }

                StatusUpdated(new StatusUpdatedEventArguments("Loading lights from file", 40));
                await Task.Delay(0).ConfigureAwait(false);
                LightsChunk = new LightsChunk(this);
                byte[] lightsHeader = reader.ReadBytes(6);
                if (!Utility.ByteArrayCompare(lightsHeader, LightsChunk.ChunkIdTemplate))
                {
                    StatusUpdated(new StatusUpdatedEventArguments("Failed to load file", 0));
                    MessageBox.Show("Expected LIGHTS chunk header, got " + Encoding.ASCII.GetString(lightsHeader));
                    return;
                }

                LightsChunk.Size = reader.ReadInt32();
                LightsChunk.LightsNum = reader.ReadInt16();

                success = LightsChunk.ReadChunk(reader);
                if (!success)
                {
                    StatusUpdated(new StatusUpdatedEventArguments("Failed to load file", 0));
                    MessageBox.Show("Couldn't parse Lights chunk");
                    return;
                }

                StatusUpdated(new StatusUpdatedEventArguments("Loading meshes from file", 40));
                await Task.Delay(0).ConfigureAwait(false);
                MeshesChunk = new MeshesChunk(this);
                byte[] meshesHeader = reader.ReadBytes(6);
                if (!Utility.ByteArrayCompare(meshesHeader, MeshesChunk.ChunkIdTemplate))
                {
                    StatusUpdated(new StatusUpdatedEventArguments("Failed to load file", 0));
                    MessageBox.Show("Expected MESHES chunk header, got " + Encoding.ASCII.GetString(meshesHeader));
                    return;
                }

                MeshesChunk.Size = reader.ReadInt32();
                MeshesChunk.MeshesNum = reader.ReadInt16();


                success = await MeshesChunk.ReadChunk(reader);
                if (!success)
                {
                    StatusUpdated(new StatusUpdatedEventArguments("Failed to load file", 0));
                    MessageBox.Show("Couldn't parse Meshes chunk");
                    return;
                }

                MeshesChunk.SeparateUVVertices();
                MeshesChunk.SeparateSubMeshesEdges();
                

                StatusUpdated(new StatusUpdatedEventArguments("Loading userdata from file", 80));
                await Task.Delay(0).ConfigureAwait(false);
                UserDataChunk = new UserDataChunk(this);
                byte[] userDataHeader = reader.ReadBytes(4);
                if (!Utility.ByteArrayCompare(userDataHeader, UserDataChunk.ChunkIdTemplate))
                {
                    StatusUpdated(new StatusUpdatedEventArguments("Failed to load file", 0));
                    MessageBox.Show("Expected USER chunk header, got " + Encoding.ASCII.GetString(userDataHeader));
                    return;
                }
                UserDataChunk.Size = reader.ReadInt32();
                UserDataChunk.UserData = reader.ReadBytes(UserDataChunk.Size);

                StatusUpdated(new StatusUpdatedEventArguments("Finished loading", 100));
                await Task.Delay(0).ConfigureAwait(false);
                reader.Close();
            }
        }

        public void SaveP3D(string path)
        {
            BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create));
            String message;
            // Write header
            writer.Write(MagicWord);
            writer.Write((byte)0x02);
            writer.Write(Size.GetXSize());
            writer.Write(Size.GetYSize());
            writer.Write(Size.GetZSize());

            // Write textures chunk
            message = TextureChunk.WriteChunk(writer);
            if (!message.Equals(String.Empty))
            {
                MessageBox.Show("Saving failed while writing Textures Chunk, Message: " + message);
                return;
            }

            // Write lights chunk
            message = LightsChunk.WriteChunk(writer);
            if (!message.Equals(String.Empty))
            {
                MessageBox.Show("Saving failed while writing Lights Chunk, Message: " + message);
                return;
            }

            // Write meshes chunk
            //MeshesChunk.CalculateMeshChunkSize();
            message = MeshesChunk.WriteChunk(writer);
            if (!message.Equals(String.Empty))
            {
                MessageBox.Show("Saving failed while writing Meshes Chunk, Message: " + message);
                return;
            }

            // Write userdata chunk
            message = UserDataChunk.WriteChunk(writer);
            if (!message.Equals(String.Empty))
            {
                MessageBox.Show("Saving failed while writing User Data Chunk, Message: " + message);
                return;
            }

            writer.Close();
        }

        //        public void LoadP3DAssimp(Scene scene)
        //        {
        //            Version = 0x02;
        //            Size = new Extent();
        //
        //            TextureChunk = new TextureChunk(this);
        //            foreach (Material mat in scene.Materials)
        //            {
        //                if (!TextureChunk.IsTextureInList(mat.TextureDiffuse.FilePath.ToLower()))
        //                {
        //                    TextureChunk.TextureNames.Add(new TextureName(TextureChunk,mat.TextureDiffuse.FilePath.ToLower()));
        //                }
        //            }
        //            TextureChunk.CalculateSizeFromTexturesList();
        //            TextureChunk.CalculateTexNum();
        //
        //            LightsChunk = new LightsChunk(this);
        //            foreach (Assimp.Light light in scene.Lights)
        //            {
        //                string name = light.Name;
        //                float x = light.Position.X;
        //                float y = light.Position.Y;
        //                float z = light.Position.Z;
        //                float radius = 1; //ASSIMP doesn't save point light radius
        //                int color = LightsChunk.ColorFromRGB(Convert.ToInt32(light.ColorDiffuse.R*255), Convert.ToInt32(light.ColorDiffuse.G * 255), Convert.ToInt32(light.ColorDiffuse.B * 255));
        //                bool corona = false;
        //                bool lensFlare = false;
        //                bool lightUpEnv = true;
        //                LightsChunk.Lights.Add(new DataTypes.Light(LightsChunk, name, x, y, z, radius, color, corona, lensFlare, lightUpEnv));
        //            }
        //            LightsChunk.CalculateSizeFromLightsList();
        //            LightsChunk.CalculateLightsNum();
        //
        //            MeshesChunk = new MeshesChunk(this);
        //            MeshesChunk.MeshesNum = Convert.ToInt16(scene.MeshCount);
        //            MeshesChunk.Size = 0;
        //            //MessageBox.Show(scene.RootNode.Name);
        //            foreach (Assimp.Mesh mesh in scene.Meshes)
        //            {
        //               //MessageBox.Show();
        //            }
        //        }

        public void Load3DS(Lib3dsFile scene)
        {
            Version = 0x02;
            Size = new Extent();
            
            TextureChunk = new TextureChunk(this);
            foreach (Lib3dsMaterial mat in scene.materials)
            {
                if (mat.texture1_map.name.ToLower().Equals(String.Empty))
                {
                    continue;
                }
                if (!TextureChunk.IsTextureInList(mat.texture1_map.name.ToLower()))
                {
                    TextureChunk.TextureNames.Add(new TextureName(TextureChunk,mat.texture1_map.name.ToLower()));
                }
            }
            if (TextureChunk.TextureNames.Count == 0)
            {
                TextureChunk.TextureNames.Add(new TextureName(TextureChunk,"colwhite.tga"));
            }
            TextureChunk.CalculateSizeFromTexturesList();
            TextureChunk.CalculateTexNum();

            LightsChunk = new LightsChunk(this);
            foreach (Lib3dsLight light in scene.lights)
            {
                string name = light.name;
                float x = light.position[0];
                float y = light.position[2];
                float z = light.position[1];
                float radius = light.inner_range;
                int color = LightsChunk.ColorFromRGB(Convert.ToInt32(light.color[0]*255), Convert.ToInt32(light.color[1] * 255), Convert.ToInt32(light.color[2] * 255));
                bool corona = false;
                bool lensFlare = false;
                bool lightUpEnv = true;
                LightsChunk.Lights.Add(new DataTypes.Light(LightsChunk, name, x, y, z, radius, color, corona, lensFlare, lightUpEnv));
            }
            LightsChunk.CalculateSizeFromLightsList();
            LightsChunk.CalculateLightsNum();

            MeshesChunk = new MeshesChunk(this);
            MeshesChunk.MeshesNum = Convert.ToInt16(scene.meshes.Count);
            MeshesChunk.Size = 0;
                        //MessageBox.Show(scene.RootNode.Name);
            foreach (Lib3dsMesh mesh in scene.meshes)
            {
                Mesh newMesh = new Mesh(MeshesChunk);
                newMesh.Parse3DSMesh(scene, mesh);
                newMesh.SortPolygonsAndGenerateTextureInfo();
                newMesh.SeparateHardEdges();
                newMesh.SeparateUVVertices();
                newMesh.CalculateExtent();
                MeshesChunk.Meshes.Add(newMesh);
            }

            //MeshesChunk.SeparateSubMeshesEdges();
            MeshesChunk.CheckFlagsValidity();
            MeshesChunk.CalculateMeshChunkSize();
            P3DVertex origin = MeshesChunk.CalculateMeshesLocalPos();
            MeshesChunk.MoveMeshesToOrigin();
            LightsChunk.CalculateLightsPostionRelativeToOrigin(origin);

            Size.CalculateExtentFromMeshes(MeshesChunk.Meshes);

            UserDataChunk = new UserDataChunk(this);
            UserDataChunk.Size = 4;
            UserDataChunk.UserData = new byte[] {0, 0, 0, 0};
        }

        public override ArrayList GetItemInfo()
        {
            ArrayList itemInfo = new ArrayList
            {
                new InputText(this,"Version:",Version.ToString(),false, ""),
                new InputText(this,"xSize:",Size.GetXSize().ToString(),false, ""),
                new InputText(this,"ySize:",Size.GetYSize().ToString(),false, ""),
                new InputText(this,"zSize:",Size.GetZSize().ToString(),false, ""),
//                "Version: " + Version,
//                "xSize: " + Size.XSize,
//                "ySize: " + Size.YSize,
//                "zSize: " + Size.ZSize
            };

            return itemInfo;
        }

        public void RaiseStatusUpdatedEvent(StatusUpdatedEventArguments args)
        {
            StatusUpdated(args);
        }

        
    }
}
