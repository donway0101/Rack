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

    public enum Info
    {
        TesterRequestRackState = 20001,
        TesterRequestBoxState = 20002,
        TesterSendTestResult = 20003,
        UserCloseProgram = 20004,
        SystemStartOK = 20005,
        SystemStarting = 20006,
        RobotHoming = 20007,
        RobotHomeComplete = 20008,
        StartProduction = 20009,
        PauseProduction = 20010,
        BoxTest = 20011,
        BoxTestComplete = 20012,
        //FoundPhoneToBeServed = 20013,
        // = 20014,
        // = 20015,
        // = 20016,
        // = 20017,
        // = 20018,
        // = 20019,
        // = 20020,
        // = 20021,
        // = 20022,
        // = 20023,
        // = 20024,
        // = 20025,
        // = 20026,
        // = 20027,
        // = 20028,
        // = 20029,
        // = 20030,
        // = 20031,
        // = 20032,
    }

    public enum Error
    {

        OpenBoxFail = 40001,

        PhoneLost = 40023,
    }
}
