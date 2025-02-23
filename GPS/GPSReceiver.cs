using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;


namespace Chetch.GPS;

//This is assumed to be a serial device

public class GPSReceiver : Chetch.Utilities.SerialPortConnection
{

    #region Constants
    const int VENDOR_ID = 0x1546;
    const int PRODUCT_ID = 0x01a7;
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
    protected override string GetPortName()
    {
        String devicePath;
        if(OperatingSystem.IsMacOS())
        {
            devicePath = "/dev/tty.usbmodem*";
        }
        else if(OperatingSystem.IsLinux())
        {
            devicePath = "/dev/serial/by-id/usb-u-blox*"; //Full name: usb-u-blox_AG_-_www.u-blox.com_u-blox_7_-_GPS_GNSS_Receiver-if00 
        }
        else
        {
            throw new Exception(String.Format("Operation system {0} is not supported", Environment.OSVersion.Platform));
        }

        var devices = GetUSBDevices(devicePath);
        foreach(var f in devices)
        {
            USBDeviceInfo di = GetUSBDeviceInfo(f);
            if(di.IsValidProduct(PRODUCT_ID, VENDOR_ID))
            {
                return di.PortName;
            }
        }
        throw new Exception(String.Format("Failed to find port for device path {0}", devicePath));
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