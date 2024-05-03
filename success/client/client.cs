using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace camtest
{
    class Spike
    {
        public async Task Send_Cshap(string filepath)
        {
            var file_path = filepath;
            //var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            TcpClient clnt = new TcpClient("127.0.0.1", 12345);

            var file = new FileInfo(file_path);
            if (file.Exists) //파일 존재여부
            {
                FileStream stream = new FileStream(file_path, FileMode.Open, FileAccess.Read);
                NetworkStream ns = clnt.GetStream();
                //Console.WriteLine($"경로는:{file_path}");

                //파일 이름 버퍼
                byte[] file_name = Encoding.UTF8.GetBytes(Path.GetFileName(file_path));
                //파일 이름 사이즈 버퍼
                byte[] file_name_size = BitConverter.GetBytes(file_name.Length);

                //파일이름사이즈 보내기
                await ns.WriteAsync(file_name_size, 0, file_name_size.Length);
                int name_size = BitConverter.ToInt32(file_name_size, 0);
                Console.WriteLine($"파일이름사이즈 전송:{name_size}");
                Thread.Sleep(1000);

                //파일 이름 전송
                await ns.WriteAsync(file_name, 0, file_name.Length);
                string path = Encoding.Default.GetString(file_name);
                Console.WriteLine($"파일이름 전송:{path}");
                Thread.Sleep(1000);

                //파일크기 
                byte[] sizeByte = BitConverter.GetBytes(stream.Length);
                await ns.WriteAsync(sizeByte, 0, sizeByte.Length);
                Console.WriteLine($"파일크기전송:{stream.Length.ToString()}");
                Thread.Sleep(1000);

                //파일 전송
                byte[] data = new byte[stream.Length];
                stream.Read(data);
                await ns.WriteAsync(data, 0, data.Length);
                Console.WriteLine("파일전송");

                byte[] msg = new byte[1024];
                ns.Read(msg, 0, msg.Length);
                string message = Encoding.Default.GetString(msg, 0, msg.Length);
                Console.WriteLine(message);
                MessageBox.Show(message);
            }
            else
            {
                Console.WriteLine("The file is not exists. - " + file_path);
            }
        }
    }
    //class Program
    //{
    //    static async Task Main()
    //    {
    //        Spike spike = new Spike();
    //        spike.Send_Cshap();
    //    }
    //}
}

