using System;
using Chetch.Database;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chetch.GPS;

public class GPSDBContext : ChetchDbContext
{
    #region Constants
    public const String DEFAULT_DATABASE_NAME = "gps";
    #endregion

    [Table("gps_positions")]
    public class GPSPosition
    {
        [Column("id")]
        public long ID { get; set; } = 0;

        [Column("latitude")]
        public double Latitude { get; set; }

        [Column("longitude")]
        public double Longitude { get; set; }
    }

    public GPSDBContext(string databaseName = DEFAULT_DATABASE_NAME, string dbConfigKey = "DBConfig") : base(databaseName, dbConfigKey)
    {
    }


}
