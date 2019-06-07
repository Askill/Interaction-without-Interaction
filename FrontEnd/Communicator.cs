using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FrontEnd
{
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
            return $"Id: {Id}, Label: {Label}, Status {Status}\n" +
                   $"ClientID: {Client_Id}, Ip : {Ip}\n" +
                   $"X: {X}  Y: {Y}  Angle: {Angle}";
        }
    }

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
            return $"Id: {Id}, Label: {Label}, Status {Status}\n" +
                   $"Ip : {Ip}\n" +
                   $"X: {X}  Y: {Y}";
        }
    }

    class Communicator
    {
        static private HttpClient client = new HttpClient();

        static Communicator()
        {
            client.BaseAddress = new Uri("http://ui.askill.science:8003/");
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
    }
}
