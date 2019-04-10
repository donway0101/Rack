using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rack
{
    public partial class CqcRack
    {
        private void PhoneServer()
        {
            while (true)
            {
                try
                {
                    //phone in service
                    Phone phoneSome;
                    lock (_phoneToBeServedLocker)
                    {
                        foreach (var phone in PhoneToBeServed)
                        {

                        }
                    }
                  

                    //Phone to be served


                }
                catch (Exception e)
                {
                    OnErrorOccured(e.Message);
                }

                Delay(50);
            }
        }

        private void AddPhoneToBeServed(Phone phone)
        {
            lock (_phoneToBeServedLocker)
            {
                PhoneToBeServed.Add(phone);
            }
        }

        private void RemovePhoneToBeServed(Phone phone)
        {
            lock (_phoneToBeServedLocker)
            {
                PhoneToBeServed.Add(phone);
            }
        }

        public void StartPhoneServer()
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
