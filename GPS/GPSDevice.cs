using System.IO.Ports;
using System.Reflection.Metadata.Ecma335;
using Microsoft.VisualBasic;

namespace Chetch.GPS;

//This is assumed to be a serial device

public class GPSDevice
{
    #region Constants
    public const int REOPEN_TIMER_INTERVAL = 2000;
    #endregion

    #region Static stuff
    static public bool PortExists(String portName)
    {
        var portNames = SerialPort.GetPortNames();
        foreach(var pn in portNames){
            if(pn.Equals(portName)){
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Enums and Classes
    #endregion

    #region Fields
    int baudRate;
    Parity parity;
    int dataBits;
    StopBits stopBits;

    SerialPort? serialPort = null;
    
    System.Timers.Timer reopenTimer = new System.Timers.Timer();
    #endregion

    #region Properties
    
    public String PortName { get; internal set; } = String.Empty;
    
    bool IsListening => serialPort != null && serialPort.IsOpen;
    #endregion

    #region Events
    public event EventHandler<String>? NewSentenceReceived;
    #endregion

    #region Constructors
    public GPSDevice(String portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
    {
        PortName = portName;
        this.baudRate = baudRate;
        this.parity = parity;
        this.dataBits = dataBits;
        this.stopBits = stopBits;
    }
    #endregion

    #region Methods
    public void StartListening()
    {
        if(serialPort == null)
        {
            serialPort = new SerialPort(PortName, baudRate, parity, dataBits, stopBits);
            serialPort.DataReceived += (ArrayShapeEncoder, eargs) => {
                    int dataLength = serialPort.BytesToRead;
                    byte[] data = new byte[dataLength];
                    int nbrDataRead = serialPort.Read(data, 0, dataLength);
                    if (nbrDataRead == 0)
                        return;

                    string str = System.Text.Encoding.ASCII.GetString(data);
                    String[] sentences = str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    foreach (String sentence in sentences)
                    {
                        NewSentenceReceived?.Invoke(this, sentence);
                    }
                };
            
            //create timer for reopen
            reopenTimer.AutoReset = false;
            reopenTimer.Interval = REOPEN_TIMER_INTERVAL;
            reopenTimer.Elapsed += (sender, eargs) => {
                    reopenTimer.Stop();

                    Console.WriteLine("Reopen timer fired");
                    try
                    {
                        if(!IsListening)
                        {
                            StartListening();
                        }
                    }
                    catch (Exception e)
                    {
                        //what to do here??
                    }
                    reopenTimer.Start();
                };
        }
        else if(serialPort.IsOpen)
        {
            serialPort.Close();
            reopenTimer.Stop();
        }

        //we check for the existence of the port
        if(PortExists(PortName))
        {
            try
            {
                serialPort.Open();
                Console.WriteLine("Serial port opened");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }
        else 
        {
            Console.WriteLine("Port {0} not found", PortName);
        }
        reopenTimer.Start();
    }

    public void StopListening()
    {
        if(serialPort != null)
        {
            if(serialPort.IsOpen)
                serialPort.Close();

            reopenTimer.Stop();
        }
    }
    #endregion
}