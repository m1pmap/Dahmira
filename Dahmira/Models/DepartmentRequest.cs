using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    public class DepartmentRequest
    {
        public string RequestNum { get; set; } = "0";
        public DateTime Date { get; set; } = DateTime.Now;
        public string Manager { get; set; } = string.Empty;
        public string Client { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        public string AnimalType { get; set; } = string.Empty;
        public string HeadCount { get; set; } = string.Empty;
        public bool IsBuildingHas { get; set; } = false;
        public string Group { get; set; } = string.Empty;
        public string MaxWeight { get; set; } = string.Empty;
        public string SectionCount { get; set; } = string.Empty;

        public string LSize { get; set; } = string.Empty;
        public string H1Size { get; set; } = string.Empty;
        public string H2Size { get; set; } = string.Empty;
        public string WSize { get; set; } = string.Empty;
        public string L1Size { get; set; } = string.Empty;
        public string L2Size { get; set; } = string.Empty;

        public string Maintanance { get; set; } = string.Empty;
        public string Feeding { get; set; } = string.Empty;
        public string Watering { get; set; } = string.Empty;
        public string Microclimate { get; set; } = string.Empty;
        public string ManureRemoval { get; set; } = string.Empty;

        public string Mounting { get; set; } = string.Empty;
        public string Electricity { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

    }
}
