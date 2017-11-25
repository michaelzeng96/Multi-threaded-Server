using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;

namespace ConsoleApplication1
{
    class Program
    {
        //Hash table to contain clients
        public static Hashtable clientsList = new Hashtable();

        static void Main(string[] args)
        {
            //Creates a TcpListener object to listen for incoming connections on computer's IP address 
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 80);
            //intialize TcpClient to null
            TcpClient clientSocket = default(TcpClient);
            //keep track of connecting client's IP Address
            string client_IP = "";

            //start server and begin listening 
            serverSocket.Start();
            Console.WriteLine("Chat Server Started ....");

            //create a new thread for my commands to run on
            Thread me = new Thread(RemoveClient);
            me.Start();

            while ((true))
            {
                //accept pending request
                 clientSocket = serverSocket.AcceptTcpClient();

                //record down it's IP Address
                client_IP = ((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString();
                //intialize a buffer
                byte[] bytesFrom = new byte[70000];
                //initialize string to store name of client
                string dataFromClient = null;

                //get byte data from accepted clientSocket
                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                //convert bytes from buffer into string, store in dataFromClient
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("\0"));
                
                //add client into hashtable. string, TcpClient
                clientsList.Add(clientSocket, dataFromClient);

                //broadcast that this client has joined
                broadcast(dataFromClient + " Joined ", dataFromClient, false);
                Console.WriteLine(dataFromClient + " Joined chat room. IP -  " + client_IP);

                //create new handleClient object to handle client's messages that are sent
                handleClinet client = new handleClinet();
                client.startClient(clientSocket, dataFromClient, clientsList);
            }
        }

        public static void RemoveClient()
        {
            string a = "";
            Console.WriteLine("To remove a client, type \"r [cliet_ip]\"");
            while (true)
            {
                a = Console.ReadLine();
                //go through each item in hash table until you find the client ip you want to close
                foreach (DictionaryEntry Item in clientsList)
                {
                    if (a == "r " + (((IPEndPoint)((TcpClient)Item.Key).Client.RemoteEndPoint).Address).ToString()) 
                    {
                        //broadcast on server and to every client what is about happen
                        Console.WriteLine((string)Item.Value + " has been executed from this connection.");
                        Program.broadcast((string)Item.Value+" has been executed from this connection.", (string)Item.Value, false);

                        //remove from hashtable
                        clientsList.Remove((TcpClient)Item.Key);

                        //dispose of tcpClient instance and underlying networkstream connection
                        ((TcpClient)Item.Key).Close();

                    }
                    if (clientsList.Count == 0)
                        break;
                }
            }
            
        }
        //broadcast to every client in hashtable 
        public static void broadcast(string msg, string uName, bool is_from_client)
        {
            //for every connected client
            foreach (DictionaryEntry Item in clientsList)
            {
          
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Key;

                //set up networkstream connection
                NetworkStream broadcastStream = broadcastSocket.GetStream();

                
                Byte[] broadcastBytes = null;

                //flag determines what type of message we will write to the networkstream
                if (is_from_client == true)
                {
                    //message sent from other clients 
                    broadcastBytes = Encoding.ASCII.GetBytes(uName + " says : " + msg);
                }
                else
                {
                    //message to notify everyone that a client has joined
                    broadcastBytes = Encoding.ASCII.GetBytes(msg);
                }

                //write into stream so it transfers back to client
                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                //flush broadcastStream
                broadcastStream.Flush();
            }
        }  //end broadcast function
    }//end Main class


    public class handleClinet
    {
        TcpClient clientSocket;
        string clNo;
        Hashtable clientsList;

        public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            this.clientsList = cList;

            //create new thread to manage client 
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            byte[] bytesFrom = new byte[70000];
            string dataFromClient = null;
            while ((true))
            {
                try
                {
                    //set up stream
                    NetworkStream networkStream = clientSocket.GetStream();
                    //read in data from client into bytesFrom byte buffer
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);

                    //convert to string and assign to datafromClient
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);

                    //clear bytesFrom byte array
                    Array.Clear(bytesFrom, 0, bytesFrom.Length);

                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("\0"));
                    //if networkstream disconnects and datafromClient reads in nothing at first
                    if(dataFromClient=="")
                    {
                        Console.WriteLine(clNo + " has disconnected. Ip Address - " + ((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString());
                        clientsList.Remove(clientSocket);
                        dataFromClient = clNo+" has disconnected.";
                        Program.broadcast(dataFromClient, clNo, false); 
                        break;
                    }
                        
                    //print onto server's console the message from the client
                    Console.WriteLine("From client - " + clNo + " : " + dataFromClient);
                    //broadcoast to all other clients the message
                    Program.broadcast(dataFromClient, clNo, true);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                    break;
                }
            }
        }
    }
}