﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onicorn.CRMApp.Dtos.StatisticDtos
{
    public class StatisticDto
    {
        public int UserCount { get; set; }
        public int MaleUserCount { get; set; }
        public int FemaleUserCount { get; set; }
        public int UnknownUserCount { get; set; }
        public int ProjectCount { get; set; }
        public int CustomerCount { get; set; }
        public int TaskCount { get; set; }
        public int ActiveTaskCount { get; set; }
        public int FinishedTaskCount { get; set; }
        public int SaleCount { get; set; }
        public decimal TotalSalesPrice { get; set; }
    }
}
