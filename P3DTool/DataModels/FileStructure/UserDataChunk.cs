using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows.Controls;
using P3DTool.DataModels.DataTypes;
using P3DTool.Views;

namespace P3DTool.DataModels.FileStructure
{
    public class UserDataChunk : P3DElement
    {
        public P3D Parent { get; set; }
        internal TreeViewItem TreeItem { get; set; }
        public byte[] ChunkIdTemplate { get; } = Encoding.ASCII.GetBytes("USER");

        public int Size { get; set; }
        public byte[] UserData { get; set; }

        public UserDataChunk(P3D parent)
        {
            Parent = parent;
            TreeItem = new TreeViewItem {Header = new P3DElementView(this, "User data chunk")};
            Parent.TreeItem.Items.Add(TreeItem);
        }

        public override ArrayList GetItemInfo()
        {
            ArrayList itemInfo = new ArrayList
            {
                new InputText(this, "Userdata chunk size:", Size + " Bytes", false, ""),
                new InputText(this, "Userdata data:", Encoding.Default.GetString(UserData), false, ""),
            };

            return itemInfo;
        }

        public string WriteChunk(BinaryWriter writer)
        {
            if (Size == 0)
            {
                return "User Data chunk size equals 0";
            }

            writer.Write(ChunkIdTemplate);
            writer.Write(Size);
            writer.Write(UserData);

            return String.Empty;
        }
    }
}
