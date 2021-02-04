using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ADOp.Ludo.Test.Network
{
	public class TcpClientTest : MonoBehaviour
	{
		private TcpClient socketConnection;
		private Thread clientReceiveThread;
		private string m_IpAddress;
		private int m_Port;

		/// <summary> 	
		/// Setup socket connection. 	
		/// </summary> 	
		public void ConnectToTcpServer(string ipAddress, int port)
		{
			try
			{
				m_IpAddress = ipAddress;
				m_Port = port;
				clientReceiveThread = new Thread(new ThreadStart(ListenForData));
				clientReceiveThread.IsBackground = true;
				clientReceiveThread.Start();
			}
			catch (Exception e)
			{
				Debug.Log("Client start exception: " + e);
			}
		}

		/// <summary> 	
		/// Send message to server using socket connection. 	
		/// </summary> 	
		public void SendMessage()
		{
			if (socketConnection == null)
			{
				return;
			}
			try
			{
				// Get a stream object for writing. 			
				NetworkStream stream = socketConnection.GetStream();
				if (stream.CanWrite)
				{
                    string clientMessage = "This is a message from one of your clients.";
					// Convert string message to byte array.                 
					byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
					// Write byte array to socketConnection stream.                 
					stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
					Debug.Log("Client sent his message - should be received by server");
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("Socket exception, error code: " + socketException.ErrorCode + ", exception: " + socketException);
			}
		}

		public void Close()
        {
			if(socketConnection != null)
            {
				socketConnection.Close();
				socketConnection = null;
			}
        }

		/// <summary> 	
		/// Runs in background clientReceiveThread; Listens for incomming data. 	
		/// </summary>     
		private void ListenForData()
		{
			try
			{
				IPAddress remoteIpAddress = IPAddress.Parse(m_IpAddress);
				IPEndPoint remoteEndPoint = new IPEndPoint(remoteIpAddress, m_Port);
				socketConnection = new TcpClient();
				socketConnection.Connect(remoteEndPoint);
				Debug.Log("Client connected to ip: " + remoteEndPoint.Address.ToString() + ", port: " + remoteEndPoint.Port);
				Byte[] bytes = new Byte[1024];
				while (true)
				{
					// Get a stream object for reading 				
					using (NetworkStream stream = socketConnection.GetStream())
					{
						int length;
						// Read incomming stream into byte arrary. 					
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);
							// Convert byte array to string message. 						
							string serverMessage = Encoding.ASCII.GetString(incommingData);
							Debug.Log("server message received as: " + serverMessage);
						}
					}
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("Socket exception, error code: " + socketException.ErrorCode + ", exception: " + socketException);
			}
		}
	}
}
