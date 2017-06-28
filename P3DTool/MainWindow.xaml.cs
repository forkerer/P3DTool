using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using lib3ds.Net;
using Microsoft.Win32;
using P3DTool.DataModels;
using P3DTool.DataModels.DataTypes;
using P3DTool.DataModels.FileStructure;
using P3DTool.Views;

namespace P3DTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public P3D P3DFile;
        public Lib3dsFile File3DS;
        private P3DElement SelectedElement;
        public IToolPanel ActiveToolWindow;
        public ObservableCollection<TreeViewItem> P3DViewItems = new ObservableCollection<TreeViewItem>();
        

        public MainWindow()
        {
            InitializeComponent();
            P3DView.ItemsSource = P3DViewItems;
        }

        /// <summary>
        /// Handles loading of file, starts parser on separate thread 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            bool? result = file.ShowDialog();
            if (result == true)
            {
                string path = file.FileName;
                if (Path.GetExtension(path) == ".p3d")
                {
                    ChangeStatus(new StatusUpdatedEventArguments("Loading p3d file", 10));
                    SelectedElement = null;
                    P3DViewItems.Clear();
                    itemInfo.Children.Clear();
                    P3DFile = new P3D(this);
                    P3DFile.StatusUpdated += new StatusUpdatedEventHandler(ChangeStatus);
                    Task.Run(() => P3DFile.LoadP3D(path));
                }
                else
                {
                    if (Path.GetExtension(path) == ".3ds")
                    {
                        ChangeStatus(new StatusUpdatedEventArguments("Loading 3ds file", 10));
                        P3DViewItems.Clear();
                        itemInfo.Children.Clear();
                        File3DS = LIB3DS.lib3ds_file_open(path);
                        P3DFile = new P3D(this);
                        Task.Run(() => P3DFile.Load3DS(File3DS));
                        return;
                    }
                    MessageBox.Show("Format not supported");
                }

            }
        }

        /// <summary>
        /// Handles displaying selected P3DElement info in the left panel 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void P3DView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //MessageBox.Show( ((P3DElementView)((TreeViewItem)e.NewValue).Header).content.Content.ToString());
            itemInfo.Children.Clear();
            try
            {
                if (e.NewValue as TreeViewItem != null)
                {
                    P3DElement element = ((P3DElementView) ((TreeViewItem) e.NewValue).Header).ParentElement;
                    if (element == null) return;
                    foreach (IToolPanel item in element.GetItemInfo())
                    {
                        if (item is InputText)
                        {
                            itemInfo.Children.Add((InputText) item);
                        }
                        else
                        {
                            itemInfo.Children.Add((InputBool) item);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message + " - //SelectedItemChanged// ");
            }

        }

        /// <summary>
        /// Handles saving of P3DFile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (P3DFile == null)
            {
                MessageBox.Show("No P3D file loaded");
                return;
            }
            SaveFileDialog file = new SaveFileDialog
            {
                Filter = "P3D File (*.p3d)|*.p3d|3ds File (*.3ds)|*.3ds",
                DefaultExt = "p3d"
            };
            bool? result = file.ShowDialog();
            if (result == true)
            {
                string path = file.FileName;
                switch (Path.GetExtension(path).ToLower())
                {
                    case ".p3d":
                        P3DFile.SaveP3D(path);
                        break;
                    case ".3ds":
                        _3dsExporter.ExportP3D(P3DFile, path);
                        break;
                }
            }
        }

        /// <summary>
        /// Handles displaying of context menu options
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void P3DView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            P3DElement element = ((P3DElementView)e.Source).ParentElement;
            SelectedElement = element;
            ContextMenu menu = new ContextMenu();
            MenuItem mi;

            if (element is LightsChunk)
            {
                mi = new MenuItem {Header = "Add Light"};
                mi.Click += P3DView_OnContextMenuItemSelected;
                menu.Items.Add(mi);

                mi = new MenuItem {Header = "Export chunk"};
                mi.Click += P3DView_OnContextMenuItemSelected;
                menu.Items.Add(mi);

                mi = new MenuItem {Header = "Import chunk"};
                mi.Click += P3DView_OnContextMenuItemSelected;
                menu.Items.Add(mi);
            }

            if (element is Light)
            {
                mi = new MenuItem {Header = "Clone light"};
                mi.Click += P3DView_OnContextMenuItemSelected;
                menu.Items.Add(mi);

                mi = new MenuItem {Header = "Delete light"};
                mi.Click += P3DView_OnContextMenuItemSelected;
                menu.Items.Add(mi);
            }
            menu.PlacementTarget = e.Source as P3DElementView;
            menu.IsOpen = true;

            menu.MouseLeftButtonDown += P3DView_OnContextMenuItemSelected;
        }

        private void P3DView_OnContextMenuItemSelected(object sender, RoutedEventArgs e)
        {
            if (((MenuItem) e.Source).Header.ToString().Equals("Add Light"))
            {
                ((LightsChunk)SelectedElement).AddLightFromContextMenu(this);
            }
        }

        /// <summary>
        /// Changes information in the status bar  on the bottom of window
        /// </summary>
        /// <param name="args"></param>
        public void ChangeStatus(StatusUpdatedEventArguments args)
        {
            StatusMessage.Dispatcher.BeginInvoke((Action)(() => StatusMessage.Text = args.Message));
            Progress.Dispatcher.BeginInvoke((Action)(() => Progress.Value = args.Value));
        }
    }
}
