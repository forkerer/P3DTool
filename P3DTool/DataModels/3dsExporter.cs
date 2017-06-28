using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using lib3ds.Net;
using P3DTool.DataModels.DataTypes;

namespace P3DTool.DataModels
{
    public static class _3dsExporter
    {
        public static void ExportP3D(P3D p3d, string path)
        {
            Lib3dsFile file = LIB3DS.lib3ds_file_new();
            //file.frames = 0;

//            Lib3dsMaterial mat = LIB3DS.lib3ds_material_new("c_white");
//            //LIB3DS.lib3ds_file_insert_material(file, mat, -1);
//            mat.diffuse[0] = 1;
//            mat.diffuse[1] = 1;
//            mat.diffuse[2] = 1;

            foreach (Mesh P3Dmesh in p3d.MeshesChunk.Meshes)
            {
                Lib3dsMesh mesh = LIB3DS.lib3ds_mesh_new(P3Dmesh.Name);
                Lib3dsMeshInstanceNode inst;

                LIB3DS.lib3ds_file_insert_mesh(file, mesh, -1);
                LIB3DS.lib3ds_mesh_resize_vertices(mesh,(ushort)P3Dmesh.NumVertices,true,false);
                for (int i = 0; i < P3Dmesh.NumVertices; i++)
                {
                    mesh.vertices[i].x = P3Dmesh.Vertices[i].x;
                    mesh.vertices[i].z = P3Dmesh.Vertices[i].y;
                    mesh.vertices[i].y = P3Dmesh.Vertices[i].z;
                    mesh.texcos[i].s = findVerticeU(i, P3Dmesh);
                    mesh.texcos[i].t = findVerticeV(i, P3Dmesh);
                }

                LIB3DS.lib3ds_mesh_resize_faces(mesh, (ushort)P3Dmesh.NumPolys);
                for (int i = 0; i < P3Dmesh.NumPolys; i++)
                {
                    mesh.faces[i].index[2] = (ushort)P3Dmesh.Polygons[i].P1;
                    mesh.faces[i].index[1] = (ushort)P3Dmesh.Polygons[i].P2;
                    mesh.faces[i].index[0] = (ushort)P3Dmesh.Polygons[i].P3;
                    mesh.faces[i].material = _3dsMaterials.Instance.GetMaterial(P3Dmesh.Polygons[i].Texture, P3Dmesh.Polygons[i].Material, file);
                    mesh.faces[i].smoothing_group = (uint)1;
                }
                float[] pos = {P3Dmesh.LocalPos.x, P3Dmesh.LocalPos.z, P3Dmesh.LocalPos.y};
                inst = LIB3DS.lib3ds_node_new_mesh_instance(mesh, String.Empty, pos, null, null);
                LIB3DS.lib3ds_file_append_node(file, inst, null);

            }

            foreach (Light p3dlight in p3d.LightsChunk.Lights)
            {
                Lib3dsLight light = LIB3DS.lib3ds_light_new(p3dlight.Name);
                LIB3DS.lib3ds_file_insert_light(file, light, -1);
                light.color[0] = Convert.ToSingle(p3dlight.GetColorRed())/255;
                light.color[1] = Convert.ToSingle(p3dlight.GetColorGreen()) / 255;
                light.color[2] = Convert.ToSingle(p3dlight.GetColorBlue()) / 255;
                float[] pos = {p3dlight.Position.x, p3dlight.Position.z, p3dlight.Position.y};
                light.position = pos;
                light.inner_range = p3dlight.Radius;
                light.multiplier = 1.5f;
                Lib3dsOmnilightNode inst = LIB3DS.lib3ds_node_new_omnilight(light);

                LIB3DS.lib3ds_file_append_node(file, inst, null);
            }

            if (!LIB3DS.lib3ds_file_save(file, path))
                MessageBox.Show("ERROR: Saving 3ds file failed!");

            LIB3DS.lib3ds_file_free(file);
        }

        public static float findVerticeU(int id, Mesh mesh)
        {
            foreach (TexturePolygon poly in mesh.Polygons)
            {
                if (poly.P1 == id)
                {
                    return poly.U1;
                }
                else if (poly.P2 == id)
                {
                    return poly.U2;
                }
                else if (poly.P3 == id)
                {
                    return poly.U3;
                }
            }
            return 0;
        }

        public static float findVerticeV(int id, Mesh mesh)
        {
            foreach (TexturePolygon poly in mesh.Polygons)
            {
                if (poly.P1 == id)
                {
                    return poly.V1;
                }
                else if (poly.P2 == id)
                {
                    return poly.V2;
                }
                else if (poly.P3 == id)
                {
                    return poly.V3;
                }
            }
            return 0;
        }
       
    }
}
