using System.Threading;

namespace Rack
{
    public partial class CqcRack
    {
        
 
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

       

 

    }
}
