using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FrontEnd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Load and set canvas background image.
            var img = new BitmapImage(new Uri(@"pack://application:,,,/Images/map.png"));
            imgMap.Source = img;
        }

        /// <summary>
        /// Creates a camera marker on the map.
        /// </summary>
        private void CreateCameraMarker(Point pos, Cam cam)
        {
            // Represent the client with a custom images.
            // Load the green camera image from the resources when it's active and the red one when not.
            var img = new Image
            {
                Source = new BitmapImage(new Uri($@"pack://application:,,,/Images/cam_{(cam.Status ? "green" : "red")}.png")),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(cam.Angle),
                Width = 32,
                Height = 32,
                Tag = cam,
                ToolTip = cam.Label
            };

            // Set mouse event handlers.
            img.MouseDown += Cam_MouseDown;
            img.MouseEnter += Img_MouseEnter;

            // Place the camera image on the canvas.
            cnvMap.Children.Add(img);
            Canvas.SetLeft(img, pos.X - img.Width / 2);
            Canvas.SetTop(img, pos.Y - img.Height / 2);
        }

        /// <summary>
        /// Select the corresponding list item of the hovered camera marker.
        /// </summary>
        private void Img_MouseEnter(object sender, MouseEventArgs e)
        {
            var cam = (Cam)((Image)sender).Tag;
            var newSelection = from ListBoxItem item in lstDevices.Items
                               where cam.Label.Equals(item.Content)
                               select item;
            if (newSelection.Any())
                lstDevices.SelectedItem = newSelection.First();
        }

        /// <summary>
        /// Show the stream of the hovered camera marker. (LMB = original stream, LMB = processed stream)
        /// </summary>
        private void Cam_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var cam = (Cam)((Image)sender).Tag;
            if (e.LeftButton == MouseButtonState.Pressed)
                new StreamWindow(cam, false).ShowDialog();
            else if (e.RightButton == MouseButtonState.Pressed)
                new StreamWindow(cam, true).ShowDialog();
        }

        /// <summary>
        /// Creates a client marker on the map.
        /// </summary>
        private void CreateClientMarker(Point pos, Client client)
        {
            // Represent the client with a colored ellipse.
            // The color is green when active and red when not.
            var ellipse = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = client.Status ? Brushes.Green : Brushes.Red,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                ToolTip = client.Label,
                Tag = client
            };

            // Set mouse event handlers.
            ellipse.MouseEnter += Ellipse_MouseEnter;

            // Place the ellipse on the canvas.
            cnvMap.Children.Add(ellipse);
            Canvas.SetLeft(ellipse, pos.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, pos.Y - ellipse.Height / 2);
        }
        
        /// <summary>
        /// Select the corresponding list item of the hovered client marker.
        /// </summary>
        private void Ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            var client = (Client)((Ellipse)sender).Tag;
            var newSelection = from ListBoxItem item in lstDevices.Items
                               where client.Label.Equals(item.Content)
                               select item;
            if (newSelection.Any())
                lstDevices.SelectedItem = newSelection.First();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadInformation();

            // Set a timer to periodically reload shown information.
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Get the current selected list item to restore the selection after refresh.
            var selectedItem = (ListBoxItem)lstDevices.SelectedItem;
            LoadInformation();
            if (selectedItem != null)
            {
                var newSelection = from ListBoxItem item in lstDevices.Items
                                   where selectedItem.Content.Equals(item.Content)
                                   select item;
                if (newSelection.Any())
                    lstDevices.SelectedItem = newSelection.First();
            }
        }

        /// <summary>
        /// Populates the canvas map and device list.
        /// </summary>
        private void LoadInformation()
        {
            // Retrieve cam and client objects.
            List<Cam> cams = Task.Run(async () => { return await Communicator.GetCamsAsync(); }).Result;
            List<Client> clients = Task.Run(async () => { return await Communicator.GetClientsAsync(); }).Result;

            // Clear canvas map and properly set it's size.
            cnvMap.Children.Clear();
            cnvMap.Width = imgMap.ActualWidth;
            cnvMap.Height = imgMap.ActualHeight;

            // Also clear the list and description textbox.
            lstDevices.Items.Clear();
            txtInfo.Clear();

            // Create client markers on canvas map.
            foreach (var client in clients)
            {
                CreateClientMarker(new Point(cnvMap.Width * client.X, cnvMap.Height * client.Y), client);
            }

            foreach (var cam in cams)
            {
                // Create a cam marker on the canvas map...
                CreateCameraMarker(new Point(cnvMap.Width * cam.X, cnvMap.Height * cam.Y), cam);

                // ...and create a correspondig list item...
                ListBoxItem item = new ListBoxItem
                {
                    Tag = cam,
                    Content = cam.Label,
                    FontWeight = FontWeights.Bold
                };
                lstDevices.Items.Add(item);

                // ...with associated clients as subitems.
                foreach (var client in clients.Where(c => c.Id == cam.Client_Id))
                {
                    ListBoxItem subitem = new ListBoxItem
                    {
                        Tag = client,
                        Content = client.Label
                    };
                    lstDevices.Items.Add(subitem);
                }
            }
        }

        private void LstDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Show device properties in textbox.
            var selectedItem = lstDevices.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                if (selectedItem.Tag is Client client)
                {
                    txtInfo.Text = client.ToString();
                }
                else if (selectedItem.Tag is Cam cam)
                {
                    txtInfo.Text = cam.ToString();
                }
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Reload information on button click.
            LoadInformation();
        }
    }
}
