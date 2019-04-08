using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Conveyor;
using ACS.SPiiPlusNET;
using EcatIo;
using System.Threading;

namespace ConveyorTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        PickAndPlaceConveyor conveyor;

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Api ch = new Api();
                ch.OpenCommEthernet("192.168.8.18", 701);
                EthercatIo iO = new EthercatIo(ch, 88, 8, 5);
                iO.Setup();
                conveyor = new PickAndPlaceConveyor(iO);
                conveyor.ErrorOccured += Conveyor_ErrorOccured;
                conveyor.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Conveyor_ErrorOccured(object sender, string description)
        {
            StopTesting = true;
            MessageBox.Show(description);
        }

        public bool StopTesting { get; set; }
        private void button2_Click(object sender, EventArgs e)
        {
            StopTesting = false;
            //conveyor.Run();
            Task.Run(() => {
                while (StopTesting == false)
                {
                    conveyor.CommandInposForPicking = true;
                    while (conveyor.InposForPicking==false)
                    {
                        Thread.Sleep(100);
                        Console.WriteLine("Waiting for InposForPicking");
                    }

                    conveyor.CommandReadyForPicking = true;
                    while (conveyor.CommandReadyForPicking == false)
                    {
                        Thread.Sleep(100);
                        Console.WriteLine("Waiting for ReadyForPicking");
                    }

                    Thread.Sleep(2000);
                    //Assume picking complete.
                    conveyor.InposForPicking = false;
                    conveyor.ReadyForPicking = false;
                    Thread.Sleep(2000);
                }
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                //conveyor.ResetCylinder(Output.BlockPick, Input.BlockPickUp, true);
                //conveyor.SetCylinder(Output.BlockPick, Input.BlockPickUp, false);

                conveyor.CommandInposForPicking = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
