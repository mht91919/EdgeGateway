﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdgeGateway
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [JavascriptCallbackBehavior(UrlParameterName = "jsonpCallback")]
    public class RestService : IRest
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static RestService _rest;
        WebServiceHost _webServiceHost;
        ServiceStatus _status = ServiceStatus.Closed;

        public Action CloseHandle;

        private static EdgeGatewayModel _edgeGatewayModel = new EdgeGatewayModel();

        private static JObject configData;

        private WebSocketClient webSocketClientCVW = new WebSocketClient();

        private WebSocketClient webSocketClientDevice = new WebSocketClient();

        private LampManageWifi _lampManageWifi;

        private LampManageUSB _lampManageUSB;

        public static int FormStytle = 1;

        public RestService()
        {

        }

        public void Initialization()
        {
            Initpa();
            Init();
        }

        public void Start()
        {
            //var path = configData.Children().FirstOrDefault(x => x.Path == "getCurrentVoltageWorkMode").First.ToString();

            //webSocketClientCVW.Connect(path);

            //var path2 = configData.Children().FirstOrDefault(x => x.Path == "getDeviceRealConnect").First.ToString();
            //webSocketClientDevice.Connect(path2, "{\"cmd\":\"sub\",\"topic\":\"all_device_state\"}");

        }

        /// <summary>
        /// 初始话参数
        /// </summary>
        private void Initpa()
        {
            GetControl();

            configData = GetControlJobj();

            InitData();

            InitLampWifi();//初始化三色灯

            InitLampUSB();

            ////初始化Miot的Ws
            //InitWebSocketMiot();

            //InitWebSocketDeviceStatus();

            HttpQueryByCodeAndType();
        }

        /// <summary>
        /// 初始化灯
        /// </summary>
        private void InitLampWifi()
        {
            string model = configData.Children().FirstOrDefault(x => x.Path == "model").First.ToString();
            string sim = configData.Children().FirstOrDefault(x => x.Path == "sim").First.ToString();
            string version = configData.Children().FirstOrDefault(x => x.Path == "version").First.ToString();
            var path = configData.Children().FirstOrDefault(x => x.Path == "funSSDControl").First.ToString();
            _lampManageWifi = new LampManageWifi(model, sim, version, path);
        }

        private void InitLampUSB()
        {
            string port = configData.Children().FirstOrDefault(x => x.Path == "port").First.ToString();

            _lampManageUSB = new LampManageUSB(port);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitData()
        {
            _edgeGatewayModel.TaskInfo.weld_mode = "0";

            //电流电压初始值
            _edgeGatewayModel.CVWMInfo.maxCurrent = 0;
            _edgeGatewayModel.CVWMInfo.minCurrent = 0;
            _edgeGatewayModel.CVWMInfo.maxVoltage = 0;
            _edgeGatewayModel.CVWMInfo.minVoltage = 0;

            _edgeGatewayModel.CVWMInfo.medianVoltage = 0;
            _edgeGatewayModel.CVWMInfo.medianCurrent = 0;


            //是否有任务任务初始值
            _edgeGatewayModel.HasTask = false;
        }

        /// <summary>
        /// 初始化Mito的ws，监听电流电压的数据
        /// </summary>
        private void InitWebSocketMiot()
        {
            webSocketClientCVW.ReceiveData += WebSocketClient_ReceiveData;
        }

        /// <summary>
        /// 初始化平台的的ws，监听设备在线数据
        /// </summary>
        private void InitWebSocketDeviceStatus()
        {
            webSocketClientDevice.ReceiveData += WebSocketClientDevice_ReceiveData;
        }

        private void WebSocketClientDevice_ReceiveData(object sender, WebSocketSharp.MessageEventArgs e)
        {

            //TODO:接受WS推的数据，优先级不高
            logger.Info($"ReceiveWebSocketData:{e.Data}");
        }

        /// <summary>
        /// 绑定ws收到的结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebSocketClient_ReceiveData(object sender, WebSocketSharp.MessageEventArgs e)
        {
            /*
             * {"deviceCode":"EQ001","gatewayId":34923,"msgType":"telemetry","ts":1688088096000,
             * "values":{"taskWeldMode":"0","calcWeldMode":"0","taskId":"ss",
             * "minVoltage":27.04,"maxVoltage":33.88,"minCurrent":490.7,"maxCurrent":524.7}}
             * 
             */

            //电流电压初始值
            //_edgeGatewayModel.CVWMInfo.maxCurrent = 0;
            //_edgeGatewayModel.CVWMInfo.minCurrent = 0;
            //_edgeGatewayModel.CVWMInfo.maxVoltage = 0;
            //_edgeGatewayModel.CVWMInfo.minVoltage = 0;

            //_edgeGatewayModel.CVWMInfo.medianVoltage = 0;
            //_edgeGatewayModel.CVWMInfo.medianCurrent = 0;

            //_edgeGatewayModel.CVWMInfo.maxCurrentexceed = false;
            //_edgeGatewayModel.CVWMInfo.minCurrentexceed = false;
            //_edgeGatewayModel.CVWMInfo.maxVoltageexceed = false;
            //_edgeGatewayModel.CVWMInfo.minVoltageexceed = false;

            //_edgeGatewayModel.CVWMInfo.medianVoltageexceed = false;
            //_edgeGatewayModel.CVWMInfo.medianCurrentexceed = false;

            logger.Info($"算法接口:{DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据:{e.Data}");

            JObject obj = JsonConvert.DeserializeObject<JObject>(e.Data);

            if (obj != null)
            {
                var currentID = obj.Children().FirstOrDefault(x => x.Path == "deviceCode").First.ToString();
                //如果ws推的设备是当前设备
                if (currentID == _edgeGatewayModel.currentID)
                {
                    var values = obj.Children().FirstOrDefault(x => x.Path == "values").First().ToString();

                    var value = JsonConvert.DeserializeObject<JObject>(values);

                    foreach (var item in value.Children())
                    {
                        switch (item.Path.ToString())
                        {
                            case "taskWeldMode":
                                _edgeGatewayModel.CVWMInfo.taskWeldMode = item.First.ToString();
                                break;
                            case "calcWeldMode":
                                _edgeGatewayModel.CVWMInfo.calcWeldMode = item.First.ToString();
                                break;
                            case "taskId":
                                _edgeGatewayModel.CVWMInfo.taskId = item.First.ToString();
                                break;

                            case "minVoltage":

                                decimal minVoltage = 0;
                                decimal.TryParse(item.First.ToString(), out minVoltage);
                                _edgeGatewayModel.CVWMInfo.minVoltage = minVoltage;

                                if (_edgeGatewayModel.HasTask)
                                {
                                    //如果脉冲电压小于 脉冲阈值最小值
                                    if (_edgeGatewayModel.CVWMInfo.minVoltage < _edgeGatewayModel.TaskInfo.voltage_base)
                                    {
                                        if (!_edgeGatewayModel.CVWMInfo.minVoltageexceed)
                                        {
                                            _edgeGatewayModel.CVWMInfo.minVoltageexceed = true;
                                        }
                                    }
                                    else
                                    {
                                        _edgeGatewayModel.CVWMInfo.minVoltageexceed = false;
                                    }
                                }

                                break;
                            case "maxVoltage":

                                decimal maxVoltage = 0;
                                decimal.TryParse(item.First.ToString(), out maxVoltage);

                                _edgeGatewayModel.CVWMInfo.maxVoltage = maxVoltage;

                                if (_edgeGatewayModel.HasTask)
                                {
                                    //如果脉冲电压大于 脉冲阈值最大值
                                    if (_edgeGatewayModel.CVWMInfo.maxVoltage > _edgeGatewayModel.TaskInfo.voltage_peak)
                                    {
                                        if (!_edgeGatewayModel.CVWMInfo.maxVoltageexceed)
                                        {
                                            _edgeGatewayModel.CVWMInfo.maxVoltageexceed = true;

                                        }
                                    }
                                    else
                                    {
                                        _edgeGatewayModel.CVWMInfo.maxVoltageexceed = false;
                                    }
                                }

                                break;
                            case "minCurrent":

                                decimal minCurrent = 0;
                                decimal.TryParse(item.First.ToString(), out minCurrent);

                                _edgeGatewayModel.CVWMInfo.minCurrent = minCurrent;

                                if (_edgeGatewayModel.HasTask)
                                {
                                    //如果脉冲电流小于 脉冲阈值最小值
                                    if (_edgeGatewayModel.CVWMInfo.minCurrent < _edgeGatewayModel.TaskInfo.current_base)
                                    {
                                        if (!_edgeGatewayModel.CVWMInfo.minCurrentexceed)
                                        {
                                            _edgeGatewayModel.CVWMInfo.minCurrentexceed = true;
                                        }
                                    }
                                    else
                                    {
                                        _edgeGatewayModel.CVWMInfo.minCurrentexceed = false;
                                    }
                                }

                                break;
                            case "maxCurrent":

                                decimal maxCurrent = 0;
                                decimal.TryParse(item.First.ToString(), out maxCurrent);

                                _edgeGatewayModel.CVWMInfo.maxCurrent = maxCurrent;

                                if (_edgeGatewayModel.HasTask)
                                {
                                    //如果脉冲电流大于 脉冲阈值最大值
                                    if (_edgeGatewayModel.CVWMInfo.maxCurrent > _edgeGatewayModel.TaskInfo.current_peak)
                                    {
                                        if (!_edgeGatewayModel.CVWMInfo.maxCurrentexceed)
                                        {
                                            _edgeGatewayModel.CVWMInfo.maxCurrentexceed = true;
                                        }

                                    }
                                    else
                                    {
                                        _edgeGatewayModel.CVWMInfo.maxCurrentexceed = false;
                                    }
                                }
                                break;


                            case "medianVoltage":
                            case "averageVoltage":

                                decimal medianVoltage = 0;
                                decimal.TryParse(item.First.ToString(), out medianVoltage);

                                _edgeGatewayModel.CVWMInfo.medianVoltage = medianVoltage;

                                if (_edgeGatewayModel.HasTask)
                                {
                                    //如果恒流电压大于 脉冲阈值最大值或者如果恒流电压小于于 阈值最小值
                                    if (_edgeGatewayModel.CVWMInfo.medianVoltage > _edgeGatewayModel.TaskInfo.voltage_peak
                                     || _edgeGatewayModel.CVWMInfo.medianVoltage < _edgeGatewayModel.TaskInfo.voltage_base)

                                    {
                                        if (!_edgeGatewayModel.CVWMInfo.medianVoltageexceed)
                                        {
                                            _edgeGatewayModel.CVWMInfo.medianVoltageexceed = true;
                                        }
                                    }
                                    else
                                    {
                                        _edgeGatewayModel.CVWMInfo.medianVoltageexceed = false;
                                    }
                                }

                                break;
                            case "medianCurrent":
                            case "averageCurrent":

                                decimal medianCurrent = 0;
                                decimal.TryParse(item.First.ToString(), out medianCurrent);

                                _edgeGatewayModel.CVWMInfo.medianCurrent = medianCurrent;

                                if (_edgeGatewayModel.HasTask)
                                {
                                    //如果恒流电流大于 脉冲阈值最大值或者如果恒流电流小于于 阈值最小值
                                    if (_edgeGatewayModel.CVWMInfo.medianCurrent > _edgeGatewayModel.TaskInfo.current_peak
                                      || _edgeGatewayModel.CVWMInfo.medianCurrent < _edgeGatewayModel.TaskInfo.current_base)
                                    {
                                        if (!_edgeGatewayModel.CVWMInfo.medianCurrentexceed)
                                        {
                                            _edgeGatewayModel.CVWMInfo.medianCurrentexceed = true;
                                        }

                                    }
                                    else
                                    {
                                        _edgeGatewayModel.CVWMInfo.medianCurrentexceed = false;
                                    }
                                }

                                break;
                        }
                    }
                }
            }

            //处理是否需要三色灯报警
            SetLampStatus();
        }

        /// <summary>
        /// 改变灯和喇叭的状态
        /// </summary>
        private void SetLampStatus()
        {
            //选则USB灯
            if (_edgeGatewayModel.LampManageMode == "1")
            {
                //如果灯存在
                if (_lampManageUSB != null)
                {
                    //如果有任务
                    if (_edgeGatewayModel.HasTask)
                    {
                        //喇叭开着
                        if (_lampManageUSB._isUseBuzz)
                        {
                            //电压超电流不超
                            if (!_edgeGatewayModel.IsVoltageAlarm && !_edgeGatewayModel.IsCurrentAlarm)
                            {
                                _lampManageUSB.SetLampStatus(1);
                            }
                            //电流超电压不超
                            else if (!_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
                            {
                                _lampManageUSB.SetLampStatus(2);
                            }
                            //电压和电流都超
                            else if (_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
                            {
                                _lampManageUSB.SetLampStatus(3);
                            }
                            //电流电压都不超
                            else
                            {
                                _lampManageUSB.SetLampStatus(4);
                            }
                        }
                        //喇叭关着
                        else
                        {
                            //电压超电流不超
                            if (!_edgeGatewayModel.IsVoltageAlarm && !_edgeGatewayModel.IsCurrentAlarm)
                            {
                                _lampManageUSB.SetLampStatus(5);
                            }
                            //电流超电压不超
                            else if (!_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
                            {
                                _lampManageUSB.SetLampStatus(6);
                            }
                            //电压和电流都超
                            else if (_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
                            {
                                _lampManageUSB.SetLampStatus(7);
                            }
                            //电流电压都不超
                            else
                            {
                                _lampManageUSB.SetLampStatus(8);
                            }
                        }
                    }
                    else
                    {
                        //关灯，停止报警
                        _lampManageUSB.SetLampStatus(0);
                    }
                }
            }

            else if (_edgeGatewayModel.LampManageMode == "1")
            {
                //如果灯存在
                if (_lampManageWifi != null)
                {
                    //如果有任务
                    if (_edgeGatewayModel.HasTask)
                    {
                        //喇叭开着
                        if (_lampManageWifi._isUseBuzz)
                        {
                            //电压超电流不超
                            if (_edgeGatewayModel.IsVoltageAlarm && !_edgeGatewayModel.IsCurrentAlarm)
                            {
                                _lampManageWifi.SetLampStatus(1);
                            }
                            //电流超电压不超
                            else if (!_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
                            {
                                _lampManageWifi.SetLampStatus(2);
                            }
                            //电压和电流都超
                            else if (_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
                            {
                                _lampManageWifi.SetLampStatus(3);
                            }
                            //电流电压都不超
                            else
                            {
                                _lampManageWifi.SetLampStatus(4);
                            }
                        }
                        //喇叭关着
                        else
                        {
                            //电压超电流不超
                            if (_edgeGatewayModel.IsVoltageAlarm && !_edgeGatewayModel.IsCurrentAlarm)
                            {
                                _lampManageWifi.SetLampStatus(5);
                            }
                            //电流超电压不超
                            else if (!_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
                            {
                                _lampManageWifi.SetLampStatus(6);
                            }
                            //电压和电流都超
                            else if (_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
                            {
                                _lampManageWifi.SetLampStatus(7);
                            }
                            //电流电压都不超
                            else
                            {
                                _lampManageWifi.SetLampStatus(8);
                            }
                        }
                    }
                    else
                    {
                        //关灯，停止报警
                        _lampManageWifi.SetLampStatus(0);
                    }
                }
            }
        }

        /// <summary>
        /// 创建一个服务
        /// </summary>
        /// <returns></returns>
        public static RestService CreateInstance()
        {
            if (_rest == null) _rest = new RestService();
            return _rest;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        private async void Init()
        {
            Start(_rest, "http://127.0.0.1:8005/api");

            await Task.Delay(10000);

            var task = new Task(async () =>
            {
                while (true)
                {
                    //1秒刷新一次任务
                    HttpGetTaskInfo();

                    HttpGetMiot();

                    await Task.Delay(1000);
                }

            });
            task.Start();

            var task2 = new Task(async () =>
            {
                while (true)
                {
                    //三十秒刷新一次平台的接口
                    HttpGetIOTConnect();

                    //初始化获取设备状态
                    HttpGetDeviceInitConnect();

                    await Task.Delay(30000);
                }

            });
            task2.Start();
        }


        /// <summary>
        /// 获取服务信息New
        /// </summary>
        /// <returns></returns>
        public EdgeGatewayModel GetTaskInfoNew()
        {
            logger.Info($"GetTaskInfoNew:{DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据:  {JsonConvert.SerializeObject(_edgeGatewayModel)}");

            return _edgeGatewayModel;
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="service"></param>
        /// <param name="uri"></param>
        public void Start(RestService service, string uri)
        {
            if (_status == ServiceStatus.Closed)
            {
                _webServiceHost = new WebServiceHost(service, new Uri(uri));
                _webServiceHost.Open();
                _status = ServiceStatus.Opened;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Close()
        {
            if (_status == ServiceStatus.Opened)
            {
                _webServiceHost.Close();
                _status = ServiceStatus.Closed;
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        public void Exit()
        {

            Application.Exit();
        }

        /// <summary>
        /// 获取配置参数
        /// </summary>
        /// <returns></returns>
        public string GetControl()
        {
            string str = File.ReadAllText(Application.StartupPath + "\\contaol.json");
            //灯初始化
            //lighmange

            return str.Replace("[", "").Replace("]", "").Replace("\r\n", "").Replace("\\", "").Replace(" ", "");
        }

        public JObject GetControlJobj()
        {
            string str = File.ReadAllText(Application.StartupPath + "\\contaol.json");

            JObject obj = JsonConvert.DeserializeObject<JObject>(str);

            return obj;
        }

        /// <summary>
        /// 获取设备绑定状态
        /// </summary>
        /// <returns></returns>
        public bool IsBinded()
        {
            bool flag;

            if (!File.Exists(Application.StartupPath + "\\DeviceNo.txt"))
            {
                flag = false;
            }
            else
            {
                StreamReader sr = new StreamReader(Application.StartupPath + "\\DeviceNo.txt", false);

                string line = sr.ReadLine();

                //如果文件存在
                if (line == null)
                {
                    flag = false;
                }
                else
                {
                    //设备号
                    _edgeGatewayModel.currentID = line.ToString();
                    flag = true;
                }
            }

            return flag;
        }

        /// <summary>
        /// 获取本地存的设备编号
        /// </summary>
        /// <returns></returns>
        public string GetDevice()
        {
            string str;
            StreamReader sr = new StreamReader(Application.StartupPath + "\\DeviceNo.txt", false);
            str = sr.ReadLine().ToString();
            _edgeGatewayModel.currentID = str;

            sr.Close();

            return str;

        }

        /// <summary>
        /// （作废）写入选中的设备编号
        /// </summary>
        /// <param name="uName"></param>
        public void SetDevice(string uName)
        {
            try
            {
                StreamWriter sw = new StreamWriter(Application.StartupPath + "\\DeviceNo.txt", false);

                _edgeGatewayModel.currentID = uName;

                sw.WriteLine(uName);
                sw.Close();//写入
            }
            catch (Exception ex)
            {

            }

            CloseHandle?.Invoke();
        }

        /// <summary>
        /// 按钮触发开关
        /// </summary>
        public void SetAgreeStatus()
        {
            logger.Info($"{"MuteInfo:" + _edgeGatewayModel.MuteInfo.Mute}");

            _edgeGatewayModel.MuteInfo.Mute = !_edgeGatewayModel.MuteInfo.Mute;

            _lampManageWifi.SetUseBuzz(_edgeGatewayModel.MuteInfo.Mute);

            _lampManageUSB.SetUseBuzz(_edgeGatewayModel.MuteInfo.Mute);
        }

        /// <summary>
        /// 无任务的情况下，获取前端选择的焊接模式
        /// </summary>
        /// <param name="uName"></param>
        public void setHasTaskWeldingMode(string uName)
        {
            _edgeGatewayModel.WeldMode = uName;

            logger.Info("WeldMode:" + _edgeGatewayModel.WeldMode);
        }

        public void setHasTaskLampMode(string uName)
        {
            _edgeGatewayModel.LampManageMode = uName;

            logger.Info("LampManageMode:" + _edgeGatewayModel.LampManageMode);
        }

        public nifiWeldMode getHasTaskWeldingMode()
        {
            /*{ "weldMode":"1"}*/

            nifiWeldMode weldMode = new nifiWeldMode();
            weldMode.weldMode = _edgeGatewayModel.WeldMode;

            return weldMode;
        }

        /// <summary>
        ///  获取任务接口后台
        /// </summary>
        public async void HttpGetTaskInfo()
        {
            /*
             [{"task_id":"B4E3589372844243A60AA7A5D4CEF845","st_no":"流转卡001","op_no":"0001",
            "op_content":"0001[0001工序]","weld_code":"test焊缝001","weld_type":"2","operator_type":"焊工",
            "wo_no":"1101","wps_code":"223","weld_mode":"脉冲模式","task_status":"3","current_min":"100","current_max":"200",
            "voltage_min":"100","voltage_max":"220","current_base":"30",
            "current_peak":"50","voltage_base":"200","voltage_peak":"200"}]
             */
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");//设置请求头

                try
                {
                    var path = configData.Children().FirstOrDefault(x => x.Path == "getByTaskId").First.ToString();
                    //get
                    var url1 = new Uri(path);
                    // response
                    var response = await httpClient.GetAsync(url1);
                    var data = await response.Content.ReadAsStringAsync();
                    //data = "[]";
                    logger.Info($"任务接口: {DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据: {data}");


                    if (data != null && data.Length > 10)
                    {
                        var objs = JsonConvert.DeserializeObject<List<JObject>>(data);

                        if (objs != null && objs.Count > 0)
                        {
                            foreach (var item in objs.Children())
                            {
                                switch (item.Path.ToString())
                                {
                                    case "task_status":
                                        var status = _edgeGatewayModel.TaskInfo.task_status = item.First.ToString();
                                        //进行中
                                        if (status == "1" || status == "3")
                                        {
                                            _edgeGatewayModel.HasTask = true;

                                            _edgeGatewayModel.TaskInfo.IsCalcWeldModeequal = getTaskInfoIsCalcWeldModeequal();
                                        }
                                        break;
                                    case "task_id":
                                        _edgeGatewayModel.TaskInfo.task_id = item.First.ToString();
                                        break;
                                    case "st_no":
                                        _edgeGatewayModel.TaskInfo.st_no = item.First.ToString();
                                        break;
                                    case "op_no":
                                        _edgeGatewayModel.TaskInfo.op_no = item.First.ToString();
                                        break;
                                    case "op_content":
                                        _edgeGatewayModel.TaskInfo.op_content = item.First.ToString();
                                        break;
                                    case "weld_code":
                                        _edgeGatewayModel.TaskInfo.weld_code = item.First.ToString();
                                        break;
                                    case "weld_type":
                                        _edgeGatewayModel.TaskInfo.weld_type = item.First.ToString();
                                        break;
                                    case "operator_type":
                                        _edgeGatewayModel.TaskInfo.operator_type = item.First.ToString();
                                        break;
                                    case "wo_no":
                                        _edgeGatewayModel.TaskInfo.wo_no = item.First.ToString();
                                        break;

                                    case "wps_code":
                                        _edgeGatewayModel.TaskInfo.wps_code = item.First.ToString();
                                        break;
                                    case "weld_mode":
                                        if (item.First.ToString().Contains("脉冲"))
                                        {
                                            _edgeGatewayModel.TaskInfo.weld_mode = "0";

                                        }
                                        else if (item.First.ToString().Contains("恒流"))
                                        {
                                            _edgeGatewayModel.TaskInfo.weld_mode = "1";
                                        }
                                        else
                                        {
                                            _edgeGatewayModel.TaskInfo.weld_mode = "2";
                                        }
                                        break;
                                    case "task_sort":
                                        _edgeGatewayModel.TaskInfo.task_sort = item.First.ToString();
                                        break;
                                    case "op_remarks ":
                                        _edgeGatewayModel.TaskInfo.op_remarks = item.First.ToString();
                                        break;
                                    case "classes":
                                        _edgeGatewayModel.TaskInfo.classes = item.First.ToString();
                                        break;
                                    case "weld_layer":
                                        _edgeGatewayModel.TaskInfo.weld_layer = item.First.ToString();
                                        break;
                                    case "weld_method":
                                        _edgeGatewayModel.TaskInfo.weld_method = item.First.ToString();
                                        break;
                                    case "remarks":
                                        _edgeGatewayModel.TaskInfo.remarks = item.First.ToString();
                                        break;
                                    case "task_create_time":
                                        _edgeGatewayModel.TaskInfo.task_create_time = item.First.ToString();
                                        break;
                                    case "task_updte_time":
                                        _edgeGatewayModel.TaskInfo.task_updte_time = item.First.ToString();
                                        break;

                                    case "wps_current_min":
                                        decimal currentbase = 0;
                                        decimal.TryParse(item.First.ToString(), out currentbase);
                                        _edgeGatewayModel.TaskInfo.current_base = currentbase;

                                        decimal currentmin = 0;
                                        decimal.TryParse(item.First.ToString(), out currentmin);
                                        _edgeGatewayModel.TaskInfo.current_min = currentmin;
                                        break;
                                    case "wps_current_max":
                                        decimal currentpeak = 0;
                                        decimal.TryParse(item.First.ToString(), out currentpeak);
                                        _edgeGatewayModel.TaskInfo.current_peak = currentpeak;

                                        decimal currentmax = 0;
                                        decimal.TryParse(item.First.ToString(), out currentmax);
                                        _edgeGatewayModel.TaskInfo.current_max = currentmax;
                                        break;
                                    case "wps_voltage_min":
                                        decimal voltagebase = 0;
                                        decimal.TryParse(item.First.ToString(), out voltagebase);
                                        _edgeGatewayModel.TaskInfo.voltage_base = voltagebase;

                                        decimal voltagemin = 0;
                                        decimal.TryParse(item.First.ToString(), out voltagemin);
                                        _edgeGatewayModel.TaskInfo.voltage_min = voltagemin;
                                        break;
                                    case "wps_voltage_max":
                                        decimal voltagepeak = 0;
                                        decimal.TryParse(item.First.ToString(), out voltagepeak);
                                        _edgeGatewayModel.TaskInfo.voltage_peak = voltagepeak;

                                        decimal voltagemax = 0;
                                        decimal.TryParse(item.First.ToString(), out voltagemax);
                                        _edgeGatewayModel.TaskInfo.voltage_max = voltagemax;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            // 没有任务
                            _edgeGatewayModel.HasTask = false;

                            ////没有任务的情况下，任务模式一致，“焊接模式不一致”不显示
                            //_edgeGatewayModel.TaskInfo.IsCalcWeldModeequal = true;
                        }
                    }
                    else
                    {
                        // 没有任务
                        _edgeGatewayModel.HasTask = false;

                        ////没有任务的情况下，任务模式一致，“焊接模式不一致”不显示
                        //_edgeGatewayModel.TaskInfo.IsCalcWeldModeequal = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"{"HttpGetTaskInfoError:" + ex.Message}");
                }
            }
        }

        /// <summary>
        /// 算法获取电流电压数据
        /// </summary>
        public async void HttpGetMiot()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");//设置请求头

                /*
                 {
	            "deviceCode": "E0001",
	            "gateayId": 34923,
	            "msglype": "telemetry",
	            "ts": 1688629480000,
	            "values": {
		            "taskheldMode": "0",
		            "taskid": "22222",
		            "minVoltage": 27.0,
		            "maxoltage": 32.68,
		            "minCurrent": 491.8,
		            "maxCurrent": 520.6
	            }
            }
                 */

                //1688632578000
                //1688629480000
                try
                {
                    long time = DateTimeOffset.UtcNow.AddSeconds(-1).ToUnixTimeSeconds() * 1000;

                    var ts = time.ToString();

                    var url = configData.Children().FirstOrDefault(x => x.Path == "getCurrentVoltageWorkMode").First.ToString();
                    //get
                    url = url + "&ts=" + ts;

                    var url1 = new Uri(url);

                    logger.Info($"算法接口请求入地址: {url1}");

                    // response
                    var response = await httpClient.GetAsync(url1);
                    var data = await response.Content.ReadAsStringAsync();

                    logger.Info($"算法接口: {DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据: {data}");

                    if (data != null && data.Length > 10)
                    {

                        var obj = JsonConvert.DeserializeObject<JObject>(data);

                        if (obj != null)
                        {
                            var currentID = obj.Children().FirstOrDefault(x => x.Path == "deviceCode").First.ToString();
                            //如果ws推的设备是当前设备
                            if (currentID == _edgeGatewayModel.currentID)
                            {
                                var values = obj.Children().FirstOrDefault(x => x.Path == "values").First().ToString();

                                var value = JsonConvert.DeserializeObject<JObject>(values);

                                foreach (var item in value.Children())
                                {
                                    switch (item.Path.ToString())
                                    {
                                        case "taskWeldMode":
                                            _edgeGatewayModel.CVWMInfo.taskWeldMode = item.First.ToString();
                                            break;
                                        case "calcWeldMode":
                                            _edgeGatewayModel.CVWMInfo.calcWeldMode = item.First.ToString();
                                            break;
                                        case "taskId":
                                            _edgeGatewayModel.CVWMInfo.taskId = item.First.ToString();
                                            break;

                                        case "minVoltage":

                                            decimal minVoltage = 0;
                                            decimal.TryParse(item.First.ToString(), out minVoltage);
                                            _edgeGatewayModel.CVWMInfo.minVoltage = minVoltage;

                                            if (_edgeGatewayModel.HasTask && _edgeGatewayModel.TaskInfo.voltage_base > 0)
                                            {
                                                //如果脉冲电压小于 脉冲阈值最小值
                                                if (_edgeGatewayModel.CVWMInfo.minVoltage < _edgeGatewayModel.TaskInfo.voltage_base)
                                                {
                                                    if (!_edgeGatewayModel.CVWMInfo.minVoltageexceed)
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.minVoltageexceed = true;
                                                    }
                                                }
                                                else
                                                {
                                                    _edgeGatewayModel.CVWMInfo.minVoltageexceed = false;
                                                }
                                            }
                                            else
                                            {
                                                _edgeGatewayModel.CVWMInfo.minVoltageexceed = false;
                                            }

                                            break;
                                        case "maxVoltage":

                                            decimal maxVoltage = 0;
                                            decimal.TryParse(item.First.ToString(), out maxVoltage);

                                            _edgeGatewayModel.CVWMInfo.maxVoltage = maxVoltage;

                                            if (_edgeGatewayModel.HasTask && _edgeGatewayModel.TaskInfo.voltage_peak > 0)
                                            {
                                                //如果脉冲电压大于 脉冲阈值最大值
                                                if (_edgeGatewayModel.CVWMInfo.maxVoltage > _edgeGatewayModel.TaskInfo.voltage_peak)
                                                {
                                                    if (!_edgeGatewayModel.CVWMInfo.maxVoltageexceed)
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.maxVoltageexceed = true;

                                                    }
                                                }
                                                else
                                                {
                                                    _edgeGatewayModel.CVWMInfo.maxVoltageexceed = false;
                                                }
                                            }
                                            else
                                            {
                                                _edgeGatewayModel.CVWMInfo.maxVoltageexceed = false;
                                            }

                                            break;
                                        case "minCurrent":

                                            decimal minCurrent = 0;
                                            decimal.TryParse(item.First.ToString(), out minCurrent);

                                            _edgeGatewayModel.CVWMInfo.minCurrent = minCurrent;

                                            if (_edgeGatewayModel.HasTask && _edgeGatewayModel.TaskInfo.current_base > 0)
                                            {
                                                //如果脉冲电流小于 脉冲阈值最小值
                                                if (_edgeGatewayModel.CVWMInfo.minCurrent < _edgeGatewayModel.TaskInfo.current_base)
                                                {
                                                    if (!_edgeGatewayModel.CVWMInfo.minCurrentexceed)
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.minCurrentexceed = true;
                                                    }
                                                }
                                                else
                                                {
                                                    _edgeGatewayModel.CVWMInfo.minCurrentexceed = false;
                                                }
                                            }
                                            else
                                            {
                                                _edgeGatewayModel.CVWMInfo.minCurrentexceed = false;
                                            }

                                            break;
                                        case "maxCurrent":

                                            decimal maxCurrent = 0;
                                            decimal.TryParse(item.First.ToString(), out maxCurrent);

                                            _edgeGatewayModel.CVWMInfo.maxCurrent = maxCurrent;

                                            if (_edgeGatewayModel.HasTask && _edgeGatewayModel.TaskInfo.current_peak > 0)
                                            {
                                                //如果脉冲电流大于 脉冲阈值最大值
                                                if (_edgeGatewayModel.CVWMInfo.maxCurrent > _edgeGatewayModel.TaskInfo.current_peak)
                                                {
                                                    if (!_edgeGatewayModel.CVWMInfo.maxCurrentexceed)
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.maxCurrentexceed = true;
                                                    }

                                                }
                                                else
                                                {
                                                    _edgeGatewayModel.CVWMInfo.maxCurrentexceed = false;
                                                }
                                            }
                                            else
                                            {
                                                _edgeGatewayModel.CVWMInfo.maxCurrentexceed = false;
                                            }

                                            break;


                                        case "medianVoltage":
                                        case "averageVoltage":

                                            decimal medianVoltage = 0;
                                            decimal.TryParse(item.First.ToString(), out medianVoltage);

                                            _edgeGatewayModel.CVWMInfo.medianVoltage = medianVoltage;

                                            if (_edgeGatewayModel.HasTask && _edgeGatewayModel.TaskInfo.voltage_peak > 0 && _edgeGatewayModel.TaskInfo.voltage_base > 0)
                                            {
                                                //如果恒流电压大于 脉冲阈值最大值或者如果恒流电压小于于 阈值最小值
                                                if (_edgeGatewayModel.CVWMInfo.medianVoltage > _edgeGatewayModel.TaskInfo.voltage_peak
                                                 || _edgeGatewayModel.CVWMInfo.medianVoltage < _edgeGatewayModel.TaskInfo.voltage_base)

                                                {
                                                    if (!_edgeGatewayModel.CVWMInfo.medianVoltageexceed)
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.medianVoltageexceed = true;
                                                    }
                                                }
                                                else
                                                {
                                                    _edgeGatewayModel.CVWMInfo.medianVoltageexceed = false;
                                                }
                                            }
                                            else
                                            {
                                                _edgeGatewayModel.CVWMInfo.medianVoltageexceed = false;
                                            }

                                            break;
                                        case "medianCurrent":
                                        case "averageCurrent":

                                            decimal medianCurrent = 0;
                                            decimal.TryParse(item.First.ToString(), out medianCurrent);

                                            _edgeGatewayModel.CVWMInfo.medianCurrent = medianCurrent;

                                            if (_edgeGatewayModel.HasTask && _edgeGatewayModel.TaskInfo.current_peak > 0 && _edgeGatewayModel.TaskInfo.current_base > 0)
                                            {
                                                //如果恒流电流大于 脉冲阈值最大值或者如果恒流电流小于于 阈值最小值
                                                if (_edgeGatewayModel.CVWMInfo.medianCurrent > _edgeGatewayModel.TaskInfo.current_peak
                                                  || _edgeGatewayModel.CVWMInfo.medianCurrent < _edgeGatewayModel.TaskInfo.current_base)
                                                {
                                                    if (!_edgeGatewayModel.CVWMInfo.medianCurrentexceed)
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.medianCurrentexceed = true;
                                                    }

                                                }
                                                else
                                                {
                                                    _edgeGatewayModel.CVWMInfo.medianCurrentexceed = false;
                                                }
                                            }
                                            else
                                            {
                                                _edgeGatewayModel.CVWMInfo.medianCurrentexceed = false;
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        //处理是否需要三色灯报警
                        SetLampStatus();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"{"getCurrentVoltageWorkModeError:" + ex.Message}");
                }
            }
        }

        /// <summary>
        ///  获取IOT平台接口后台
        /// </summary>
        public async void HttpGetIOTConnect()
        {
            /*
             {"status":200,"msg":"请求成功","result":true}
             */
            bool isconnect = true;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");//设置请求头

                try
                {
                    var path = configData.Children().FirstOrDefault(x => x.Path == "getIOTConnect").First.ToString();
                    //get
                    var url1 = new Uri(path);
                    // response
                    var response = await httpClient.GetAsync(url1);
                    var data = await response.Content.ReadAsStringAsync();

                    logger.Info($"IOT平台连接状态: {DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据: {data}");

                    if (data != null)
                    {
                        var obj = JsonConvert.DeserializeObject<JObject>(data);

                        if (obj != null)
                        {
                            bool result = obj.Children().FirstOrDefault(x => x.Path == "status").First.ToString() == "200";

                            isconnect = result;

                        }
                    }

                    _edgeGatewayModel.IOTConnectInfo.result = isconnect;
                }
                catch (Exception ex)
                {
                    logger.Error($"{"HttpGetIOTConnectError:" + ex.Message}");
                }
            }
        }

        /// <summary>
        /// 设备连接状态
        /// </summary>
        public async void HttpGetDeviceInitConnect()
        {
            /*
             {
         "status": 200,
         "msg": "设备查询成功",
         "result": {
          "records": [{
           "id": "1123700207420829696",
           "code": "test555",
           "name": "name",
           "protoId": "1032000",
           "readInterval": "100",
           "config": "{\"ip\":\"10.56\",\"port\":\"8193\",\"conn_timeout\":\"5000\"}",
           "state": 0,
           "workState": 0,
           "lastestTime": null,
           "lastestTimeStr": null,
           "online": null,
           "groupId": "0",
           "errorMessage": null,
           "masterCode": null,
           "mprotoVO": null
          }, {
           "id": "1123694585916813312",
           "code": "modbus001-4",
           "name": "modbus001-4",
           "protoId": "1000001",
           "readInterval": "1000,2000",
           "config": "{\"ip\":\"10.90.254.239\",\"port\":\"502\",\"station_id\":\"1\",\"endian\":\"CDAB\",\"start_zero\":true,\"conn_timeout\":\"5000\",\"batchRead\":false}",
           "state": 0,
           "workState": 0,
           "lastestTime": null,
           "lastestTimeStr": "2023-06-29 15:53:31",
           "online": 0,
           "groupId": "1671430297109368833",
           "errorMessage": "",
           "masterCode": "modbus001",
           "mprotoVO": null
          }],!
          "total": "8",
          "size": "999",
          "current": "1",
          "orders": [],
          "optimizeCountSql": true,
          "searchCount": true,
          "maxLimit": null,
          "countId": null,
          "pages": "1"
         }
        }*/
            bool isconnect = true;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");//设置请求头

                try
                {

                    var path = configData.Children().FirstOrDefault(x => x.Path == "getOption").First.ToString();
                    //get
                    var url1 = new Uri(path);
                    // response
                    var response = await httpClient.GetAsync(url1);

                    var data = await response.Content.ReadAsStringAsync();

                    logger.Info($"设备连接状态: {DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据: {data}");

                    if (data != null)
                    {
                        var obj = JsonConvert.DeserializeObject<JObject>(data);

                        if (obj != null)
                        {
                            var status = obj.Children().FirstOrDefault(x => x.Path == "status").First.ToString();

                            if (status == "200")
                            {
                                //result 对象
                                var result = obj.Children().FirstOrDefault(x => x.Path == "result").First.ToString();

                                //result 对象数组
                                var results = JsonConvert.DeserializeObject<JObject>(result).Children().FirstOrDefault().First.ToString();

                                //records数组
                                var records = JsonConvert.DeserializeObject<List<JObject>>(results);

                                foreach (var record in records)
                                {
                                    var device = record.Children().FirstOrDefault(x => x.Path == "masterCode");
                                    if (device.First.ToString() == _edgeGatewayModel.currentID)
                                    {
                                        //都为1
                                        isconnect &= record.Children().FirstOrDefault(x => x.Path == "online").First.ToString() == "1";
                                    }
                                }
                            }
                        }
                    }


                    _edgeGatewayModel.DeviceConnectInfo.online = isconnect;
                }
                catch (Exception ex)
                {
                    logger.Error($"{"IsDeviceConnectIonline:" + ex.Message}");
                }
            }
        }

        /// <summary>
        /// 没有任务，焊接能力选择
        /// </summary>
        public async void HttpQueryByCodeAndType()
        {
            /*
             {
	        "success": true,
	        "message": "",
	        "code": 200,
	        "result": {
		        "id": "1676138507523231746",
		        "equipmentCode": "756-025",
		        "equipmentName": "焊机25",
		        "type": "1",
		        "weldingMode": "3",
		        "networking": 1,
		        "remark": "123123123123123ddddddddddd",
		        "createBy": "admin",
		        "createTime": "2023-07-04 15:58:55",
		        "updateBy": "admin",
		        "updateTime": "2023-07-06 14:47:42",
		        "sysOrgCode": "G50179909"
	        },
	        "timestamp": 1689042888827
        }
        }*/
            string WeldMode = null;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");//设置请求头

                try
                {
                    if (!_edgeGatewayModel.HasTask)
                    {
                        var path = configData.Children().FirstOrDefault(x => x.Path == "queryByCodeAndType").First.ToString();
                        //get
                        var url1 = new Uri(path);
                        // response
                        var response = await httpClient.GetAsync(url1);

                        var data = await response.Content.ReadAsStringAsync();

                        logger.Info($"设备焊接能力: {DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据: {data}");

                        if (data != null)
                        {
                            var obj = JsonConvert.DeserializeObject<JObject>(data);

                            if (obj != null)
                            {
                                var status = obj.Children().FirstOrDefault(x => x.Path == "code").First.ToString();

                                //查到焊接能力
                                if (status == "200")
                                {
                                    //result 对象
                                    var result = obj.Children().FirstOrDefault(x => x.Path == "result").First.ToString();

                                    var weldingMode = JsonConvert.DeserializeObject<JObject>(result).Children().FirstOrDefault(x => x.Path == "weldingMode");

                                    WeldMode = weldingMode.First.ToString();
                                }
                            }
                        }
                        List<string> strlist = new List<string>();

                        //如果设备能力存在
                        if (WeldMode?.Length > 0)
                        {
                            if (WeldMode.IndexOf("1") > -1)
                            {
                                strlist.Add("恒流");
                            }
                            if (WeldMode.IndexOf("2") > -1)
                            {
                                strlist.Add("脉冲");
                            }
                            if (WeldMode.IndexOf("3") > -1)
                            {
                                strlist.Add("埋弧焊");
                            }
                        }

                        _edgeGatewayModel.WeldingMode = strlist;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"{"HttpQueryByCodeAndTypeError:" + ex.Message}");
                }
            }
        }

        /// <summary>
        /// 任务存在的情况下，两边的焊接模式是否一致
        /// </summary>
        /// <returns></returns>
        private bool getTaskInfoIsCalcWeldModeequal()
        {
            bool flag;

            if (_edgeGatewayModel.CVWMInfo.calcWeldMode != null & _edgeGatewayModel.TaskInfo.weld_mode != null)
            {
                flag = _edgeGatewayModel.CVWMInfo.calcWeldMode != _edgeGatewayModel.TaskInfo.weld_mode;
            }
            else
            {
                flag = false;
            }

            return flag;
        }

        /// <summary>
        /// 后台服务状态
        /// </summary>
        private enum ServiceStatus
        {
            Opened,
            Closed
        }
    }
}