using System;
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
        private string _streamUri;
        private StreamWindow()
        {
            InitializeComponent();
        }

        public StreamWindow(string streamUri) : this()
        {
            _streamUri = streamUri;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _ = SimpleMJPEGDecoder.StartAsync((BitmapImage img) =>
              {
                  imgStream.Dispatcher.Invoke(() => { imgStream.Source = img; });

              },
            _streamUri);
        }
    }
}
