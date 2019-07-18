using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FrontEnd
{
    /// <summary>
    /// Class to represent the JSON cam object returned by the central web service.
    /// </summary>
    public class Cam
    {
        public int Id { get; set; }
        public string Ip { get; set; }
        public string Label { get; set; }
        public bool Status { get; set; }
        public int Client_Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }

        override public string ToString()
        {
            return $"Id: {Id}, Label: {Label}, Status: {Status}\n" +
                   $"ClientID: {Client_Id}, Ip : {Ip}\n" +
                   $"X: {X}  Y: {Y}  Angle: {Angle}";
        }
    }

    /// <summary>
    /// Class to represent the JSON client object returned by the central web service.
    /// </summary>
    public class Client
    {
        public int Id { get; set; }
        public string Ip { get; set; }
        public string Label { get; set; }
        public bool Status { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        override public string ToString()
        {
            return $"Id: {Id}, Label: {Label}, Status: {Status}\n" +
                   $"Ip : {Ip}\n" +
                   $"X: {X}  Y: {Y}";
        }
    }

    /// <summary>
    /// Class to communicate with the central web service:
    /// <list type="bullet">
    /// <item><description>retrieves and deserializes JSON objects to <seealso cref="Client"/> and <seealso cref="Cam"/> objects,</description></item>
    /// <item><description>retrieves images</description></item>
    /// </list>
    /// </summary>
    class Communicator
    {
        static private HttpClient client = new HttpClient();

        static Communicator()
        {
            client.BaseAddress = new Uri("http://ui.askill.science:5000/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<List<Cam>> GetCamsAsync()
        {
            List<Cam> cams = null;
            HttpResponseMessage response = await client.GetAsync("cam/");
            if (response.IsSuccessStatusCode)
            {
                cams = await response.Content.ReadAsAsync<List<Cam>>();
            }
            return cams;
        }

        public static async Task<List<Client>> GetClientsAsync()
        {
            List<Client> clients = null;
            HttpResponseMessage response = await client.GetAsync("client/");
            if (response.IsSuccessStatusCode)
            {
                clients = await response.Content.ReadAsAsync<List<Client>>();
            }
            return clients;
        }

        public static async Task<BitmapImage> GetProcessedCameraImage(Cam cam)
        {
            BitmapImage img = null;
            HttpResponseMessage response = await client.GetAsync($"cam/{cam.Id}/processed");
            if (response.IsSuccessStatusCode)
            {
                byte[] imgData = await response.Content.ReadAsByteArrayAsync();
                img = BytesToImage(imgData);
            }
            return img;
        }

        /// <summary>
        /// Returns the url to the processed camera stream.
        /// </summary>
        public static string GetProcessedCameraAddress(Cam cam)
        {
            return $"{client.BaseAddress.AbsoluteUri}cam/{cam.Id}/processed";
        }

        /// <summary>
        /// Converts a byte array to a bitmap image.
        /// </summary>
        /// <param name="array">byte array representing an image</param>
        /// <returns>converted Image</returns>
        public static BitmapImage BytesToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }
    }
}
