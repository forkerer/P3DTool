using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using P3DTool.DataModels.DataTypes;

namespace P3DTool.Views
{
    /// <summary>
    /// Interaction logic for InputText.xaml
    /// </summary>
    public partial class InputText : UserControl, IToolPanel
    {
        public P3DElement ParentElement { get; set; }
        private string ValToChange;

        public InputText(P3DElement parent, string description, string value, bool editable, string valToChange)
        {
            InitializeComponent();
            ParentElement = parent;
            Description.Text = description;
            Description.IsEnabled = editable;
            Value.Text = value;
            Value.IsEnabled = editable;
            ValToChange = valToChange;
        }

        private void Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ParentElement is Light)
            {
                if (ValToChange == "name")
                {
                    if (!IsValidName(Value.Text))
                    {
                        return;
                    }
                    ((Light) ParentElement).Name = Value.Text.ToLower();
                    ((P3DElementView)((Light) ParentElement).TreeItem.Header).content.Text = Value.Text.ToLower();
                    ((Light) ParentElement).Parent.CalculateSizeFromLightsList();
                }
                else if (ValToChange == "posX")
                {
                    if (!IsValidFloat(Value.Text))
                    {
                        return;
                    }
                    ((Light)ParentElement).Position.x = float.Parse(Value.Text);
                }
                else if (ValToChange == "posY")
                {
                    if (!IsValidFloat(Value.Text))
                    {
                        return;
                    }
                    ((Light)ParentElement).Position.y = float.Parse(Value.Text);
                }
                else if (ValToChange == "posZ")
                {
                    if (!IsValidFloat(Value.Text))
                    {
                        return;
                    }
                    ((Light)ParentElement).Position.z = float.Parse(Value.Text);
                }
                else if (ValToChange == "radius")
                {
                    if (!IsValidFloat(Value.Text))
                    {
                        return;
                    }
                    ((Light)ParentElement).Radius = float.Parse(Value.Text);
                }
                else if (ValToChange == "colR")
                {
                    if (!IsValidColor(Value.Text))
                    {
                        return;
                    }
                    ((Light)ParentElement).SetColorRed(Math.Min(255,Math.Max(0,int.Parse(Value.Text))));
                }
                else if (ValToChange == "colG")
                {
                    if (!IsValidColor(Value.Text))
                    {
                        return;
                    }
                    ((Light)ParentElement).SetColorGreen(Math.Min(255, Math.Max(0, int.Parse(Value.Text))));
                }
                else if (ValToChange == "colB")
                {
                    if (!IsValidColor(Value.Text))
                    {
                        return;
                    }
                    ((Light)ParentElement).SetColorBlue(Math.Min(255, Math.Max(0, int.Parse(Value.Text))));
                }
            }
            else if (ParentElement is Mesh)
            {
                if (ValToChange == "name")
                {
                    if (!IsValidName(Value.Text))
                    {
                        return;
                    }
                    ((Mesh)ParentElement).Name = Value.Text.ToLower();
                    ((P3DElementView)((Mesh)ParentElement).TreeItem.Header).content.Text = Value.Text.ToLower();
                }
                else if (ValToChange == "posX")
                {
                    if (!IsValidFloat(Value.Text))
                    {
                        return;
                    }
                    ((Mesh)ParentElement).LocalPos.x = float.Parse(Value.Text);
                }
                else if (ValToChange == "posY")
                {
                    if (!IsValidFloat(Value.Text))
                    {
                        return;
                    }
                    ((Mesh)ParentElement).LocalPos.y = float.Parse(Value.Text);
                }
                else if (ValToChange == "posZ")
                {
                    if (!IsValidFloat(Value.Text))
                    {
                        return;
                    }
                    ((Mesh)ParentElement).LocalPos.z = float.Parse(Value.Text);
                }
            }
            else if (ParentElement is TextureName)
            {
                if (ValToChange == "name")
                {
                    if (!IsValidTextureName(Value.Text))
                    {
                        return;
                    }
                    ((TextureName) ParentElement).Name = Value.Text.ToLower();
                    ((P3DElementView) ((TextureName) ParentElement).TreeItem.Header).content.Text = Value.Text.ToLower();
                }
            }
        }

        private bool IsValidColor(string val)
        {
            int num;
            return int.TryParse(val, out num);
        }

        private bool IsValidFloat(string val)
        {
            float num;
            return float.TryParse(val, out num);
        }

        private bool IsValidName(string val)
        {
            return Regex.IsMatch(val, @"^[a-zA-Z0-9_]+$", RegexOptions.IgnoreCase);
        }

        private bool IsValidTextureName(string val)
        {
            return Regex.IsMatch(val, @"^[a-zA-Z0-9_]+\.(tga)$", RegexOptions.IgnoreCase);
        }


    }
}
