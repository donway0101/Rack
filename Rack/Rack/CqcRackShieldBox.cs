using System.Threading;

namespace Rack
{
    public partial class CqcRack
    {
 
        public void ShieldBoxSetup()
        {
            ShieldBox1 = new BpShieldBox(1, "COM3");

            ShieldBoxs = new BpShieldBox[1]{ShieldBox1};
        }

        public void ShieldBoxInitialization()
        {
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled)
                {
                    box.OpenBox();
                }
            }
        }

        public void StartShieldBoxServer()
        {
            if (_shieldBoxServerThread == null)
            {
                _shieldBoxServerThread = new Thread(ShieldBoxServer)
                {
                    IsBackground = true
                };
            }

            if (_shieldBoxServerThread.IsAlive == false)
            {
                _shieldBoxServerThread.Start();
            }
        }

        private void Delay(int millisec)
        {
            Thread.Sleep(millisec);
        }

        private void ShieldBoxServer()
        {
            while (true)
            {

                Delay(50);
            }
        }

        public void StopShieldBoxServer()
        {
            if (_shieldBoxServerThread != null)
            {
                _shieldBoxServerThread.Abort();
                _shieldBoxServerThread.Join();
            }
        }

    }
}
