using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GripperStepper;

namespace Rack
{
    public partial class CqcRack
    {
        public void Test()
        {
            ReadyThePhone();
            Pick(StepperMotor.One);
            UnloadAndLoad(Motion.Holder4, StepperMotor.Two);
            UnloadAndLoad(Motion.Holder1, StepperMotor.One);
            Place(StepperMotor.One);
        }

        private void TestLoadGold()
        {
            Task.Run(() =>
            {
                Load(StepperMotor.One, Motion.Gold1);
                Load(StepperMotor.One, Motion.Holder1);
                Load(StepperMotor.One, Motion.Gold2);
                Load(StepperMotor.One, Motion.Holder2);
                Load(StepperMotor.One, Motion.Gold3);
                Load(StepperMotor.One, Motion.Holder3);
                Load(StepperMotor.One, Motion.Gold4);
                Load(StepperMotor.One, Motion.Holder4);
                Load(StepperMotor.One, Motion.Gold5);
                Load(StepperMotor.One, Motion.Holder5);
                Load(StepperMotor.One, Motion.Gold1);
                Load(StepperMotor.One, Motion.Holder6);
                Load(StepperMotor.One, Motion.HomePosition);
            });
        }

        private void TestUnloadAndBin()
        {
            Task.Run(() =>
            {
                SetSpeed(20);
                Unload(StepperMotor.Two, Motion.Holder1);
                Unload(StepperMotor.Two, Motion.BinPosition);
                Unload(StepperMotor.Two, Motion.Holder2);
                Unload(StepperMotor.Two, Motion.BinPosition);
                Unload(StepperMotor.Two, Motion.Holder3);
                Unload(StepperMotor.Two, Motion.BinPosition);
                Unload(StepperMotor.Two, Motion.Holder4);
                Unload(StepperMotor.Two, Motion.BinPosition);
                Unload(StepperMotor.Two, Motion.Holder5);
                Unload(StepperMotor.Two, Motion.BinPosition);
                Unload(StepperMotor.Two, Motion.Holder6);
                Unload(StepperMotor.Two, Motion.BinPosition);
                Unload(StepperMotor.Two, Motion.HomePosition);
            });
        }

        private void TestUnloadAndLoadHolders()
        {
            Task.Run(() =>
            {
                SetSpeed(20);
                UnloadAndLoad(Motion.PickPosition, StepperMotor.One);
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
