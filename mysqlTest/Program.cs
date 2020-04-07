using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().ExecuteServer();
        }

        void ExecuteServer()
        {
            string localIPaddr = "192.168.0.36";
            IPAddress ipAddr = IPAddress.Parse(localIPaddr);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 8888);

            Socket listener = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    Console.WriteLine("Waiting connection ... ");
                    Socket clientSocket = listener.Accept();
                    Thread thread = new Thread(() => HandleSocket(clientSocket));
                    thread.Start();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void HandleSocket(Socket socket)
        {
            Console.WriteLine("IN THREAD");
            byte[] bytes = new byte[512];
            socket.Receive(bytes);

            string companyName = Encoding.ASCII.GetString(bytes, 0, 10);
            string maufacturedCode = Encoding.ASCII.GetString(bytes, 0, 20);
            Console.WriteLine("companyName : " + companyName);
            Console.WriteLine("manufacturerCode : " + maufacturedCode);

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        //void SqlInsert()
        //{
        //    using (MySqlConnection connection = new MySqlConnection("Server="))
        //    {
        //        string query = "select * from press_spm";
        //        try
        //        {
        //            connection.Open();
        //            MySqlCommand command = new MySqlCommand(query, connection);
        //            MySqlDataReader table = command.ExecuteReader();

        //            while (table.Read())
        //            {
        //                Console.WriteLine("{0} {1} {2}", table["concept"], table["statement"], table["time"]);
        //            }
        //            table.Close();
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e.ToString());
        //        }
        //    }
        //}
    }
}
