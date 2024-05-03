using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using MySql.Data.MySqlClient;

namespace recv_serv
{
    class pinpong
    {
        private string result = "";
        private TcpListener listener;
        private List<TcpClient> clntlist;
        private string file_name;
        private string file_root = @"C:\Users\user\Desktop\save\CShap";


        public void Recv_Cshap(TcpClient clnt)
        {
            NetworkStream re_ns = clnt.GetStream();

            //파일이름사이즈 받기
            byte[] file_name_size = new byte[4];
            re_ns.Read(file_name_size, 0, file_name_size.Length);
            int name_size = BitConverter.ToInt32(file_name_size, 0);
            Console.WriteLine($"파일 이름 사이즈 수신:{name_size}");
            Console.WriteLine(name_size);


            //파일 이름받기
            byte[] filename = new byte[name_size];
            re_ns.Read(filename, 0, filename.Length);
            string path = Encoding.Default.GetString(filename);
            file_name = path;
            Console.WriteLine(path);
            Console.WriteLine($"파일이름 수신:{path}");

            //파일사이즈받기
            byte[] file_size = new byte[4];
            re_ns.Read(file_size, 0, file_size.Length);
            int filesize = BitConverter.ToInt32(file_size, 0);
            Console.WriteLine($"파일사이즈 수신:{filesize}");
            Console.WriteLine(filesize);

            //파일받기
            byte[] file = new byte[filesize];
            int total = 0;
            while (total < filesize)
            {
                int file_data = re_ns.Read(file, 0, filesize);
                if (file_data == 0)
                {
                    throw new EndOfStreamException("End of stream reached before fully reading file.");
                }
                total += file_data;
            }
            if (!Directory.Exists(file_root))
            {
                Directory.CreateDirectory(file_root);
            }
            string save_point = Path.Combine(file_root, path);
            File.WriteAllBytes(save_point, file);

            if (!File.Exists(path))
            {
                File.Create(path);
            }
                
            Console.WriteLine("파일 수신완료");
            Send_python(clntlist[0], save_point);
            //Task.Run(() => starting(clntlist[0], save_point));
            
        }
        

        public void Send_python(TcpClient client, string filepath)
        {
            string file_path = filepath;
            Console.WriteLine($"넘겨받은 경로:{file_path}");
            FileStream stream = new FileStream(file_path, FileMode.Open, FileAccess.Read);
            NetworkStream ns = client.GetStream();
            //Console.WriteLine($"경로는:{file_path}");

            //파일크기 
            byte[] sizeByte = BitConverter.GetBytes(stream.Length);
            ns.Write(sizeByte, 0, sizeByte.Length);
            Console.WriteLine($"파일크기전송:{stream.Length.ToString()}");

            //파일 전송
            byte[] data = new byte[stream.Length];
            stream.Read(data);
            ns.Write(data, 0, data.Length);
            Console.WriteLine("파일전송완료");
            Thread.Sleep(2000);

            //결과값 받기
            byte[] send = new byte[1024];
            ns.Read(send, 0, send.Length);
            result = Encoding.Default.GetString(send, 0, send.Length).ToString();
            result = result.Trim('\0');
            Console.WriteLine($"딥러닝 결과는:{result}");
            Mysql(result, file_root, file_name);
            Send_msg_Cshap(result);

        }


        public void Send_msg_Cshap(string msg)
        {
            string message = msg;
            NetworkStream ns = clntlist[1].GetStream();
            byte[] send = Encoding.Default.GetBytes(message);
            ns.Write(send, 0, send.Length);
            clntlist.RemoveAt(1);
        }

        public async Task Start()
        {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 12345);
            listener.Start();
            Console.WriteLine("서버 시작");
            Console.WriteLine($"클라 접속 대기");
            while (true)
            {
                TcpClient clnt = await listener.AcceptTcpClientAsync();
                clntlist.Add(clnt);
                Console.WriteLine($"새로운 클라 접속");
                int age = clntlist.Count;
                if (clntlist.Count > 1)
                {
                    Task.Run(() => Recv_Cshap(clntlist[1]));
                }
            }
        }
        public void Mysql(string result, string line, string name)
        {
            using (MySqlConnection connection = new MySqlConnection("Server=localhost;Port=3306;Database=data;Uid=root;Pwd=1208"))
            {
                string msg = result;
                Console.WriteLine($"전달 받은 결과:{msg}");
                string file_path = line + "\\"+name;
                var change = file_path.Replace('\\', '/');
                Console.WriteLine($"변환시킨 루트:{change}");
                var file_name = name;
                var change_name = "";
                string toRemove = ".jpg";
                int i = name.IndexOf(toRemove);
                if (i >= 0)
                {
                    change_name = name.Remove(i, toRemove.Length);
                }
                var name1 = change_name.Substring(0, 8);
                var name2 = change_name.Substring(9, 8);
                byte[] imageBytes = File.ReadAllBytes(file_path);
                string insertQuery = "INSERT INTO result(date,hms,path,message,image) VALUES(@name1, @name2, @change, @msg, @image)";
                //string insertQuery = "INSERT INTO result(date,hms,path,message,image) VALUES('" + name1 + "','" + name2 + "','" + change + "','" + msg + "','" + change + "')";
                Console.WriteLine($"완성시킨 쿼리:{insertQuery}");
                connection.Open();
                MySqlCommand command = new MySqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@name1", name1);
                command.Parameters.AddWithValue("@name2", name2);
                command.Parameters.AddWithValue("@change", change);
                command.Parameters.AddWithValue("@msg", msg);
                command.Parameters.AddWithValue("@image", imageBytes);
                try
                {
                    if (command.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("등록완료");
                    }
                    else
                    {
                        Console.WriteLine("오류발생");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                connection.Close();
                //else if (msg == "false")
                //{
                //    string insertQuery = "INSERT INTO result(date,file_path,result) VALUES('" + name + "','" + change + "','" + msg + "')";
                //    connection.Open();
                //    MySqlCommand command = new MySqlCommand(insertQuery, connection);
                //    try
                //    {
                //        if (command.ExecuteNonQuery() == 1)
                //        {
                //            Console.WriteLine("등록완료");
                //        }
                //        else
                //        {
                //            Console.WriteLine("오류발생");
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine(e.Message);
                //    }
                //    connection.Close();
                //}
            }
            
        }
        public pinpong()
        {
            clntlist = new List<TcpClient>();
        }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            pinpong ping = new pinpong();
            await ping.Start();
        }
    }
}
