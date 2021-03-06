﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FrontEnd
{
    /// <summary>
    /// Interaction logic for StreamWindow.xaml
    /// </summary>
    public partial class StreamWindow : Window
    {
        private Cam _cam = null;
        private bool _processed;
        private StreamWindow()
        {
            InitializeComponent();
        }

        public StreamWindow(Cam cam, bool processed) : this()
        {
            _cam = cam;
            _processed = processed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if we want the original or processed stream.
            string steamAddr = _processed ? Communicator.GetProcessedCameraAddress(_cam) : _cam.Ip;

            // // retrieve single image
            //if (_processed)
            //{
            //    _ = Task.Run(async () =>
            //      {
            //          var img = await Communicator.GetProcessedCameraImage(_cam);
            //          imgStream.Dispatcher.Invoke(() => { imgStream.Source = img; });
            //      });
            //}
            //else

            // Show stream in image control.
            _ = SimpleMJPEGDecoder.StartAsync((BitmapImage img) =>
            {
                imgStream.Dispatcher.Invoke(() => { imgStream.Source = img; });

            }, steamAddr);
        }
    }
}
