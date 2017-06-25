using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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
        private BackgroundWorker worker;
        private String filepath;

        public MainWindow()
        {
            InitializeComponent();
        }

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
                    P3DView.Items.Clear();
                    itemInfo.Children.Clear();
                    P3DFile = new P3D(this);
                    P3DFile.StatusUpdated += new StatusUpdatedEventHandler(ChangeStatus);
                    filepath = path;
                    if (worker == null)
                    {
                        worker = new BackgroundWorker();
                    }
                    worker.DoWork += workerLoadP3D;
                    worker.RunWorkerAsync();

                }
                else
                {
                    if (Path.GetExtension(path) == ".3ds")
                    {
                        ChangeStatus(new StatusUpdatedEventArguments("Loading 3ds file", 10));
                        P3DView.Items.Clear();
                        itemInfo.Children.Clear();
                        File3DS = LIB3DS.lib3ds_file_open(path);
                        P3DFile = new P3D(this);
                        Dispatcher.Invoke(new Action(() => P3DFile.Load3DS(File3DS)), DispatcherPriority.Normal, null);
                        return;
                    }
                    MessageBox.Show("Format not supported");
                }

            }
        }

        private void workerLoadP3D(object sender, DoWorkEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => P3DFile.LoadP3D(filepath)), DispatcherPriority.Normal, null);
        }

        private void P3DView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //itemInfo.Items.Clear();
            //MessageBox.Show(sender.GetType().ToString());
            //P3DElement element = ((P3DElementView)((TreeViewItem)sender).Header).parent;
            //foreach (string item in element.GetItemInfo()) {
            //	itemInfo.Items.Add(item);
            //}
        }

        private void P3DView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //MessageBox.Show( ((P3DElementView)((TreeViewItem)e.NewValue).Header).content.Content.ToString());
            itemInfo.Children.Clear();
            try
            {
                P3DElement element = ((P3DElementView)((TreeViewItem)e.NewValue).Header).ParentElement;
                foreach (IToolPanel item in element.GetItemInfo())
                {
                    if (item is InputText)
                    {
                        itemInfo.Children.Add((InputText) item);
                    }
                    else
                    {
                        itemInfo.Children.Add((InputBool)item);
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (P3DFile == null)
            {
                MessageBox.Show("No P3D file loaded");
                return;
            }
            SaveFileDialog file = new SaveFileDialog
            {
                Filter = "P3D File (*.p3d)|*.p3d",
                DefaultExt = "p3d"
            };
            bool? result = file.ShowDialog();
            if (result == true)
            {
                string path = file.FileName;
                P3DFile.SaveP3D(path);
            }
        }

        private void P3DView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            P3DElement element = ((P3DElementView)e.Source).ParentElement;
            SelectedElement = element;
            ContextMenu menu = new ContextMenu();
            MenuItem mi;// = new MenuItem();
//            mi.Header = "Edit";
//            mi.Click += P3DView_OnContextMenuItemSelected;
//            menu.Items.Add(mi);
            if (element is LightsChunk)
            {
                mi = new MenuItem();
                mi.Header = "Add Light";
                mi.Click += P3DView_OnContextMenuItemSelected;
                menu.Items.Add(mi);

                mi = new MenuItem();
                mi.Header = "Export chunk";
                mi.Click += P3DView_OnContextMenuItemSelected;
                menu.Items.Add(mi);

                mi = new MenuItem();
                mi.Header = "Import chunk";
                mi.Click += P3DView_OnContextMenuItemSelected;
                menu.Items.Add(mi);
            }

            if (element is Light)
            {
                mi = new MenuItem();
                mi.Header = "Clone light";
                mi.Click += P3DView_OnContextMenuItemSelected;
                menu.Items.Add(mi);

                mi = new MenuItem();
                mi.Header = "Delete light";
                mi.Click += P3DView_OnContextMenuItemSelected;
                menu.Items.Add(mi);
            }
            menu.PlacementTarget = e.Source as P3DElementView;
            menu.IsOpen = true;

            menu.MouseLeftButtonDown += P3DView_OnContextMenuItemSelected;
        }

        private void P3DView_OnContextMenuItemSelected(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(((MenuItem)e.Source).Header.ToString());
            if (((MenuItem) e.Source).Header.ToString().Equals("Add Light"))
            {
                ((LightsChunk)SelectedElement).AddLightFromContextMenu(this);
            }
        }

        public void ChangeStatus(StatusUpdatedEventArguments args)
        {
            StatusMessage.Text = args.Message;
            Progress.Value = args.Value;
            Progress.UpdateLayout();
            UpdateLayout();
        }
    }
}
