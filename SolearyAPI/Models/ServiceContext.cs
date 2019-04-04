using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace SolearyAPI.Models
{
    public class ServiceContext : DbContext
    {
       
        private const string connectionStringName = "Name=MS_TableConnectionString";

        public ServiceContext()
            : base(connectionStringName)
        {
            base.Configuration.ProxyCreationEnabled = false;
        }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

        }

        public System.Data.Entity.DbSet<SolearyAPI.Models.Company> Companies { get; set; }

        public System.Data.Entity.DbSet<SolearyAPI.Models.User> Users { get; set; }
    }
}