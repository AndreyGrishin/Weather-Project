using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weather
{
    class TableInfo
    {
        public DateTime Updated { get; set; }
        public string Location { get; set;}
        public string Time { get; set; }
        public string Visibility { get; set; }
        public string Wind { get; set; }
        public string SkyConditions { get; set; }
        public string Temperature { get; set; }
        public string DewPoint { get; set; }
        public string RelativeHumidity { get; set; }
        public string Pressure { get; set; }

        //public TableInfo(DateTime requestTime, string Location, string Time, string Visibility, string SkyConditions, string Temperature, string DewPoint, string RelativeHumidity, string Pressure)
        //{
        //    this.requestTime = requestTime;
        //    this.Location = Location;
        //    this.Time = Time;
        //    this.Visibility = Visibility;
        //    this.SkyConditions = SkyConditions;
        //    this.Temperature = Temperature;
        //    this.DewPoint = DewPoint;
        //    this.RelativeHumidity = RelativeHumidity;
        //    this.Pressure = Pressure;
        //}
    }
}
