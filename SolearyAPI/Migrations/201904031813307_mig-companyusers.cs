namespace SolearyAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migcompanyusers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        CompanyID = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                        Address = c.String(),
                        StateAbbrev = c.String(),
                        City = c.String(),
                        Zip = c.String(),
                        Latitude = c.String(),
                        Longitude = c.String(),
                        Phone = c.String(),
                        Website = c.String(),
                        ContactName = c.String(),
                        ContactEmail = c.String(),
                        CreatedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.CompanyID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserID = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                        Address = c.String(),
                        StateAbbrev = c.String(),
                        City = c.String(),
                        Zip = c.String(),
                        Latitude = c.String(),
                        Longitude = c.String(),
                        Phone = c.String(),
                        Email = c.String(),
                        Salt = c.Binary(),
                        SaltedAndHashedPassword = c.Binary(),
                        CreatedOn = c.DateTime(),
                        CompanyID = c.Guid(),
                    })
                .PrimaryKey(t => t.UserID)
                .ForeignKey("dbo.Companies", t => t.CompanyID)
                .Index(t => t.CompanyID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "CompanyID", "dbo.Companies");
            DropIndex("dbo.Users", new[] { "CompanyID" });
            DropTable("dbo.Users");
            DropTable("dbo.Companies");
        }
    }
}
