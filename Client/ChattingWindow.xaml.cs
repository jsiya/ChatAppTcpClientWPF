using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for ChattingWindow.xaml
    /// </summary>
    public partial class ChattingWindow : Window
    {
        public TcpClient Client { get; set; }
        public ChattingWindow(string username)
        {
            InitializeComponent();
            AddUser(username);
            DataContext = this;

            Task.Run(() => ReceiveMessages());
        }
        private void AddUser(string username)
        {
            var ip = IPAddress.Parse("10.2.24.38");
            var port = 27001;


            Client = new TcpClient(ip.ToString(), port);
            

            var binaryWriter = new BinaryWriter(Client.GetStream());
            binaryWriter.Write(username);


        }
        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var binaryWriter = new BinaryWriter(Client.GetStream()))
            {
                binaryWriter.Write(MsgTxtBox.Text);
            }

        }

        private async void MsgTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(MsgTxtBox.Text.Length > 0) SendBtn.IsEnabled = true;
            else SendBtn.IsEnabled = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Client.Close();
        }

        private async void ReceiveMessages()
        {
            try
            {
                var binaryReader = new BinaryReader(Client.GetStream());
                while (true)
                {
                    string receivedMessage = await Task.Run(() => binaryReader.ReadString());
                    MsgTextBlock.Dispatcher.Invoke(() => { MsgTextBlock.Text += receivedMessage; });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
