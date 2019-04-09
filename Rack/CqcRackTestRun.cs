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
            UnloadAndLoad(Motion.ShieldBox4, StepperMotor.Two);
            UnloadAndLoad(Motion.ShieldBox1, StepperMotor.One);
            Place(StepperMotor.One);
        }

        private void TestLoadGold()
        {
            Task.Run(() =>
            {
                Load(StepperMotor.One, Motion.Gold1);
                Load(StepperMotor.One, Motion.ShieldBox1);
                Load(StepperMotor.One, Motion.Gold2);
                Load(StepperMotor.One, Motion.ShieldBox2);
                Load(StepperMotor.One, Motion.Gold3);
                Load(StepperMotor.One, Motion.ShieldBox3);
                Load(StepperMotor.One, Motion.Gold4);
                Load(StepperMotor.One, Motion.ShieldBox4);
                Load(StepperMotor.One, Motion.Gold5);
                Load(StepperMotor.One, Motion.ShieldBox5);
                Load(StepperMotor.One, Motion.Gold1);
                Load(StepperMotor.One, Motion.ShieldBox6);
                Load(StepperMotor.One, Motion.HomePosition);
            });
        }

        private void TestUnloadAndBin()
        {
            Task.Run(() =>
            {
                SetSpeed(20);
                Unload(StepperMotor.Two, Motion.ShieldBox1);
                Unload(StepperMotor.Two, Motion.BinPosition);
                Unload(StepperMotor.Two, Motion.ShieldBox2);
                Unload(StepperMotor.Two, Motion.BinPosition);
                Unload(StepperMotor.Two, Motion.ShieldBox3);
                Unload(StepperMotor.Two, Motion.BinPosition);
                Unload(StepperMotor.Two, Motion.ShieldBox4);
                Unload(StepperMotor.Two, Motion.BinPosition);
                Unload(StepperMotor.Two, Motion.ShieldBox5);
                Unload(StepperMotor.Two, Motion.BinPosition);
                Unload(StepperMotor.Two, Motion.ShieldBox6);
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
