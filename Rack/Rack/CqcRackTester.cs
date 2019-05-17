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
            if (_testerInstanced == false)
            {
                Tester1 = new Tester(1,"192.168.8.100", 1001);
                Tester2 = new Tester(2,"192.168.8.100", 1002);
                Tester3 = new Tester(3,"192.168.8.100", 1003);
                Tester4 = new Tester(4,"192.168.8.100", 1004);
                Tester5 = new Tester(5,"192.168.8.100", 1005);
                Tester6 = new Tester(6,"192.168.8.100", 1006);

                Testers = new Tester[6] { Tester1, Tester2, Tester3, Tester4, Tester5, Tester6 };

                foreach (var tester in Testers)
                {
                    tester.ErrorOccured -= Tester_ErrorOccured;
                    tester.ErrorOccured += Tester_ErrorOccured;

                    tester.InfoOccured -= Tester_InfoOccured;
                    tester.InfoOccured += Tester_InfoOccured;
                }

                _testerInstanced = true;
            }

            for (int i = 0; i < Testers.Length; i++)
            {
                Testers[i].ShieldBox = ShieldBoxs[i];
                Testers[i].Start();
            }
        }

        private void Tester_InfoOccured(object sender, int code, string description)
        {
            OnInfoOccured(code, description);
        }

        private void Tester_ErrorOccured(object sender, int code, string description)
        {
            OnErrorOccured(code, description);
        }
    }
}
