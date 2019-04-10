using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rack
{
    public partial class CqcRack
    {

        public void StartShieldBoxServer()
        {
            if (_phoneServerThread == null)
            {
                _phoneServerThread = new Thread(PhoneServer)
                {
                    IsBackground = true
                };
            }

            if (_phoneServerThread.IsAlive == false)
            {
                _phoneServerThread.Start();
            }
        }

        private void Delay(int millisec)
        {
            Thread.Sleep(millisec);
        }

        private void PhoneServer()
        {
            while (true)
            {

                Delay(50);
            }
        }

        public void StopPhoneServer()
        {
            if (_phoneServerThread != null)
            {
                _phoneServerThread.Abort();
                _phoneServerThread.Join();
            }
        }
    }
}
