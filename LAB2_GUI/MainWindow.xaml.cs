using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace LAB2_GUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static bool isClient;
        private Client client;
        private Server server;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (rbtnClient.IsChecked == true)
                isClient = true;
            else
                isClient = false;

            if (isClient)
            {
                
                client = new Client();
                client.InitConnection();

            }
            else
            {
                server = new Server();
                server.InitConnection();
            }

        }

        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            if (isClient)
            {
                string text = client.GetMessage();
                
                if (text == "Client leaft the chat")
                {
                    txtBox.Text += "---" + "Client leaft the chat" + "---";
                    client.CloseConnection();
                    btnOff.IsEnabled = false;
                }
                else
                    txtBox.Text += "Client: " + text + "\n";
            }
            else
            {
                string text = server.GetMessage();
                
                if (text == "Client leaft the chat")
                {
                    txtBox.Text += "---" + "Client leaft the chat" + "---";
                    server.CloseConnection();
                    btnOff.IsEnabled = false;
                }
                else
                    txtBox.Text += "Client: " + text + "\n";
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string text = txtBoxInput.Text;
            if (isClient)
            {
                txtBox.Text += "You: " + text + "\n";
                client.SendMessage(text);
            }
            else
            {
                txtBox.Text += "You: " + text + "\n";
                server.SendMessage(text);
            }
        }

        private void btnOff_Click(object sender, RoutedEventArgs e)
        {
            if (isClient)
            {
                client.SendMessage("Client leaft the chat");
                client.CloseConnection();
                btnOff.IsEnabled = false;
            }
            else
            {
                server.SendMessage("Client leaft the chat");
                server.CloseConnection();
                btnOff.IsEnabled = false;
            }
        }
    }

    abstract class TCP_Connection
    {
        abstract public void SendMessage(string outputMessage);
        abstract public string GetMessage();
    }

    class Client : TCP_Connection
    {
        static string ipAdress = "127.0.0.1";
        static int port = 8000;
        public Socket socket;


        override public string GetMessage()
        {
            StringBuilder inputMessage = new StringBuilder();
            int bytesRead = 0;
            byte[] inputData = new byte[256];

            bytesRead = socket.Receive(inputData);
            string text = Encoding.UTF8.GetString(inputData, 0, bytesRead);
            inputMessage.Append(text);

            return inputMessage.ToString();
        }

        override public void SendMessage(string outputMessage)
        {

            byte[] outputData = Encoding.UTF8.GetBytes(outputMessage);
            socket.Send(outputData);
        }

        public void InitConnection()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ipAdress), port);
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint);
        }

        public void CloseConnection()
        {
            socket.Close();
            Console.Read();
        }
    }

    class Server : TCP_Connection 
    {
        static string ipAdress = "127.0.0.1";
        static int port = 8000;
        private Socket socket;
        private Socket clientSocket;

        override public string GetMessage()
        {
            StringBuilder inputMessage = new StringBuilder();
            int bytesRead = 0;
            byte[] inputData = new byte[256];

            bytesRead = clientSocket.Receive(inputData);
            string text = Encoding.UTF8.GetString(inputData, 0, bytesRead);
            inputMessage.Append(text);

            return inputMessage.ToString();
        }

        override public void SendMessage(string outputMessage)
        {

            byte[] outputData = Encoding.UTF8.GetBytes(outputMessage);
            clientSocket.Send(outputData);
        }


        public void CloseConnection()
        {
            clientSocket.Close();
            socket.Close();
        }

        public void InitConnection()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ipAdress), port);
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(ipPoint);

            while (clientSocket == null)
            {
                socket.Listen(1);
                clientSocket = this.socket.Accept();
            }
            
        }
    }
}
