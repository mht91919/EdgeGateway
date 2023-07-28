using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml.Linq;
using NLog;

namespace EdgeGateway
{
    /// <summary>
    //灯变色
    //{
    //    "model":"a1l0aYxKTHW",
    //    "sim":"69860434191830800933",
    //    "version":"003",
    //    "type":"2001",
    //    "targetstate":"001"
    //}

    //报警声
    //{
    //    "model":"a1l0aYxKTHW",
    //    "sim":"69860434191830800933",
    //    "version":"003",
    //    "type":"2019",
    //    "buzz":"1"
    //}

    /// </summary>
    public class LampManageWifi
    {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 灯型号
        /// </summary>
        private string _model { get; set; }

        /// <summary>
        ///      灯编号
        /// </summary>
        private string _sim { get; set; }

        /// <summary>
        ///      版本
        /// </summary>
        private string _version { get; set; }

        /// <summary>
        /// 接口地址
        /// </summary>
        private string _funSSDControl { set; get; }


        /// <summary>
        ///"000":关灯
        ///"001":绿灯
        ///"010":黄灯
        ///"100":红灯
        ///"110":红黄灯
        /// </summary>
        private string _targetstate { get; set; }

        /// <summary>
        /// 初始化正常
        /// </summary>
        private int _currentStatus { get; set; } = 0;

        ///// <summary>
        ////"0":停止报警声
        ////"1": 报警声长响
        ////"2":报警声响停
        ///// </summary>
        //public string Buzz { get; set; }

        /// <summary>
        /// 三色灯当前是报警(喇叭)
        /// </summary>
        private bool _isBuzzAlarm { get; set; }

        /// <summary>
        /// 是否启用喇叭
        /// </summary>
        public bool _isUseBuzz { get; set; }

        public LampManageWifi(string model, string sim, string version, string funSSDControl)
        {
            _model = model;
            _sim = sim;
            _version = version;
            _funSSDControl = funSSDControl;
        }

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
                SetLampBuzz("0");
        }

        /// <summary>
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
        /// 0 没有报警 关灯，停止报警
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
                //喇叭按钮打开
                case 0:
                    result &= SetLampColor("000");//关灯
                    result &= SetLampBuzz("0");//停止报警
                    break;
                case 1:
                    result &= SetLampColor("100");//红灯
                    result &= SetLampBuzz("2");//短音
                    break;
                case 2:
                    result &= SetLampColor("010");//黄灯
                    result &= SetLampBuzz("1");//长音
                    break;
                case 3:
                    result &= SetLampColor("110");//红黄灯都亮
                    result &= SetLampBuzz("1");
                    break;
                case 4:
                    result &= SetLampColor("001");//绿灯
                    result &= SetLampBuzz("0");//没音
                    break;

                //喇叭没打开
                case 5:
                    result &= SetLampColor("100");//红灯
                    result &= SetLampBuzz("0");//没音
                    break;
                case 6:
                    result &= SetLampColor("010");//黄灯
                    result &= SetLampBuzz("0");//没音
                    break;
                case 7:
                    result &= SetLampColor("110");//红黄灯都亮
                    result &= SetLampBuzz("0");//没音
                    break;
                case 8:
                    result &= SetLampColor("001");//绿灯
                    result &= SetLampBuzz("0");//没音
                    break;
            }

            _currentStatus = status; //当前灯的状态   
            return result;
        }

        /// <summary>
        /// 设置三色灯颜色
        /// 000关灯
        /// 100红灯
        /// 010黄灯
        /// 001绿灯
        /// </summary>
        /// <param name="color">灯颜色字符串</param>
        /// <returns></returns>
        private bool SetLampColor(string color)
        {
            //颜色的状态

            SSDHttpPostLampManage("2001", color);

            return true;
        }

        /// <summary>
        /// 设置灯喇叭状态0.关闭，1.长音，2.短音
        /// 电流长音，电压短音
        /// </summary>
        /// <param name="status">喇叭状态</param>
        /// <returns></returns>
        private bool SetLampBuzz(string status)
        {
            LBHttpPostLampManage("2019", status);
            //接口的反馈状态
            return true;
        }


        /// <summary>
        /// 是否报警
        /// </summary>
        /// <returns></returns>
        public bool IsBuzzAlarm()
        {
            return _isBuzzAlarm;
        }


        /// <summary>
        /// 三色灯接口
        /// </summary>
        /// <param name="type"></param>
        /// <param name="targetstate"></param>
        public async void SSDHttpPostLampManage(string type, string targetstate)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var str = JsonConvert.SerializeObject(new { model = _model, sim = _sim, version = _version, type, targetstate });
                    var response = await httpClient.PostAsync(
                      _funSSDControl,
                     new StringContent(
                     str,
                      Encoding.UTF8,
                     "application/json")
                        );
                    logger.Info($"SSDHttpPostLampManagePost:{str};path:{_funSSDControl}");
                    var data = await response.Content.ReadAsStringAsync();
                    logger.Info($"SSDHttpPostLampManageGet:  {data}");

                    if (data != null)
                    {
                        var obj = JsonConvert.DeserializeObject<JObject>(data);

                        if (obj != null)
                        {
                            bool result = obj.Children().FirstOrDefault(x => x.Path == "code").First.ToString() == "200";
                            if (!result)
                            {
                                logger.Error("SSDHttpPostLampManageError:三色灯调用失败!");
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    logger.Error($"{"SSDHttpPostLampManageError:" + ex.Message}");
                }
            }
        }

        /// <summary>
        /// 喇叭
        /// </summary>
        /// <param name="type"></param>
        /// <param name="buzz"></param>
        public async void LBHttpPostLampManage(string type, string buzz)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var str = JsonConvert.SerializeObject(new { model = _model, sim = _sim, version = _version, type, buzz });

                    logger.Info($"LBHttpPostLampManagePost:{str};path:{_funSSDControl}");

                    var response = await httpClient.PostAsync(
                      _funSSDControl,
                     new StringContent(
                     str,
                      Encoding.UTF8,
                     "application/json")
                        );

                    var data = await response.Content.ReadAsStringAsync();
                    logger.Info($"LBHttpPostLampManageGet:  {data}");

                    if (data != null)
                    {
                        var obj = JsonConvert.DeserializeObject<JObject>(data);

                        if (obj != null)
                        {
                            bool result = obj.Children().FirstOrDefault(x => x.Path == "code").First.ToString() == "200";
                            if (!result)
                            {
                                logger.Error("LBHttpPostLampManageError:喇叭调用失败!");
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    logger.Error($"{"LBHttpPostLampManageError:" + ex.Message}");
                }
            }
        }

    }
}
