using System;
using Chetch.Utilities;

namespace Chetch.GPS;

public class GPSManager
{
    #region Constants
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

        public GPSPositionData()
        {
        }

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
    #endregion

    public GPSManager()
    {
        nmea.PositionReceived += positionReceived;

        reciever.SentenceReeived += (sender, sentence) => {
            nmea.Parse(sentence);
        };
    }

    public void StartRecording()
    {
        try
        {
            reciever.Connect();
        } catch (Exception e)
        {
            
        }
    }

    public void StopRecording()
    {
        reciever.Disconnect();
    }

    void positionReceived(double latitude, double longitude){

    }
}
