using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;

namespace SolearyAPI.Models
{
    public class Company
    {
        public Company()
        {
            this.CreatedOn = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid CompanyID { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string StateAbbrev { get; set; }

        public string City { get; set; }

        public string Zip { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string Phone { get; set; }

        public string Website { get; set; }

        public string ContactName { get; set; }

        public string ContactEmail { get; set; }

        public DateTime? CreatedOn { get; set; }

        public ICollection<User> Users { get; set; }
    }
}