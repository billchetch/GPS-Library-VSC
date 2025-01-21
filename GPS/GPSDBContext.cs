using System;
using Chetch.Database;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

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
        public long ID { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; }

        [Column("longitude")]
        public double Longitude { get; set; }

        [Column("hdop")]
        public double HDOP { get; set; }

        [Column("vdop")]
        public double VDOP { get; set; }
        
        [Column("pdop")]
        public double PDOP { get; set; }

        [Column("speed")]
        public double Speed { get; set; }

        [Column("bearing")]
        public double Bearing { get; set; } //in degrees

        [NotMapped]        
        String? SentenceType { get; set; } = null;

        [NotMapped]
        public DateTime Timestamp;
    }

    public DbSet<GPSPosition> GPSPositions { get; set; }


    public GPSDBContext(string databaseName = DEFAULT_DATABASE_NAME, string dbConfigKey = "DBConfig") : base(databaseName, dbConfigKey)
    {
    }


}
