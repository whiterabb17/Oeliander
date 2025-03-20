using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OelianderUI.Helpers
{
    class NetcatListener
    {
        private static TcpListener listener;
        private static bool isRunning = true;

        public static bool StartNCat(string[] args)
        {
            try
            {

                // Set the IP address and port for the listener to bind to.
                string ipAddress = "127.0.0.1";  // Localhost (can be changed to any IP)
                int port = 8888;  // Port number (can be changed to any available port)

                // Create a TcpListener to listen on the specified IP address and port.
                listener = new TcpListener(IPAddress.Parse(ipAddress), port);
                listener.Start();

                Objects.ShowAlert("Reverse Shell", $"[*] Listening on {ipAddress}:{port}...", 0);

                // Start a new thread to handle the incoming connections.
                Thread listenerThread = new Thread(new ThreadStart(ListenForClients));
                listenerThread.Start();
                return true;
            }
            catch (Exception ex)
            {
                Objects.ShowAlert("NCat Error", ex.Message, 2);
                return false;
            }
        }

        private static void ListenForClients()
        {
            while (isRunning)
            {
                try
                {
                    // Block until a client connects.
                    TcpClient tcpClient = listener.AcceptTcpClient();
                    Objects.term.AddResult("[*] Client connected.");

                    // Start a new thread to handle the communication with the connected client.
                    Thread clientThread = new Thread(() => HandleClientComm(tcpClient));
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    Objects.ShowAlert("Connection Error", "[!] Error accepting client: " + ex.Message, 2);
                }
            }
        }

        private static void HandleClientComm(TcpClient tcpClient)
        {
            // Get the network stream for reading and writing data.
            NetworkStream networkStream = tcpClient.GetStream();

            // Set up a buffer to read incoming data.
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while (true)
                {
                    // Read data from the client.
                    bytesRead = networkStream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        break;  // Client has disconnected.
                    }

                    // Convert the received byte array to a string.
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Objects.term.AddResult("[*] Received: " + receivedMessage);

                    // Echo the message back to the client.
                    byte[] response = Encoding.UTF8.GetBytes("Server Echo: " + receivedMessage);
                    networkStream.Write(response, 0, response.Length);
                    networkStream.Flush();
                }
            }
            catch (Exception ex)
            {
                Objects.ShowAlert("Communication Error", "[!] Error handling client communication: " + ex.Message, 2);
            }
            finally
            {
                // Close the connection.
                tcpClient.Close();
                Objects.term.AddResult("[*] Client disconnected.");
            }
        }
    }

}
