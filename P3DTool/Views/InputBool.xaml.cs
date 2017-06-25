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
        }
    }
}
