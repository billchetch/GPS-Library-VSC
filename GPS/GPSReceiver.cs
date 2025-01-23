using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;


namespace Chetch.GPS;

//This is assumed to be a serial device

public class GPSReceiver : Chetch.Utilities.SerialPortConnection
{

    #region Constants
    #endregion

    #region Static stuff
    #endregion
    
    #region Properties
    public DateTime SentenceLastReceived { get; internal set; }
    public String LastSentenceReceived { get; internal set; } = String.Empty;
    #endregion

    #region Events
    public event EventHandler<String>? SentenceReceived;
    #endregion

    #region Constructors
    public GPSReceiver() : base(9600, Parity.None, 8, StopBits.One){}
    #endregion

    #region Methods
    protected override string getPortName()
    {
        String searchFor = String.Empty;
        String searchKey = String.Empty;
        if(OperatingSystem.IsMacOS())
        {
            searchFor = "u-blox 7 - GPS/GNSS Receiver";
            searchKey = "USB Product Name";
        }
        else if(OperatingSystem.IsLinux())
        {
            searchFor = "usb-u-blox"; //Full name: usb-u-blox_AG_-_www.u-blox.com_u-blox_7_-_GPS_GNSS_Receiver-if00 
        }
        else
        {
            throw new Exception(String.Format("Operation system {0} is not supported", Environment.OSVersion.Platform));
        }
        return GetPortNameForDevice(searchFor, searchKey);
    }

    protected override void OnDataReceived(byte[] data)
    {
        string str = Encoding.ASCII.GetString(data);
        String[] sentences = str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        foreach (String sentence in sentences)
        {
            if (sentence != null && !sentence.Equals(""))
            {
                SentenceLastReceived = DateTime.Now;
                LastSentenceReceived = sentence;
                SentenceReceived?.Invoke(this, sentence);
            }
            
        }

        base.OnDataReceived(data);
    }
    #endregion
}