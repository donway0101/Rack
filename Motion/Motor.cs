using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACS.SPiiPlusNET;

namespace Motion
{
    public class Motor
    {
        public Axis Id { get; set; }

        /// <summary>
        /// Encoder counts per round.
        /// </summary>
        public double EncCtsPerR { get; set; }

        public double BallScrewLead { get; set; }

        public double EncoderFactor { get; set; }

        public double HomeOffset { get; set; }

        /// <summary>
        /// Critical error on acceleration.
        /// </summary>
        public double CriticalErrAcc { get; set; }

        public double CriticalErrVel { get; set; }

        public double CriticalErrIdle { get; set; }

        public double SoftLimitNagtive { get; set; }

        public double SoftLimitPositive { get; set; }

        public double MaxTravel { get; set; }

        public int ErrCode { get; set; }

        public Motor()
        {

        }


    }
}
