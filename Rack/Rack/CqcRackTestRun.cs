using System.Threading.Tasks;

namespace Rack
{
    public partial class CqcRack
    {
        public void Test()
        {
            RunShieldBox();
        }

        public void ExchangeBox()
        {
            ReadyThePhone();
            Pick(RackGripper.One);
            UnloadAndLoad(Motion.ShieldBox4, RackGripper.Two);
            UnloadAndLoad(Motion.ShieldBox1, RackGripper.One);
            Place(RackGripper.One);
        }

        public void RunShieldBox()
        {
            ReadyThePhone();
            Pick(RackGripper.One);
            UnloadAndLoad(Motion.ShieldBox4, RackGripper.Two);
            Place(RackGripper.Two);
            ReadyThePhone();
            Pick(RackGripper.Two);
            UnloadAndLoad(Motion.ShieldBox4, RackGripper.One);
            Place(RackGripper.One);
        }

        private void TestLoadGold()
        {
            Task.Run(() =>
            {
                Load(RackGripper.One, Motion.Gold1);
                Load(RackGripper.One, Motion.ShieldBox1);
                Load(RackGripper.One, Motion.Gold2);
                Load(RackGripper.One, Motion.ShieldBox2);
                Load(RackGripper.One, Motion.Gold3);
                Load(RackGripper.One, Motion.ShieldBox3);
                Load(RackGripper.One, Motion.Gold4);
                Load(RackGripper.One, Motion.ShieldBox4);
                Load(RackGripper.One, Motion.Gold5);
                Load(RackGripper.One, Motion.ShieldBox5);
                Load(RackGripper.One, Motion.Gold1);
                Load(RackGripper.One, Motion.ShieldBox6);
                Load(RackGripper.One, Motion.HomePosition);
            });
        }

        private void TestUnloadAndBin()
        {
            Task.Run(() =>
            {
                SetRobotSpeed(20);
                Unload(RackGripper.Two, Motion.ShieldBox1);
                Unload(RackGripper.Two, Motion.BinPosition);
                Unload(RackGripper.Two, Motion.ShieldBox2);
                Unload(RackGripper.Two, Motion.BinPosition);
                Unload(RackGripper.Two, Motion.ShieldBox3);
                Unload(RackGripper.Two, Motion.BinPosition);
                Unload(RackGripper.Two, Motion.ShieldBox4);
                Unload(RackGripper.Two, Motion.BinPosition);
                Unload(RackGripper.Two, Motion.ShieldBox5);
                Unload(RackGripper.Two, Motion.BinPosition);
                Unload(RackGripper.Two, Motion.ShieldBox6);
                Unload(RackGripper.Two, Motion.BinPosition);
                Unload(RackGripper.Two, Motion.HomePosition);
            });
        }

        private void TestUnloadAndLoadHolders()
        {
            Task.Run(() =>
            {
                SetRobotSpeed(20);
                UnloadAndLoad(Motion.PickPosition, RackGripper.One);
                //UnloadAndLoad(_motion.Holder1, Gripper.Two);
                //UnloadAndLoad(_motion.PickPosition, Gripper.One);
                //UnloadAndLoad(_motion.Holder2, Gripper.Two);
                //UnloadAndLoad(_motion.PickPosition, Gripper.One);
                //UnloadAndLoad(_motion.Holder3, Gripper.Two);
                //UnloadAndLoad(_motion.PickPosition, Gripper.One);
                //UnloadAndLoad(_motion.Holder4, Gripper.Two);
                //UnloadAndLoad(_motion.PickPosition, Gripper.One);
                //UnloadAndLoad(_motion.Holder5, Gripper.Two);
                //UnloadAndLoad(_motion.PickPosition, Gripper.One);
                //UnloadAndLoad(_motion.Holder6, Gripper.Two);
            });
        }
    }
}
