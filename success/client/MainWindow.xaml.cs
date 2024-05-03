using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using OpenCvSharp;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Timers;
using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using tstcam;
using System.Collections.Generic;
using camtest;
namespace comtest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : System.Windows.Window
    {
        VideoCapture video;
        Mat frame;
        DispatcherTimer timer;
        bool is_initCam, is_initTimer;
        private string save;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void windows_loaded(object sender, RoutedEventArgs e)
        {
            // 카메라, 타이머(0.01ms 간격) 초기화
            is_initCam = init_camera();
            is_initTimer = init_Timer(0.001);

            // 초기화 완료면 타이머 실행
            if (is_initTimer && is_initCam) timer.Start();
        }

        private bool init_Timer(double interval_ms)
        {
            try
            {
                timer = new DispatcherTimer();

                timer.Interval = TimeSpan.FromMilliseconds(interval_ms);
                timer.Tick += new EventHandler(timer_tick);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool init_camera()
        {
            try
            {
                // 0번 카메라로 VideoCapture 생성 (카메라가 없으면 안됨)
                video = new VideoCapture(0);
                video.FrameHeight = (int)img_001.Height;
                video.FrameWidth = (int)img_001.Width;

                // 카메라 영상을 담을 Mat 변수 생성
                frame = new Mat();

                return true;
            }
            catch
            {
                return false;
            }
        }
        private void timer_tick(object sender, EventArgs e)
        {
            // 0번 장비로 생성된 VideoCapture 객체에서 frame을 읽어옴
            video.Read(frame);
            //img_001.Source = WriteableBitmapConverter.ToWriteableBitmap(frame);
            img_001.Source = OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap(frame);
        }

        private void ccc_Click(object sender, RoutedEventArgs e)
        {
            Window1 win = new Window1();
            win.Show();
            Hide();

        }

        private async void btn_001_Click(object sender, RoutedEventArgs e)
        {
            string save_year = DateTime.Now.ToString("yyyy");
            //Directory.CreateDirectory(save_year);
            string save_month = DateTime.Now.ToString("MM");
            //Directory.CreateDirectory(save_year+"/"+save_month);
            string save_day = DateTime.Now.ToString("dd");
            Directory.CreateDirectory(save_year + "/" + save_month + "/" + save_day);
            save = save_year + "/" + save_month + "/" + save_day;
            string path = DateTime.Now.ToString("yyyy");
            path += "/" + DateTime.Now.ToString("MM");
            path += "/" + DateTime.Now.ToString("dd");
            path += "/" + DateTime.Now.ToString("yy.MM.dd-HH.mm.ss");
            path += ".jpg";
            //aaa.Content = path;

            await Task.Run(() =>
            {
                Cv2.ImWrite(path, frame);
            });
            await Task.Run(() =>
            {
                Spike a = new Spike();
                a.Send_Cshap(path);
            });


        }
    }
}