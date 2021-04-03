using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
                
                client = new Client(txtBox, btnOff);
                client.InitConnection();
                

            }
            else
            {
                server = new Server(txtBox, btnOff);
                server.InitConnection();
              
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
                txtBox.Text += "---You leaft the chat---";
                client.SendMessage("Client leaft the chat");
                client.CloseThread();
                client.CloseConnection();
                
                btnOff.IsEnabled = false;
            }
            else
            {
                txtBox.Text += "---You leaft the chat---";
                server.SendMessage("Client leaft the chat");
                server.CloseThread();

                server.CloseConnection();
                btnOff.IsEnabled = false;
            }
        }
    }

   

    abstract class TCP_Connection
    {
        abstract public void SendMessage(string outputMessage);
        abstract public void GetMessages();
    }

    class Client : TCP_Connection
    {
        static string ipAdress = "127.0.0.1";
        static int port = 8000;
        public Socket socket;
        private TextBox textBox1;
        private Button button;
        private Thread clientThread;


        public Client(TextBox textBox, Button button)
        {
            this.textBox1 = textBox;
            this.button = button;
        }

        public void CloseThread()
        {
            clientThread.Abort();
        }


        override public void GetMessages()
        {
            while (true)
            {
                StringBuilder inputMessage = new StringBuilder();
                int bytesRead = 0;
                byte[] inputData = new byte[256];

                bytesRead = socket.Receive(inputData);
                string text = Encoding.UTF8.GetString(inputData, 0, bytesRead);
                inputMessage.Append(text);


                if (inputMessage.ToString() == "Client leaft the chat")
                {
                    textBox1.Dispatcher.BeginInvoke(new Action(() => textBox1.Text += "---" + inputMessage.ToString() + "---"));
                    button.Dispatcher.BeginInvoke(new Action(() => button.IsEnabled = false));
                    CloseThread();
                    
                }
                else
                {
                    textBox1.Dispatcher.BeginInvoke(new Action(() => textBox1.Text += "Client: " + inputMessage.ToString() + "\n"));
                }

            }
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

            //Thread clientThread = new Thread(new ThreadStart(GetMessages));
            this.clientThread = new Thread(new ThreadStart(GetMessages));
            this.clientThread.Start();
        }

        public void CloseConnection()
        {
            socket.Close();
            Console.Read();
        }
    }

    class Server
    {
        static string ipAdress = "127.0.0.1";
        static int port = 8000;
        private Socket socket;
        
        private TextBox textBox1;
        private Button button;
        private Thread serverThread;
        private Socket buffSocket;
        private List<Socket> listSockets = new List<Socket>();

        public Server(TextBox textBox, Button button)
        {
            this.textBox1 = textBox;
            this.button = button;
        }

        public void CloseThread()
        {
            serverThread.Abort();
        }

        public void GetMessages(Socket clientSocket)
        {
            while (true)
            {
                StringBuilder inputMessage = new StringBuilder();
                int bytesRead = 0;
                byte[] inputData = new byte[256];


                    bytesRead = clientSocket.Receive(inputData);
                    string text = Encoding.UTF8.GetString(inputData, 0, bytesRead);
                    inputMessage.Append(text);

                    if (inputMessage.ToString() == "Client leaft the chat")
                    {
                        textBox1.Dispatcher.BeginInvoke(new Action(() => textBox1.Text += "---" + inputMessage.ToString() + "---"));
                        button.Dispatcher.BeginInvoke(new Action(() => button.IsEnabled = false));
                        CloseThread();
                    }
                    else
                    {
                        SendMessageToAll(inputMessage.ToString(), clientSocket);
                        textBox1.Dispatcher.BeginInvoke(new Action(() => textBox1.Text += "Client: " + inputMessage.ToString() + "\n"));
                    }
                
            }
        }


        public void SendMessageToAll(string outputMessage, Socket clientSocket)
        {
            foreach (Socket someSoket in listSockets)
            {
                if (clientSocket != someSoket)
                {
                    byte[] outputData = Encoding.UTF8.GetBytes(outputMessage);
                    someSoket.Send(outputData);
                }
            }


        }

        public void SendMessage(string outputMessage)
        {

            foreach (Socket sock in listSockets)
            {
                byte[] outputData = Encoding.UTF8.GetBytes(outputMessage);
                sock.Send(outputData);

            }
        }


        public void CloseConnection()
        {
            foreach (Socket sock in listSockets)
            {
                sock.Close();
                socket.Close();
            }
        }

        public void InitConnection()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ipAdress), port);
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(ipPoint);
            this.socket.Listen(3);


                Task.Run(lstn);

                


                


                //this.serverThread = new Thread(new ThreadStart(GetMessages));
                //this.serverThread.Start();
           
            
        }

        public void lstn()
        {
            while (true)
            {
                Socket clientSocket = this.socket.Accept();
                this.listSockets.Add(clientSocket);


                Task.Run(() => GetMessages(clientSocket));

                
            }
        }



    }
}
