using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using MySql.Data.MySqlClient;
using camtest;

namespace tstcam
{
    /// <summary>
    /// Window2.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
            DB_load();
        }

        private void DB_load()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection("Server=localhost;Port=3306;Database=data;Uid=root;Pwd=1208"))
                {
                    connection.Open();
                    string query = "SELECT date, hms, message, path FROM result";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter data = new MySqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    data.Fill(dt);
                    datagrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void image_load(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView selectedRow = (DataRowView)datagrid.SelectedItem;
                string imagePath = selectedRow["path"].ToString(); // 이미지 경로 컬럼의 이름을 변경해야 할 수도 있습니다.
                image.Source = new BitmapImage(new Uri(imagePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window1 win = new Window1();
            win.Show();
            Hide();
        }
    }
}
