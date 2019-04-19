using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rack;

namespace Rack
{
    public partial class CqcRack
    {
        /// <summary>
        /// 
        /// </summary>
        /// Shield box must set first.
        private void TesterSetup()
        {
            //Todo read setting.
            Tester1 = new Tester("192.168.1.118", 1001);
            Tester2 = new Tester("192.168.1.118", 1002);
            Tester3 = new Tester("192.168.1.118", 1003);
            Tester4 = new Tester("192.168.1.118", 1004);
            Tester5 = new Tester("192.168.1.118", 1005);
            Tester6 = new Tester("192.168.1.118", 1006);

            //Todo the order of box decide priority of box which robot will choose.
            Testers = new Tester[6] { Tester1, Tester2, Tester3, Tester4, Tester5, Tester6 };

            for (int i = 0; i < Testers.Length; i++)
            {
                Testers[i].ShieldBox = ShieldBoxs[i];
                Testers[i].Start();
            }
        }


    }
}
