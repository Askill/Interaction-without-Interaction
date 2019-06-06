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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FrontEnd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Client> _clients;
        private List<Cam> _cams;

        public MainWindow()
        {
            InitializeComponent();
            var img = new BitmapImage(new Uri(@"pack://application:,,,/Images/map.png"));
            imgMap.Source = img;
        }

        private void CreateCamera(Point pos, double angle, string stream)
        {
            var cam = new Image
            {
                Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/cam.png")),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(angle),
                Width = 32,
                Height = 32
            };
            cam.MouseDown += Cam_MouseDown;
            cam.Tag = stream;
            cnvMap.Children.Add(cam);
            Canvas.SetLeft(cam, pos.X - cam.Width / 2);
            Canvas.SetTop(cam, pos.Y - cam.Height / 2);
        }

        private void Cam_MouseDown(object sender, MouseButtonEventArgs e)
        {
            new StreamWindow(((Image)sender).Tag.ToString()).ShowDialog();
        }

        private void CreateObject(Point pos)
        {
            var ellipse = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Red
            };

            cnvMap.Children.Add(ellipse);
            Canvas.SetLeft(ellipse, pos.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, pos.Y - ellipse.Height / 2);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cnvMap.Width = imgMap.ActualWidth;
            cnvMap.Height = imgMap.ActualHeight;

            _cams = Task.Run(async () => { return await Communicator.GetCamsAsync(); }).Result;
            _clients = Task.Run(async () => { return await Communicator.GetClientsAsync(); }).Result;

            foreach (var client in _clients)
            {
                CreateObject(new Point(cnvMap.Width * client.X, cnvMap.Height * client.Y));
            }

            foreach (var cam in _cams)
            {
                CreateCamera(new Point(cnvMap.Width * cam.X, cnvMap.Height * cam.Y), cam.Angle, cam.Ip);
                ListBoxItem item = new ListBoxItem();
                item.Tag = cam;
                item.Content = cam.Label;
                item.FontWeight = FontWeights.Bold;
                lstDevices.Items.Add(item);
                foreach (var client in _clients.Where(c => c.Id == cam.Client_Id))
                {
                    ListBoxItem subitem = new ListBoxItem();
                    subitem.Tag = client;
                    subitem.Content = client.Label;
                    lstDevices.Items.Add(subitem);
                }   
            }
        }

        private void LstDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = lstDevices.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                if (selectedItem.Tag is Client client)
                {
                    txtInfo.Text = client.Status.ToString();
                }
                else if (selectedItem.Tag is Cam cam)
                {
                    txtInfo.Text = cam.Status.ToString();
                }
            }
        }
    }
}
