﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using P3DTool.DataModels.DataTypes;

namespace P3DTool.Views
{
    /// <summary>
    /// Interaction logic for P3DElementView.xaml
    /// </summary>
    public partial class P3DElementView : UserControl
    {
        public P3DElement ParentElement { get; set; }

        public P3DElementView(P3DElement parent, string content)
        {
            InitializeComponent();
            ParentElement = parent;
            this.content.Text = content;
        }

        public P3DElementView(P3DElement parent, string content, Bitmap icon)
        {
            InitializeComponent();
            ParentElement = parent;
            this.content.Text = content;
            this.icon.Source = ImageSourceForBitmap(icon);
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        public ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }
    }
}
