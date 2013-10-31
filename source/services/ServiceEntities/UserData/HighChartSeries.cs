using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTimer.ServiceEntities.UserData
{
    public class HighChartSeries
    {
        public string Name { get; set; }
        public IEnumerable<HighChartResult> Data { get; set; }
        public string Color { get; set; }
    }
}
