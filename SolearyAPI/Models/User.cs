using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;

namespace SolearyAPI.Models
{
    public class User
    {
        public User()
        {
            this.CreatedOn = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserID { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string StateAbbrev { get; set; }

        public string City { get; set; }

        public string Zip { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string Phone { get; set; }

        public string ProfileImageUrl { get; set; }

        public string Email { get; set; }
        
        [NotMapped]
        public string Password { get; set; }

        public byte[] Salt { get; set; }

        public byte[] SaltedAndHashedPassword { get; set; }

        public DateTime? CreatedOn { get; set; }

        [ForeignKey("Company")]
        public Guid? CompanyID { get; set; }

        public Company Company { get; set; }

    }
}