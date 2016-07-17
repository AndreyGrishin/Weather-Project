using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Serialization;
using Weather.ServiceReference;

namespace Weather
{
    public partial class MainWindow : Window
    {
        internal Cities.NewDataSet cn;
        internal GlobalWeatherSoapClient gwsc;
        internal WeatherService.CurrentWeather w;
        internal String weather;
        ObservableCollection<TableInfo> collection = null;
        DateTime currentTime;
        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer timerCurrentTime = new System.Windows.Threading.DispatcherTimer();
        int interval;

        public MainWindow()
        {
            InitializeComponent();

        }

        private void CountryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var rr = cn.Table.Where(m => m.Country == CountryComboBox.SelectedValue.ToString()).Select(c => c.City);
            //TODO Free thread
            //CityComboBox.Items.Clear();
            CityComboBox.ItemsSource = rr.ToArray();
            CountryComboBox.IsEnabled = false;
        }
        private void ExecuteUpdate()
        {
            ThreadStart threadStart = new ThreadStart(UpdateInfo);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, threadStart);
        }
        private void ExecuteUpdateForTimer(object sender, EventArgs e)
        {
            ThreadStart threadStart = new ThreadStart(UpdateInfo);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, threadStart);
        }
        private void UpdateInfo()
        {
            try
            {

                DateTime requestTime = DateTime.Now;
                BasicHttpBinding binding = new BasicHttpBinding();
                EndpointAddress adress = new EndpointAddress("http://www.webservicex.net/globalweather.asmx");
                gwsc = new GlobalWeatherSoapClient(binding, adress);
                weather = gwsc.GetWeather(CityComboBox.SelectedValue.ToString(), CountryComboBox.SelectedValue.ToString());
                if (weather != "Data Not Found")
                {

                    XmlSerializer result = new XmlSerializer(typeof(WeatherService.CurrentWeather));
                    w = (WeatherService.CurrentWeather)result.Deserialize(new StringReader(weather));

#region Change data
                    CitytextBlock.Text = CityComboBox.Text;
                    TemptextBlock.Text = w.Temperature;
                    TemptextBlock_Copy.Text = w.SkyConditions;
                    HumiditytextBlock.Text = $"Humidity: {w.RelativeHumidity}";
                    WindtextBlock.Text = $"Wind: {w.Wind}";
                    VisibilitytextBlock.Text = $"Visability: {w.Visibility}";
                    PressuretextBlock.Text = $"Pressure: {w.Pressure}";

                    StatusTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    StatusTextBlock.Text = $"Status: {w.Status}";

                   
                    switch (w.SkyConditions)
                    {
                        case " clear":
                            {
                                image.Source = new BitmapImage(new Uri("Images/clear.png", UriKind.Relative));
                                break;
                            }
                        case " mostly clear":
                            {
                                image.Source = new BitmapImage(new Uri("Images/clear.png", UriKind.Relative));
                                break;
                            }
                        case " mostly cloudy":
                            {
                                image.Source = new BitmapImage(new Uri("Images/obscured.png", UriKind.Relative));
                                break;
                            }
                        case " overcast":
                            {
                                image.Source = new BitmapImage(new Uri("Images/overcast.png", UriKind.Relative));
                                break;
                            }
                            //TODO: Add the rest of the state
                    }
#endregion

                    collection.Add(new TableInfo()
                    {
                        Updated = requestTime,
                        Location = w.Location,
                        Time = w.Time,
                        Visibility = w.Visibility,
                        Wind = w.Wind,
                        SkyConditions = w.SkyConditions,
                        Temperature = w.Temperature,
                        DewPoint = w.DewPoint,
                        RelativeHumidity = w.RelativeHumidity,
                        Pressure = w.Pressure
                    });
                    dataGrid.ItemsSource = collection;

                }
                else
                {
                    MessageBox.Show("Data Not Found");
                    StatusTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(243, 5, 5));
                    StatusTextBlock.Text = "Status: Data Not Found!";
                }
            }
            catch (Exception)
            {

                MessageBox.Show("Internet connection is not established. Unable to connect to the service.");
            }
        }
        private void CityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Thread threadUpdate = new Thread(ExecuteUpdate);
            threadUpdate.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timerCurrentTime.Tick += new EventHandler(GetTime);
            timerCurrentTime.Start();
            timerCurrentTime.Interval = new TimeSpan(0, 0, 1);

            try
            {
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.MaxReceivedMessageSize = 30000000;

                EndpointAddress adress = new EndpointAddress("http://www.webservicex.net/globalweather.asmx");
                GlobalWeatherSoapClient gwsc = new GlobalWeatherSoapClient(binding, adress);
                var cities = gwsc.GetCitiesByCountry("");
                XmlSerializer result = new XmlSerializer(typeof(Cities.NewDataSet));
                cn = (Cities.NewDataSet)result.Deserialize(new StringReader(cities));
                var contries = cn.Table.Select(m => m.Country).Distinct();
                CountryComboBox.ItemsSource = contries.ToList();
            }
            catch (Exception)
            {
                MessageBox.Show("Internet connection is not established. Unable to connect to the service.");
            }

            if (collection == null)
            {
                collection = new ObservableCollection<TableInfo>();
                dataGrid.ItemsSource = collection;
            }

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void GetTime(object sender, EventArgs e)
        {
            currentTime = DateTime.Now;
            TimerTextBlock.Text = currentTime.ToShortTimeString();
        }

        private void SetTimerButton_Click(object sender, RoutedEventArgs e)
        {
            interval = Convert.ToInt32(TimerComboBox.SelectionBoxItem);
            timer.Tick += new EventHandler(ExecuteUpdateForTimer);
            timer.Start();
            timer.Interval = new TimeSpan(0, interval, 0);
            StateTimertextBlock.Text = "ON";
            StateTimertextBlock.Foreground = new SolidColorBrush(Color.FromRgb(33, 207, 6));
        }
        private void StopTimerButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            StateTimertextBlock.Text = "OFF";
            StateTimertextBlock.Foreground = new SolidColorBrush(Color.FromRgb(206, 93, 87));
        }
    }
}

