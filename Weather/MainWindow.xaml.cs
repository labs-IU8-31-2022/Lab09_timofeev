using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
using WeatherBot;
using System.Globalization;
using System.Threading;
using System.Windows.Markup;


namespace Weather
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-IN");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-IN");
            LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            Cities = new List<City>();
            DataContext = this;
            InitializeCityList();
        }

        public List<City> Cities { get; set; }

        public City SelectedCity { get; set; }


        private async void InitializeCityList()
        {
            var path = $"{Environment.CurrentDirectory}/../../../Resources/city.txt";
            var fileText = await File.ReadAllTextAsync(path);
            foreach (var line in fileText.Split('\n'))
            {
                var city = string.Concat(line.TakeWhile(c => c is < '0' or > '9')).Trim('-').Trim();
                var lineT = line.Replace(city, string.Empty);
                var data = string.Concat(lineT.Where(c => c is >= '0' and <= '9' or ',' or '.' or '-')).Split(',');
                var (lat, lon) = (Convert.ToDecimal(data[0]), Convert.ToDecimal(data[1]));
                //var (lat, lon) = (Convert.ToDecimal(data[0].Replace('.',',')), Convert.ToDecimal(data[1].Replace('.',',')));
                Cities.Add(new City(city, lat, lon));
            }
            //MessageBox.Show("Города получены.");
        }

        private void GetApi(object sender, RoutedEventArgs e)
        {
            if (SelectedCity.Name is null)
            {
                ComboBox1.Text = "Выберите город!!!";
                return;
            }

            Button1.IsEnabled = false;
            Text1.Text = "Получение данных...";
            var res = Task.Run(() => WeatherBot.Weather.GetAsync(SelectedCity.Lat, SelectedCity.Lon).Result);
            res.ContinueWith((t) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Text1.Text = $"Country: {res.Result.Country}   City: {res.Result.Name} " +
                                 $"\nWeather: {res.Result.Description} \nTemperature: {res.Result.Temp}°C";
                    Button1.IsEnabled = true;
                });
            });
        }

        private int _count;

        private void Count(object sender, RoutedEventArgs e)
        {
            Text2.Text = $"{++_count} click";
        }

        public struct City
        {
            public string Name { get; }
            public decimal Lat { get; }
            public decimal Lon { get; }

            public City(string city, decimal lat, decimal lon) => (Name, Lat, Lon) = (city, lat, lon);
        }
    }
}