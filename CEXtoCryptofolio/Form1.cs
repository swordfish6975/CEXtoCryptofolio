using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace CEXtoCryptofolio
{
    public partial class Form1 : Form
    {

        private static readonly Encoding encoding = Encoding.UTF8; 

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            var temp = CEXtoCryptofolio.Properties.Settings.Default;
            tbxUUID.Text = temp.UUID;
            tbxSecret.Text = temp.Secret;
            tbxKey.Text = temp.Key;
            tbxUserName.Text = temp.Username;


        }

        public  string downloadData()
        {
            try
            {
                TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                string nonce = ((int)t.TotalSeconds).ToString();

                var temp = CEXtoCryptofolio.Properties.Settings.Default;

                string key = temp.Key;
                string secret = temp.Secret;
                string username = temp.Username;

                string signature = hashIT(secret, nonce + username + key);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("https://cex.io/api/balance/");


                //t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                //nonce = t.TotalSeconds.ToString();

                Uri myUri = new Uri(sb.ToString());
                // Create a 'HttpWebRequest' object for the specified url. 
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(myUri);
                myHttpWebRequest.Timeout = 50000;  //  1/2 the default 
                // Set the user agent as if we were a web browser
                myHttpWebRequest.UserAgent = @"Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.0.4) Gecko/20060508 Firefox/1.5.0.4";

                myHttpWebRequest.Method = "POST";
                myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
                using (var writer = new StreamWriter(myHttpWebRequest.GetRequestStream()))
                {
                    writer.Write("key=" + key + "&signature=" + signature + "&nonce=" + nonce);
                }

                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                var stream = myHttpWebResponse.GetResponseStream();
                var reader = new StreamReader(stream);
                var html = reader.ReadToEnd();
                // Release resources of response object.
                myHttpWebResponse.Close();

                //html = html.Substring(html.IndexOf("last") + 6);
                //html = html.Split(',')[0];

                return html;
            }
            catch
            {
                return "ERROR";
            }


        }

        static string hashIT(string key,string message)
        {
            var keyByte = encoding.GetBytes(key);
            var hmacsha256 = new HMACSHA256(keyByte);
            var messageBytes = encoding.GetBytes(message);
            hmacsha256.ComputeHash(messageBytes);

            return ByteToString(hmacsha256.Hash);
        }

        static string ByteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
                sbinary += buff[i].ToString("X2"); /* hex format */
            return sbinary;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://cryptofolio.info");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var temp = CEXtoCryptofolio.Properties.Settings.Default;
            temp.UUID = tbxUUID.Text;
            temp.Secret = tbxSecret.Text;
            temp.Key = tbxKey.Text;
            temp.Username = tbxUserName.Text;
            temp.Save();


            string UUID = temp.UUID;

            string data = downloadData();
           // MessageBox.Show(data);

            dynamic dynamicJson = JsonConvert.DeserializeObject(data);
            // MessageBox.Show(temp.timestamp.ToString());

            Dictionary<string, string> coins = new Dictionary<string, string>();

            if(dynamicJson.LTC.orders == null)
                coins.Add("LTC", dynamicJson.LTC.available.ToString());
            else
                coins.Add("LTC", (((decimal)dynamicJson.LTC.available) + ((decimal)dynamicJson.LTC.orders)).ToString());

            if (dynamicJson.BTC.orders == null)
                coins.Add("BTC", dynamicJson.BTC.available.ToString());
            else
                coins.Add("BTC", (((decimal)dynamicJson.BTC.available) + ((decimal)dynamicJson.BTC.orders)).ToString());

            if (dynamicJson.NMC.orders == null)
                coins.Add("NMC", dynamicJson.NMC.available.ToString());
            else
                coins.Add("NMC", (((decimal)dynamicJson.NMC.available) + ((decimal)dynamicJson.NMC.orders)).ToString());

            if (dynamicJson.IXC.orders == null)
                coins.Add("IXC", dynamicJson.IXC.available.ToString());
            else
                coins.Add("IXC", (((decimal)dynamicJson.IXC.available) + ((decimal)dynamicJson.IXC.orders)).ToString());

            if (dynamicJson.DVC.orders == null)
                coins.Add("DVC", dynamicJson.DVC.available.ToString());
            else
                coins.Add("DVC", (((decimal)dynamicJson.DVC.available) + ((decimal)dynamicJson.DVC.orders)).ToString());

            if (dynamicJson.GHS.orders == null)
                coins.Add("GHS", dynamicJson.GHS.available.ToString());
            else
                coins.Add("GHS", (((decimal)dynamicJson.GHS.available) + ((decimal)dynamicJson.GHS.orders)).ToString());


            foreach (var coin in coins)
            {

                string URL = "http://cryptofolio.info/api/v1/?@UUID=" + UUID + "&@VALUE=@UpdateQuantity&@CODE=" + coin.Key + "&@Quantity=" + coin.Value;
                WebRequest webRequest = WebRequest.Create(URL);
                WebResponse webResp = webRequest.GetResponse();
                var stream = webResp.GetResponseStream();
                var reader = new StreamReader(stream);
                var html = reader.ReadToEnd();

                if (html != "Success")
                    MessageBox.Show("Failed to upload Coin:" + coin.Key + " Value:" + coin.Value);

                webResp.Close();
            }

            MessageBox.Show("Done");
        }    
    }
}
