using Newtonsoft.Json.Linq;
using System;
using System.IO.Ports;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EdgeGateway
{
    /// </summary>
    public class LampManageUSB
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 端口号
        /// </summary>
        private string _port { get; set; }

        private int _writeTimeout { get; set; } = 5000;

        private int _baudRate { get; set; } = 9600;

        private StopBits _stopBits { get; set; } = StopBits.One;

        private Parity _parity { get; set; } = Parity.None;

        private static JObject configData;

        /// <summary>
        /// 是否启用喇叭
        /// </summary>
        public bool _isUseBuzz { get; set; }

        public LampManageUSB(string port)
        {
            _port = port;
        }

        /// <summary>
        /// 初始化正常
        /// </summary>
        private int _currentStatus { get; set; } = 0;


        /// <summary>
        /// 设置喇叭状态
        /// </summary>
        /// <param name="status"></param>
        public void SetUseBuzz(bool status)
        {
            logger.Info($"当前报警消音的按钮状态：{status}");
            //改变喇叭启用状态
            _isUseBuzz = status;

            //TODO:喇叭状态？
            if (!status)
                //设置消音
                SetLampBuzz("A0 04 00 A4");//没音
        }


        /// <summary>
        /// 0 没有报警 关灯，停止报警
        /// 喇叭开着
        /// 1 电压报警  颜色红色，电压短音
        /// 2 电流报警  颜色黄色，电流长音
        /// 3 报警同时存在  颜色红黄色，电流长音
        /// 4 有任务 颜色绿灯 停止报警 
        /// 喇叭关着
        /// 5 电压报警  颜色红色，电压短音
        /// 6 电流报警  颜色黄色，电流长音
        /// 7 报警同时存在  颜色红黄色，电流长音
        /// 8 有任务 颜色绿灯 停止报警 

        /// </summary>
        /// <returns></returns>
        public bool SetLampStatus(int status)
        {
            logger.Info($"当前灯的状态:{_currentStatus},传的灯的状态：{status}");

            //当前灯状态未改变
            //if (_currentStatus == status) return true;

            bool result = true;

            switch (status)
            {
                case 0:
                    result &= SetLampColor("A0 00 00 A0");//关灯,关喇叭
                    break;

                //喇叭按钮打开
                case 1:
                    result &= SetLampColor("A0 03 01 A4");//红灯
                    result &= SetLampColor("A0 01 00 A1");//黄灯关
                    result &= SetLampColor("A0 02 00 A2");//绿灯关
                    result &= SetLampBuzz("A0 04 02 A6");//短音
                    break;
                case 2:
                    result &= SetLampColor("A0 01 01 A2");//黄灯
                    result &= SetLampColor("A0 03 00 A3");//红灯关
                    result &= SetLampColor("A0 02 00 A2");//绿灯关
                    result &= SetLampBuzz("A0 04 01 A5");//长音
                    break;
                case 3:
                    result &= SetLampColor("A0 03 01 A4");//红灯亮
                    result &= SetLampColor("A0 01 01 A2");//黄灯亮
                    result &= SetLampColor("A0 02 00 A2");//绿灯关
                    result &= SetLampBuzz("A0 04 01 A5");//长音
                    break;
                case 4:
                    result &= SetLampColor("A0 02 01 A3");//绿灯
                    result &= SetLampColor("A0 03 00 A3");//红灯关
                    result &= SetLampColor("A0 01 00 A1");//黄灯关
                    result &= SetLampBuzz("A0 04 00 A4");//没音
                    break;

                //喇叭没打开
                case 5:
                    result &= SetLampColor("A0 03 01 A4");//红灯
                    result &= SetLampColor("A0 01 00 A1");//黄灯关
                    result &= SetLampColor("A0 02 00 A2");//绿灯关
                    result &= SetLampBuzz("A0 04 00 A4");//没音
                    break;
                case 6:
                    result &= SetLampColor("A0 01 01 A2");//黄灯
                    result &= SetLampColor("A0 03 00 A3");//红灯关
                    result &= SetLampColor("A0 02 00 A2");//绿灯关
                    result &= SetLampBuzz("A0 04 00 A4");//没音
                    break;
                case 7:
                    result &= SetLampColor("A0 03 01 A4");//红灯亮
                    result &= SetLampColor("A0 01 01 A2");//黄灯亮
                    result &= SetLampBuzz("A0 04 00 A4");//没音
                    break;
                case 8:
                    result &= SetLampColor("A0 02 01 A3");//绿灯
                    result &= SetLampColor("A0 03 00 A3");//红灯关
                    result &= SetLampColor("A0 01 00 A1");//黄灯关
                    result &= SetLampBuzz("A0 04 00 A4");//没音
                    break;
            }

            _currentStatus = status; //当前灯的状态   
            return result;
        }

        /// <summary>
        /// 设置三色灯颜色
        /// </summary>
        /// <param name="color">灯颜色字符串</param>
        /// <returns></returns>
        private bool SetLampColor(string color)
        {
            bool result = false;

            byte[] data = HexStrTobyte(color);

            result = WriteToSerialPort(data);

            return result;
        }

        /// <summary>
        /// 设置灯喇叭状态0.关闭，1.长音，2.短音
        /// 电流长音，电压短音
        /// </summary>
        /// <param name="status">喇叭状态</param>
        /// <returns></returns>
        private bool SetLampBuzz(string status)
        {
            bool result = false;

            byte[] data = HexStrTobyte(status);

            result = WriteToSerialPort(data);

            return result;
        }

        /// <summary>
        /// 将要发送的字符串转换成16进制的数据流
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private byte[] HexStrTobyte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Trim(), 16);
            return returnBytes;
        }

        private bool WriteToSerialPort(byte[] byteArr)
        {
            SerialPort Com = new SerialPort();

            try
            {
                Com.ReadTimeout = 5000;
                Com.WriteTimeout = 5000;
                Com.PortName = _port;
                Com.BaudRate = 9600;
                Com.StopBits = StopBits.One;
                Com.Parity = Parity.None;
                Com.Open();
                Com.Write(byteArr, 0, byteArr.Length);
                return true;
            }
            catch (Exception ex)
            {
                logger.Info($"USB三色灯错误！错误原因:{ex.Message}");

                return false;
            }
            finally
            {
                Com.Close();
            }
        }
    }
}
