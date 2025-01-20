using System;
using Chetch.Utilities;
using Chetch.Database;

namespace Chetch.GPS;

public class GPSManager
{
    #region Constants
    public const int DEFAULT_LOG_INTERVAL = 10000;
    public const String DEFAULT_LOG_NAME = "gps-manager";
    #endregion

    #region Static stuff
    #endregion
    
    #region Classes
    public class GPSPositionData
    {

        public long DBID = 0;
        public double Latitude = 0;
        public double Longitude = 0;
        public double HDOP = 0;
        public double VDOP = 0;
        public double PDOP = 0;
        public double Speed = 0;
        public double Bearing = 0; //in degrees
        String? SentenceType = null;
        public DateTime Timestamp;

        public GPSPositionData(){}

        public GPSPositionData(double latitude, double longitude, double hdop, double vdop, double pdop, String sentenceType)
        {
            Latitude = latitude;
            Longitude = longitude;
            HDOP = hdop;
            VDOP = vdop;
            PDOP = pdop;
            SentenceType = sentenceType;
            Timestamp = DateTime.Now;
        }

        public GPSPositionData(GPSPositionData positionData)
        {
            Latitude = positionData.Latitude;
            Longitude = positionData.Longitude;
            HDOP = positionData.HDOP;
            VDOP = positionData.VDOP;
            PDOP = positionData.PDOP;
            Timestamp = DateTime.Now;
        }

        public void SetMotionData(GPSPositionData previousPos)
        {
            double elapsed = (double)(Timestamp - previousPos.Timestamp).TotalSeconds;
            if (elapsed <= 0) throw new Exception("Bad timestamp diference " + elapsed + "ms");

            //cacculate speed
            double distance = Measurement.GetDistance(Latitude, Longitude, previousPos.Latitude, previousPos.Longitude);
            Speed = distance / elapsed;
            Bearing = Measurement.GetFinalBearing(previousPos.Latitude, previousPos.Longitude, Latitude, Longitude);
        }

        public override string ToString()
        {
            String dt = Timestamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss UTC");
            return String.Format("{0} Lat/Lon: {1},{2}, Heading: {3}deg @ {4}mps", dt, Latitude, Longitude, Bearing, Speed);
        }
    }

    //TODO: record satellite data
    public class GPSSatelliteData
    {
        public int PseudoRandomCode;
        public int Elevation;
        public int Azimuth;
        public int SignalToNoiseRatio;

        public GPSSatelliteData(int[] satellite)
        {
            PseudoRandomCode = satellite[0];
            Elevation = satellite[1];
            Azimuth = satellite[2];
            SignalToNoiseRatio = satellite[3];
        }

        override public String ToString()
        {
            return String.Format("E: {0}, A: {1}, S2N: {2}", Elevation, Azimuth, SignalToNoiseRatio);
        }
    }
    #endregion

    #region Properties
    #endregion

    #region Fields
    NMEAInterpreter nmea = new NMEAInterpreter();
    GPSReceiver reciever = new GPSReceiver();

    GPSPositionData currentPosition = new GPSPositionData();
    GPSPositionData? previousPosition;

    String gpsDatabaseName = GPSDBContext.DEFAULT_DATABASE_NAME;

    SysLogDBContext logDBContext;
    

    System.Timers.Timer logTimer = new System.Timers.Timer();
    #endregion

    public GPSManager(int logInterval = DEFAULT_LOG_INTERVAL)
    {
        nmea.PositionReceived += (latitude, longitude) => {
            currentPosition.Latitude = latitude;
            currentPosition.Longitude = longitude;
            Console.WriteLine("Position: {0}/{1}", latitude, longitude);
        };
        nmea.BearingReceived += (bearing) => { currentPosition.Bearing = bearing; };
        nmea.SpeedReceived += (speed) => { currentPosition.Speed = speed; };
        nmea.HDOPReceived += (hdop) => { currentPosition.HDOP = hdop; };
        nmea.VDOPReceived += (vdop) => { currentPosition.VDOP = vdop; };
        nmea.PDOPReceived += (pdop) => { currentPosition.PDOP = pdop; };
        
        reciever.SentenceReceived += (sender, sentence) => {
            //Console.WriteLine("Received: {0}", sentence);
            nmea.Parse(sentence);
        };

        reciever.Connected += (sender, connected) => {
            Console.WriteLine("Connected: {0}", connected);
            SysLogDBContext.Log(gpsDatabaseName, 
                connected ? SysLogDBContext.LogEntryType.INFO : SysLogDBContext.LogEntryType.WARNING,
                String.Format("GPS receiver connected: {0}", connected));
        };

        logTimer.AutoReset = true;
        logTimer.Interval = logInterval;
        logTimer.Elapsed += (sender, eargs) => {
                //save current position to db
                if(reciever.IsConnected)
                {
                    //log current position
                    currentPosition.Timestamp = DateTime.Now;

                    
                    //record previous position
                    previousPosition = new GPSPositionData(currentPosition);
                }
                else
                {
                    //log an error i guess
                }
            };
    }

    public void StartRecording()
    {
        try
        {
            reciever.Connect();

            //start the timer
            logTimer.Start();

            //record this in the logs
            SysLogDBContext.Log(gpsDatabaseName, 
                SysLogDBContext.LogEntryType.INFO,
                String.Format("Start recording called, gps receiver connected: {0}", reciever.IsConnected));
            
        } catch (Exception e)
        {
            SysLogDBContext.Log(gpsDatabaseName, e);
        }
    }

    public void StopRecording()
    {
        logTimer.Stop();
        reciever.Disconnect();
    }
}
