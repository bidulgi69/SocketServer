using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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

        int ByteToInt(byte[] bytes, int n)
        {
            return BitConverter.ToInt16(bytes, n);
        }

        void HandleSocket(Socket socket)
        {
            Console.WriteLine("IN THREAD");
            byte[] bytes = new byte[512];
            socket.Receive(bytes);

            Console.WriteLine("Received : " + ByteToInt(bytes, 20) + " / " + ByteToInt(bytes, 22) + " / " + ByteToInt(bytes, 24) + " / " + ByteToInt(bytes, 26));
            SqlInsert(bytes);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        void SqlInsert(byte[] bytes)
        {
            using MySqlConnection connection = new MySqlConnection("Server=");
            string companyName = Regex.Replace(Encoding.ASCII.GetString(bytes, 0, 10), " ", "");
            string manufacturedCode = Regex.Replace(Encoding.ASCII.GetString(bytes, 10, 10), " ", "");
            string protocolKey = companyName + "_" + manufacturedCode;

            Console.WriteLine("manufacturedCode : " + manufacturedCode + " // " + companyName);
            //string query = "update press_spm set value = @value, time = @time where concept = @concept";
            //string query = "insert into press_spm(user, concept, value, statement, time) values (@concept, @concept, @value, 'active', @time)";
            connection.Open();
            string query = String.Format("update press_spm set value = '{0}', time = '{1}' where concept = '{2}'", ByteToInt(bytes, 20), DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"), protocolKey);
            Console.WriteLine("query : " + query);
            MySqlCommand command = new MySqlCommand(query, connection);

            Console.WriteLine(command.CommandText);
            if (command.ExecuteNonQuery() != 1)
            {
                Console.WriteLine("failed.");
            }

            query = "update press_angle set value = @value, time = @time where concept = @concept";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@value", ByteToInt(bytes, 22));
            command.Parameters.AddWithValue("@time", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
            command.Parameters.AddWithValue("@concept", protocolKey);
            command.ExecuteNonQuery();

            query = "update press_main_current set value = @value, time = @time where concept = @concept";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@value", ByteToInt(bytes, 24));
            command.Parameters.AddWithValue("@time", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
            command.Parameters.AddWithValue("@concept", protocolKey);
            command.ExecuteNonQuery();



            // mysql insert query
            //MySqlDataReader table = command.ExecuteReader();
            //while (table.Read())
            //{
            //    Console.WriteLine("{0} {1} {2}", table["concept"], table["statement"], table["time"]);
            //}
            //table.Close();
            connection.Close();
        }

    }
}
