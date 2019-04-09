using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rack
{
    public partial class CqcRack
    {
        public void Test()
        {
            ReadyThePhone();
            Pick(GripperStepper.Gripper.One);
            UnloadAndLoad(Motion.Holder4, GripperStepper.Gripper.Two);
            UnloadAndLoad(Motion.Holder1, GripperStepper.Gripper.One);
            Place(GripperStepper.Gripper.One);
        }

        private void TestLoadGold()
        {
            Task.Run(() =>
            {
                Load(GripperStepper.Gripper.One, Motion.Gold1);
                Load(GripperStepper.Gripper.One, Motion.Holder1);
                Load(GripperStepper.Gripper.One, Motion.Gold2);
                Load(GripperStepper.Gripper.One, Motion.Holder2);
                Load(GripperStepper.Gripper.One, Motion.Gold3);
                Load(GripperStepper.Gripper.One, Motion.Holder3);
                Load(GripperStepper.Gripper.One, Motion.Gold4);
                Load(GripperStepper.Gripper.One, Motion.Holder4);
                Load(GripperStepper.Gripper.One, Motion.Gold5);
                Load(GripperStepper.Gripper.One, Motion.Holder5);
                Load(GripperStepper.Gripper.One, Motion.Gold1);
                Load(GripperStepper.Gripper.One, Motion.Holder6);
                Load(GripperStepper.Gripper.One, Motion.HomePosition);
            });
        }

        private void TestUnloadAndBin()
        {
            Task.Run(() =>
            {
                SetSpeed(20);
                Unload(GripperStepper.Gripper.Two, Motion.Holder1);
                Unload(GripperStepper.Gripper.Two, Motion.BinPosition);
                Unload(GripperStepper.Gripper.Two, Motion.Holder2);
                Unload(GripperStepper.Gripper.Two, Motion.BinPosition);
                Unload(GripperStepper.Gripper.Two, Motion.Holder3);
                Unload(GripperStepper.Gripper.Two, Motion.BinPosition);
                Unload(GripperStepper.Gripper.Two, Motion.Holder4);
                Unload(GripperStepper.Gripper.Two, Motion.BinPosition);
                Unload(GripperStepper.Gripper.Two, Motion.Holder5);
                Unload(GripperStepper.Gripper.Two, Motion.BinPosition);
                Unload(GripperStepper.Gripper.Two, Motion.Holder6);
                Unload(GripperStepper.Gripper.Two, Motion.BinPosition);
                Unload(GripperStepper.Gripper.Two, Motion.HomePosition);
            });
        }

        private void TestUnloadAndLoadHolders()
        {
            Task.Run(() =>
            {
                SetSpeed(20);
                UnloadAndLoad(Motion.PickPosition, GripperStepper.Gripper.One);
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
