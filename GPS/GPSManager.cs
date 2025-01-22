using System;
using Chetch.Utilities;
using Chetch.Database;
using System.Dynamic;

namespace Chetch.GPS;

public class GPSManager
{
    #region Constants
    public const int DEFAULT_LOG_INTERVAL = 10000;
    public const String DEFAULT_LOG_NAME = "gps-manager";
    public const String DEVICE_STATUS_KEY = "gps-device-status";
    #endregion

    #region Static stuff
    #endregion
    
    #region Classes
    class DeviceStatus
    {
        public bool Connected { get; set; }
        public String? Message {get; set; }

        public DeviceStatus(bool connected, String? message = null)
        {
            Connected = connected;
            Message = message;
        }
    }

    #endregion

    #region Properties
    public GPSDBContext.GPSPosition CurrentPosition { get; internal set; } = new GPSDBContext.GPSPosition();

    public bool IsReceiverConnected => reciever.IsConnected;
    
    #endregion

    #region Fields
    NMEAInterpreter nmea = new NMEAInterpreter();
    GPSReceiver reciever = new GPSReceiver();

    String gpsDatabaseName = GPSDBContext.DEFAULT_DATABASE_NAME;

    System.Timers.Timer logTimer = new System.Timers.Timer();
    #endregion

    public GPSManager(int logInterval = DEFAULT_LOG_INTERVAL)
    {
        nmea.PositionReceived += (latitude, longitude) => {
            CurrentPosition.AddPosition(latitude, longitude);
            
            //Console.WriteLine("Position: {0}/{1}", latitude, longitude);
        };
        nmea.BearingReceived += (bearing) => { CurrentPosition.Bearing = bearing; };
        nmea.SpeedReceived += (speed) => { CurrentPosition.Speed = speed; };
        nmea.HDOPReceived += (hdop) => { CurrentPosition.HDOP = hdop; };
        nmea.VDOPReceived += (vdop) => { CurrentPosition.VDOP = vdop; };
        nmea.PDOPReceived += (pdop) => { CurrentPosition.PDOP = pdop; };
        
        reciever.SentenceReceived += (sender, sentence) => {
            //Console.WriteLine("Received: {0}", sentence);
            nmea.Parse(sentence);
        };

        reciever.Connected += (sender, connected) => {
            //Console.WriteLine("Connected: {0}", connected);

            if(connected)
            {
                logTimer.Start();
            }
            else
            {
                logTimer.Stop();
            }

            SysLogDBContext.Log(gpsDatabaseName, 
                connected ? SysLogDBContext.LogEntryType.INFO : SysLogDBContext.LogEntryType.WARNING,
                String.Format("GPS receiver connected: {0}", connected),
                DEFAULT_LOG_NAME);

            SysInfoDBContext.Save(gpsDatabaseName, 
                DEVICE_STATUS_KEY, 
                new DeviceStatus(reciever.IsConnected, "Device connection status chahgned"));
        };

        logTimer.AutoReset = true;
        logTimer.Interval = logInterval;
        logTimer.Elapsed += (sender, eargs) => {
                //save current position to database ... but only if a position has been set
                if(CurrentPosition.PositionAdded)
                {
                    using(var gpsdb = new GPSDBContext(gpsDatabaseName))
                    {
                        gpsdb.Add(CurrentPosition);
                        gpsdb.SaveChanges();
                        CurrentPosition.Reset(); //so can be used again
                    }
                }
            };
    }

    public void StartRecording()
    {
        try
        {
            reciever.Connect();

            //record this in the logs
            SysLogDBContext.Log(gpsDatabaseName, 
                SysLogDBContext.LogEntryType.INFO,
                String.Format("Start recording called, gps receiver connected: {0}", reciever.IsConnected),
                DEFAULT_LOG_NAME);
            
            SysInfoDBContext.Save(gpsDatabaseName, 
                DEVICE_STATUS_KEY, 
                new DeviceStatus(reciever.IsConnected, "Service has started"));

        } catch (Exception e)
        {
            SysLogDBContext.Log(gpsDatabaseName, e);
        }
    }

    public void StopRecording()
    {
        //record this in the logs
        SysLogDBContext.Log(gpsDatabaseName, 
            SysLogDBContext.LogEntryType.INFO,
            String.Format("Stop recording called, gps receiver connected: {0}", reciever.IsConnected),
            DEFAULT_LOG_NAME);

        reciever.Disconnect();
    }
}
