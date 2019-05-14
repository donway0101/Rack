using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rack;

namespace Rack
{
    public struct CqcRackError
    {
        public int Code { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
    }

    public partial class CqcRack
    {
        CqcRackError TestTimeoutError = new CqcRackError()
            { Code = 40011, Description = "Phone test timeout" };



    }
}
