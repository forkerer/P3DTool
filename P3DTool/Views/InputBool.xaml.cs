using System;
using System.Windows;
using System.Windows.Controls;
using P3DTool.DataModels.DataTypes;

namespace P3DTool.Views
{
    /// <summary>
    /// Interaction logic for InputBool.xaml
    /// </summary>
    public partial class InputBool : UserControl, IToolPanel
    {

        public P3DElement ParentElement { get; set; }
        private string ValToChange;

        public InputBool(P3DElement parent, string description, bool value, bool editable, string valToChange)
        {
            InitializeComponent();
            ParentElement = parent;
            Value.Content = description;
            Value.IsChecked = value;
            ValToChange = valToChange;
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (ValToChange == null) { return; }
            if (ParentElement is Light)
            {
                if (ValToChange == "corona")
                {
                    ((Light)ParentElement).ShowCorona = Value.IsChecked.Value;
                }
                else if ((ValToChange == "lensFlare"))
                {
                    ((Light)ParentElement).ShowLensFlare = Value.IsChecked.Value;
                }
                else if ((ValToChange == "lightUpEnvivornment"))
                {
                    ((Light)ParentElement).LightUpEnvivornment = Value.IsChecked.Value;
                }
            }
            else if (ParentElement is Mesh)
            {
                MessageBox.Show(ValToChange);
                //uint flag = (uint)(Enum.Parse(typeof(MeshFlags), ValToChange));
                uint flag = (uint)(MeshFlags) (Enum.Parse(typeof(MeshFlags), ValToChange));
                bool isSet = (((Mesh)ParentElement).Flags & flag) == flag;
////                bool isSet = ((((Mesh) ParentElement).Flags & (uint) Enum.Parse(typeof(MeshFlags), ValToChange)) ==
////                              (uint) Enum.Parse(typeof(MeshFlags), ValToChange));
//
                if (isSet)
                {
                    ((Mesh)ParentElement).Flags -= flag;
                }
                else
                {
                    ((Mesh)ParentElement).Flags += flag;
                }



                //                if (ValToChange == "MAIN")
                //                {
                //                    bool isSet = ((((Mesh) ParentElement).Flags & (int)MeshFlags.MAIN) == (int)MeshFlags.MAIN);
                //                    if (isSet)
                //                    {
                //                        ((Mesh) ParentElement).Flags -= (int) MeshFlags.MAIN;
                //                    }
                //                    else
                //                    {
                //                        ((Mesh)ParentElement).Flags += (int)MeshFlags.MAIN;
                //                    }
                //                }
                //                else if (ValToChange == "VISIBLE")
                //                {
                //                    bool isSet = ((((Mesh)ParentElement).Flags & (int)MeshFlags.VISIBLE) == (int)MeshFlags.VISIBLE);
                //                    if (isSet)
                //                    {
                //                        ((Mesh)ParentElement).Flags -= (int)MeshFlags.VISIBLE;
                //                    }
                //                    else
                //                    {
                //                        ((Mesh)ParentElement).Flags += (int)MeshFlags.VISIBLE;
                //                    }
                //                }
                //                else if (ValToChange == "TRACING_SHAPE")
                //                {
                //                    bool isSet = ((((Mesh)ParentElement).Flags & (int)MeshFlags.TRACING_SHAPE) == (int)MeshFlags.TRACING_SHAPE);
                //                    if (isSet)
                //                    {
                //                        ((Mesh)ParentElement).Flags -= (int)MeshFlags.TRACING_SHAPE;
                //                    }
                //                    else
                //                    {
                //                        ((Mesh)ParentElement).Flags += (int)MeshFlags.TRACING_SHAPE;
                //                    }
                //                }
                //                else if (ValToChange == "COLLISION_SHAPE")
                //                {
                //                    bool isSet = ((((Mesh)ParentElement).Flags & (int)MeshFlags.COLLISION_SHAPE) == (int)MeshFlags.COLLISION_SHAPE);
                //                    if (isSet)
                //                    {
                //                        ((Mesh)ParentElement).Flags -= (int)MeshFlags.COLLISION_SHAPE;
                //                    }
                //                    else
                //                    {
                //                        ((Mesh)ParentElement).Flags += (int)MeshFlags.COLLISION_SHAPE;
                //                    }
                //                }
                //                else if (ValToChange == "DETACHABLE_PART")
                //                {
                //                    bool isSet = ((((Mesh)ParentElement).Flags & (int)MeshFlags.DETACHABLE_PART) == (int)MeshFlags.DETACHABLE_PART);
                //                    if (isSet)
                //                    {
                //                        ((Mesh)ParentElement).Flags -= (int)MeshFlags.DETACHABLE_PART;
                //                    }
                //                    else
                //                    {
                //                        ((Mesh)ParentElement).Flags += (int)MeshFlags.DETACHABLE_PART;
                //                    }
                //                }
                //                else if (ValToChange == "BREAKABLE_GLASS")
                //                {
                //                    bool isSet = ((((Mesh)ParentElement).Flags & (int)MeshFlags.BREAKABLE_GLASS) == (int)MeshFlags.BREAKABLE_GLASS);
                //                    if (isSet)
                //                    {
                //                        ((Mesh)ParentElement).Flags -= (int)MeshFlags.BREAKABLE_GLASS;
                //                    }
                //                    else
                //                    {
                //                        ((Mesh)ParentElement).Flags += (int)MeshFlags.BREAKABLE_GLASS;
                //                    }
                //                }
                //                else if (ValToChange == "BREAKABLE_PLASTIC")
                //                {
                //                    bool isSet = ((((Mesh)ParentElement).Flags & (int)MeshFlags.BREAKABLE_PLASTIC) == (int)MeshFlags.BREAKABLE_PLASTIC);
                //                    if (isSet)
                //                    {
                //                        ((Mesh)ParentElement).Flags -= (int)MeshFlags.BREAKABLE_PLASTIC;
                //                    }
                //                    else
                //                    {
                //                        ((Mesh)ParentElement).Flags += (int)MeshFlags.BREAKABLE_PLASTIC;
                //                    }
                //                }
                //                else if (ValToChange == "DETACHABLE_PART")
                //                {
                //                    bool isSet = ((((Mesh)ParentElement).Flags & (int)MeshFlags.DETACHABLE_PART) == (int)MeshFlags.DETACHABLE_PART);
                //                    if (isSet)
                //                    {
                //                        ((Mesh)ParentElement).Flags -= (int)MeshFlags.DETACHABLE_PART;
                //                    }
                //                    else
                //                    {
                //                        ((Mesh)ParentElement).Flags += (int)MeshFlags.DETACHABLE_PART;
                //                    }
                //                }
            }
        }
    }
}
