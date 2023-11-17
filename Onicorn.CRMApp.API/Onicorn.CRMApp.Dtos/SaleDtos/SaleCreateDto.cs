﻿namespace Onicorn.CRMApp.Dtos.SaleDtos
{
    public class SaleCreateDto
    {
        public decimal SalesAmount { get; set; }
        public DateTime SalesDate { get; set; }

        public int CustomerId { get; set; }
        public int ProjectId { get; set; }
        public int SaleSituationId { get; set; }
    }
}
