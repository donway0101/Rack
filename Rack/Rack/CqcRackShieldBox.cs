using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rack
{
    public partial class CqcRack
    {
        private void ShieldBoxSetup()
        {
            if (_shieldBoxInstanced == false)
            {
                ShieldBox1 = new ShieldBox(1);
                ShieldBox2 = new ShieldBox(2);
                ShieldBox3 = new ShieldBox(3);
                ShieldBox4 = new ShieldBox(4);
                ShieldBox5 = new ShieldBox(5);
                ShieldBox6 = new ShieldBox(6);

                ShieldBox1.Position = Motion.ShieldBox1;
                ShieldBox2.Position = Motion.ShieldBox2;
                ShieldBox3.Position = Motion.ShieldBox3;
                ShieldBox4.Position = Motion.ShieldBox4;
                ShieldBox5.Position = Motion.ShieldBox5;
                ShieldBox6.Position = Motion.ShieldBox6;

                ShieldBoxs = new ShieldBox[6] { ShieldBox1, ShieldBox2, ShieldBox3, ShieldBox4, ShieldBox5, ShieldBox6 };
                _shieldBoxInstanced = true;
            }

            foreach (var box in ShieldBoxs)
            {
                box.PortName = XmlReaderWriter.GetBoxAttribute(Files.BoxData, box.Id, ShieldBoxItem.COM);
                box.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, box.Id, ShieldBoxItem.State) == "Enable";
                if (!Enum.TryParse(XmlReaderWriter.GetBoxAttribute(Files.BoxData, box.Id, ShieldBoxItem.Type), out ShieldBoxType type))
                {
                    throw new Exception("ShieldBoxSetup fail due to box type convert failure");
                }
                box.Type = type;

                if (box.Enabled)
                {
                    try
                    {
                        box.Start();
                        box.GreenLight();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Box " + box.Id + " is enabled but can't communicate: " + e.Message);
                    }
                }
            }
        }
 
        public void OpenAllBox()
        {
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled)
                {
                    if (box.IsClosed())
                    {
                        box.OpenBox();
                    }
                }
            }
        }

        public void CloseAllBoxAsync()
        {          
            List<Task<int>> tasks = new List<Task<int>>();
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled)
                {
                    if (box.IsClosed() == false)
                    {
                        tasks.Add(box.CloseBoxAsync());
                    }
                }
            }

            Task<int>[] boxTask = new Task<int>[tasks.Count];

            // Todo list to array.
            //Array.Copy(tasks, closeBoxTask, tasks.Count);

            for (int i = 0; i < tasks.Count; i++)
            {
                boxTask[i] = tasks[i];
            }

            Task.WaitAll(boxTask);

            foreach (var task in boxTask)
            {
                if (task.Result != 0)
                {
                    throw new Exception("Box " + task.Result + " close fail.");
                }
            }
        }

        public void OpenAllBoxAsync()
        {
            List<Task<int>> tasks = new List<Task<int>>();
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled)
                {
                    if (box.IsClosed() == true)
                    {
                        tasks.Add(box.OpenBoxAsync());
                    }
                }
            }

            Task<int>[] boxTask = new Task<int>[tasks.Count];
            for (int i = 0; i < tasks.Count; i++)
            {
                boxTask[i] = tasks[i];
            }

            Task.WaitAll(boxTask);

            foreach (var task in boxTask)
            {
                if (task.Result != 0)
                {
                    throw new Exception("Box " + task.Result + " open fail.");
                }
            }
        }

        public void CloseAllBox()
        {
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled)
                {
                    if (box.IsClosed() == false)
                    {
                        box.CloseBox();
                    }
                }
            }
        }

        public void InvalidAllBox()
        {
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled)
                {
                    box.Available = false;
                }
            }
        }

        public void ValidAllBox()
        {
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled)
                {
                    box.Available = true;
                }
            }
        }

        public void CheckBox()
        {
            OnInfoOccured(20011, "Test shield box.");
            BoxChecked = false;
            InvalidAllBox();
            CloseAllBoxAsync();
            Thread.Sleep(1000);
            OpenAllBoxAsync();
            //CloseAllBox();
            //Thread.Sleep(1000);
            //OpenAllBox();
            ValidAllBox();
            BoxChecked = true;
            OnInfoOccured(20012, "Test shield box succeed.");
        }

        public Task CloseBoxAsync(ShieldBox box)
        {
            return Task.Run(() =>
            {
                try
                {
                    OnInfoOccured(20027, "Try closing door of box:" + box.Id + ".");
                    box.CloseBox();
                }
                catch (Exception e)
                {                    
                    OnErrorOccured(40008, "Can't close box due to:" + e.Message);
                }               
            });
        }

    }
}
