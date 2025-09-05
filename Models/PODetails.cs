using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlusCP.Models
{
    public class PODetails
    {
        public int Id { get; set; } // Primary key for your database

        public int PONum { get; set; }
        public int POLine { get; set; }
        public int PORelNum { get; set; }

        public string IUM { get; set; }

        public string VendorID { get; set; }
        public string VendorName { get; set; }

        public string BuyerID { get; set; }
        public string BuyerName { get; set; }

        public string PartNum { get; set; }
        public string LineDesc { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime DueDate { get; set; }

        public decimal OrderQty { get; set; }
        public decimal? ReceivedQty { get; set; }
        public decimal? ArrivedQty { get; set; }
        public decimal RelQty { get; set; }

        public string VendorEmail { get; set; }
        public string BuyerEmail { get; set; }

        public decimal UnitCost { get; set; }
        public decimal ExtCost { get; set; }

        public string Company { get; set; }

        public decimal? OurQty { get; set; }
        public decimal? CalculatedUnitCost { get; set; }
        public DateTime? ArrivedDate { get; set; }

        public DateTime ChangeDate { get; set; }

        public bool IsApproved { get; set; }
        public string ApprovalStatus { get; set; }

        public string RowIdent { get; set; }
    }
}