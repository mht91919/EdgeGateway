using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            //InitWebSocketDeviceStatus();

            getHasTaskLampMode();

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
        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            Start(_rest, "http://127.0.0.1:8005/api");

            Task.Delay(10000);
            {
                var task = new Task(() =>
                {
                    while (true)
                    {

                        //1秒刷新一次任务
                        HttpGetTaskInfo();


                        Task.Delay(1000).Wait();
                    }

                });
                task.Start();
            }

            {
                var task = new Task(() =>
                {
                    while (true)
                    {
                        HttpGetMiot();

                        Task.Delay(1000).Wait();
                    }

                });
                task.Start();
            }

            {
                var task2 = new Task(() =>
                {
                    while (true)
                    {
                        //三十秒刷新一次平台的接口
                        //HttpGetIOTConnect();

                        HttpGetIOTConnectCpro();

                        Task.Delay(30000).Wait();
                    }

                });
                task2.Start();
            }

            {
                var task2 = new Task(() =>
                {
                    while (true)
                    {
                        //HttpGetDeviceInitConnect();
                        HttpGetDeviceInitConnectCpro();

                        Task.Delay(30000).Wait();
                    }

                });
                task2.Start();
            }

            //{
            //    var task2 = new Task(() =>
            //    {
            //        while (true)
            //        {
            //            HttpXXXXX();

            //            Task.Delay(1000).Wait();
            //        }

            //    });
            //    task2.Start();
            //}

        }

        /// <summary>
        /// 获取服务信息New
        /// </summary>
        /// <returns></returns>
        public EdgeGatewayModel GetTaskInfoNew()
        {
            //logger.Info($"GetTaskInfoNew:{DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据:  {JsonConvert.SerializeObject(_edgeGatewayModel)}");

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

                //如果文件不存在
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
        /// 无任务的情况下，获取UI前端选择的焊接模式
        /// </summary>
        /// <param name="uName">0脉冲，1恒流，2埋弧焊</param>
        public void setHasTaskWeldingMode(string uName)
        {
            try
            {
                StreamWriter sw = new StreamWriter(Application.StartupPath + "\\WeldMode.txt", false);

                _edgeGatewayModel.WeldMode = uName;

                sw.WriteLine(uName);
                sw.Close();//写入
            }
            catch (Exception ex)
            {

            }

            logger.Info("WeldMode:" + _edgeGatewayModel.WeldMode);
        }

        public void setHasTaskLampMode(string uName)
        {
            try
            {
                StreamWriter sw = new StreamWriter(Application.StartupPath + "\\LampManageMode.txt", false);

                _edgeGatewayModel.LampManageMode = uName;

                sw.WriteLine(uName);

                sw.Close();//写入
            }
            catch (Exception ex)
            {

            }

            logger.Info("LampManageMode:" + _edgeGatewayModel.LampManageMode);
        }

        /// <summary>
        /// nifi 获取选择的焊接能力
        /// </summary>
        /// <returns></returns>
        public nifiWeldMode getHasTaskWeldingMode()
        {
            /*{ "weldMode":"1"}*/

            nifiWeldMode weldMode = new nifiWeldMode();

            //如果文件不存在
            if (!File.Exists(Application.StartupPath + "\\WeldMode.txt"))
            {
                weldMode.weldMode = _edgeGatewayModel.WeldMode;
            }
            else
            {
                StreamReader sr = new StreamReader(Application.StartupPath + "\\WeldMode.txt", false);

                string line = sr.ReadLine();

                //如果文件不存在
                if (line == null)
                {
                    weldMode.weldMode = _edgeGatewayModel.WeldMode;
                }
                else
                {
                    //设备能力
                    weldMode.weldMode = line.ToString();
                }

                sr.Close();
            }

            return weldMode;
        }

        /// <summary>
        ///  获取任务接口后台
        /// </summary>
        public void HttpGetTaskInfo()
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
                httpClient.Timeout = new TimeSpan(0, 0, 3);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");//设置请求头

                try
                {
                    var path = configData.Children().FirstOrDefault(x => x.Path == "getByTaskId").First.ToString();
                    //get
                    var url1 = new Uri(path);
                    // response
                    var response = httpClient.GetAsync(url1).Result;
                    var data = response.Content.ReadAsStringAsync().Result;
                    //data = "[]";

                    if (data != null && data.Length > 10)
                    {
                        logger.Info($"任务接口: {DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据: {data}");

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
        public void HttpGetMiot()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 0, 3);
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

                    // response
                    var response = httpClient.GetAsync(url1).Result;
                    var data = response.Content.ReadAsStringAsync().Result;

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
                                            //存在任务，并且任务的阈值大于0，并且脉冲基值电流大于等于10A
                                            if (_edgeGatewayModel.HasTask && _edgeGatewayModel.TaskInfo.voltage_base > 0
                                                && _edgeGatewayModel.CVWMInfo.minCurrent >= 10
                                                )
                                            {
                                                //脉冲电压 小于 预警设定值
                                                if (_edgeGatewayModel.CVWMInfo.minVoltage < _edgeGatewayModel.TaskInfo.earlyvoltage_base)
                                                {
                                                    //并且 脉冲电压 小于 脉冲阈值最小值
                                                    if (_edgeGatewayModel.CVWMInfo.minVoltage < _edgeGatewayModel.TaskInfo.voltage_base)
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.minVoltageexceed = true;
                                                        _edgeGatewayModel.CVWMInfo.minVoltageexceedYujing = false;
                                                    }
                                                    //超预警，不超报警
                                                    else
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.minVoltageexceed = false;
                                                        _edgeGatewayModel.CVWMInfo.minVoltageexceedYujing = true;
                                                    }
                                                }
                                                else
                                                {
                                                    _edgeGatewayModel.CVWMInfo.minVoltageexceed = false;
                                                    _edgeGatewayModel.CVWMInfo.minVoltageexceedYujing = false;
                                                }
                                            }
                                            else
                                            {
                                                _edgeGatewayModel.CVWMInfo.minVoltageexceed = false;
                                                _edgeGatewayModel.CVWMInfo.minVoltageexceedYujing = false;
                                            }
                                            break;
                                        case "maxVoltage":

                                            decimal maxVoltage = 0;
                                            decimal.TryParse(item.First.ToString(), out maxVoltage);

                                            _edgeGatewayModel.CVWMInfo.maxVoltage = maxVoltage;
                                            //存在任务，并且任务的阈值大于0，并且脉冲基值电流大于等于10A
                                            if (_edgeGatewayModel.HasTask && _edgeGatewayModel.TaskInfo.voltage_peak > 0
                                                && _edgeGatewayModel.CVWMInfo.minCurrent >= 10
                                                )
                                            {
                                                //如果脉冲电压大于预警设定值
                                                if (_edgeGatewayModel.CVWMInfo.maxVoltage > _edgeGatewayModel.TaskInfo.earlyvoltage_peak)
                                                {
                                                    //并且 如果脉冲电压 大于 脉冲阈值最大值
                                                    if (_edgeGatewayModel.CVWMInfo.maxVoltage > _edgeGatewayModel.TaskInfo.voltage_peak)
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.maxVoltageexceed = true;
                                                        _edgeGatewayModel.CVWMInfo.maxVoltageexceedYujing = false;
                                                    }
                                                    //超预警，不超报警
                                                    else
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.maxVoltageexceed = false;
                                                        _edgeGatewayModel.CVWMInfo.maxVoltageexceedYujing = true;
                                                    }
                                                }
                                                else
                                                {
                                                    _edgeGatewayModel.CVWMInfo.maxVoltageexceed = false;
                                                    _edgeGatewayModel.CVWMInfo.maxVoltageexceedYujing = false;
                                                }
                                            }
                                            else
                                            {
                                                _edgeGatewayModel.CVWMInfo.maxVoltageexceed = false;
                                                _edgeGatewayModel.CVWMInfo.maxVoltageexceedYujing = false;
                                            }
                                            break;
                                        case "minCurrent":

                                            decimal minCurrent = 0;
                                            decimal.TryParse(item.First.ToString(), out minCurrent);

                                            _edgeGatewayModel.CVWMInfo.minCurrent = minCurrent;
                                            //存在任务，并且任务的阈值大于0，并且脉冲基值电流大于等于10A
                                            if (_edgeGatewayModel.HasTask && _edgeGatewayModel.TaskInfo.current_base > 0
                                                && _edgeGatewayModel.CVWMInfo.minCurrent >= 10
                                                )
                                            {
                                                //脉冲电流 小于 预警
                                                if (_edgeGatewayModel.CVWMInfo.minCurrent < _edgeGatewayModel.TaskInfo.earlycurrent_base)
                                                {
                                                    //并且 脉冲电流 小于 脉冲阈值最小值
                                                    if (_edgeGatewayModel.CVWMInfo.minCurrent < _edgeGatewayModel.TaskInfo.current_base
                                                        )
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.minCurrentexceed = true;
                                                        _edgeGatewayModel.CVWMInfo.minCurrentexceedYujing = false;
                                                    }
                                                    //超预警，不超报警
                                                    else
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.minCurrentexceed = false;
                                                        _edgeGatewayModel.CVWMInfo.minCurrentexceedYujing = true;
                                                    }
                                                }
                                                else
                                                {
                                                    _edgeGatewayModel.CVWMInfo.minCurrentexceed = false;
                                                    _edgeGatewayModel.CVWMInfo.minCurrentexceedYujing = false;
                                                }
                                            }
                                            else
                                            {
                                                _edgeGatewayModel.CVWMInfo.minCurrentexceed = false;
                                                _edgeGatewayModel.CVWMInfo.minCurrentexceedYujing = false;
                                            }
                                            break;

                                        case "maxCurrent":

                                            decimal maxCurrent = 0;
                                            decimal.TryParse(item.First.ToString(), out maxCurrent);

                                            _edgeGatewayModel.CVWMInfo.maxCurrent = maxCurrent;
                                            //存在任务，并且任务的阈值大于0，并且脉冲基值电流大于等于10A
                                            if (_edgeGatewayModel.HasTask && _edgeGatewayModel.TaskInfo.current_peak > 0
                                                 && _edgeGatewayModel.CVWMInfo.minCurrent >= 10)
                                            {
                                                //脉冲电流大于预警
                                                if (_edgeGatewayModel.CVWMInfo.maxCurrent > _edgeGatewayModel.TaskInfo.earlycurrent_peak)
                                                {
                                                    //并且 脉冲电流 大于 脉冲阈值最大值
                                                    if (_edgeGatewayModel.CVWMInfo.maxCurrent > _edgeGatewayModel.TaskInfo.current_peak)
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.maxCurrentexceed = true;
                                                        _edgeGatewayModel.CVWMInfo.maxCurrentexceedYujing = false;
                                                    }
                                                    //超预警，不超报警
                                                    else
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.maxCurrentexceed = false;
                                                        _edgeGatewayModel.CVWMInfo.maxCurrentexceedYujing = true;
                                                    }
                                                }
                                                else
                                                {
                                                    _edgeGatewayModel.CVWMInfo.maxCurrentexceed = false;
                                                    _edgeGatewayModel.CVWMInfo.maxCurrentexceedYujing = false;
                                                }
                                            }
                                            else
                                            {
                                                _edgeGatewayModel.CVWMInfo.maxCurrentexceed = false;
                                                _edgeGatewayModel.CVWMInfo.maxCurrentexceedYujing = false;
                                            }
                                            break;

                                        case "medianVoltage":
                                        case "averageVoltage":

                                            decimal medianVoltage = 0;
                                            decimal.TryParse(item.First.ToString(), out medianVoltage);

                                            _edgeGatewayModel.CVWMInfo.medianVoltage = medianVoltage;
                                            //存在任务，并且任务的峰值基值阈值大于0，并且恒流电流大于等于10A
                                            if (_edgeGatewayModel.HasTask && _edgeGatewayModel.TaskInfo.voltage_peak > 0 && _edgeGatewayModel.TaskInfo.voltage_base > 0
                                                && _edgeGatewayModel.CVWMInfo.medianCurrent >= 10
                                                )
                                            {
                                                //恒流电压 小于 最小值预警,或者恒流电压 大于 最大值预警
                                                if (_edgeGatewayModel.CVWMInfo.medianVoltage > _edgeGatewayModel.TaskInfo.earlyvoltage_max
                                                    || _edgeGatewayModel.CVWMInfo.medianVoltage < _edgeGatewayModel.TaskInfo.earlyvoltage_min)
                                                {
                                                    //并且恒流电压 大于 脉冲阈值最大值，或者恒流电压 小于 阈值最小值
                                                    if (_edgeGatewayModel.CVWMInfo.medianVoltage > _edgeGatewayModel.TaskInfo.voltage_peak
                                                     || _edgeGatewayModel.CVWMInfo.medianVoltage < _edgeGatewayModel.TaskInfo.voltage_base)

                                                    {
                                                        _edgeGatewayModel.CVWMInfo.medianVoltageexceed = true;
                                                        _edgeGatewayModel.CVWMInfo.medianVoltageexceedYujing = false;
                                                    }
                                                    //超预警，不超报警
                                                    else
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.medianVoltageexceed = false;
                                                        _edgeGatewayModel.CVWMInfo.medianVoltageexceedYujing = true;
                                                    }
                                                }
                                                else
                                                {
                                                    _edgeGatewayModel.CVWMInfo.medianVoltageexceed = false;
                                                    _edgeGatewayModel.CVWMInfo.medianVoltageexceedYujing = false;
                                                }
                                            }
                                            else
                                            {
                                                _edgeGatewayModel.CVWMInfo.medianVoltageexceed = false;
                                                _edgeGatewayModel.CVWMInfo.medianVoltageexceedYujing = false;
                                            }
                                            break;

                                        case "medianCurrent":
                                        case "averageCurrent":

                                            decimal medianCurrent = 0;
                                            decimal.TryParse(item.First.ToString(), out medianCurrent);

                                            _edgeGatewayModel.CVWMInfo.medianCurrent = medianCurrent;
                                            //存在任务，并且任务的基值阈值大于0，并且恒流电流大于等于10A
                                            if (_edgeGatewayModel.HasTask && _edgeGatewayModel.TaskInfo.current_peak > 0
                                                && _edgeGatewayModel.TaskInfo.current_base > 0
                                                && _edgeGatewayModel.CVWMInfo.medianCurrent >= 10
                                                )
                                            {
                                                //恒流电流 小于 最小值预警,或者 恒流电流 大于 最大值预警
                                                if (_edgeGatewayModel.CVWMInfo.medianCurrent > _edgeGatewayModel.TaskInfo.earlycurrent_max
                                                    || _edgeGatewayModel.CVWMInfo.medianCurrent < _edgeGatewayModel.TaskInfo.earlycurrent_min)
                                                {
                                                    //并且 恒流电流 大于 阈值最大值 或者 恒流电流 小于 阈值最小值
                                                    if (_edgeGatewayModel.CVWMInfo.medianCurrent > _edgeGatewayModel.TaskInfo.current_peak
                                                      || _edgeGatewayModel.CVWMInfo.medianCurrent < _edgeGatewayModel.TaskInfo.current_base)
                                                    {

                                                        _edgeGatewayModel.CVWMInfo.medianCurrentexceed = true;
                                                        _edgeGatewayModel.CVWMInfo.medianCurrentexceedYujing = false;
                                                    }
                                                    //超预警，不超报警
                                                    else
                                                    {
                                                        _edgeGatewayModel.CVWMInfo.medianCurrentexceed = false;
                                                        _edgeGatewayModel.CVWMInfo.medianCurrentexceedYujing = true;
                                                    }
                                                }
                                                else
                                                {
                                                    _edgeGatewayModel.CVWMInfo.medianCurrentexceed = false;
                                                    _edgeGatewayModel.CVWMInfo.medianCurrentexceedYujing = false;
                                                }
                                            }
                                            else
                                            {
                                                _edgeGatewayModel.CVWMInfo.medianCurrentexceed = false;
                                                _edgeGatewayModel.CVWMInfo.medianCurrentexceedYujing = false;
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
        public void HttpGetIOTConnect()
        {
            /*
             {"status":200,"msg":"请求成功","result":true}
             */
            bool isconnect = true;
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 0, 3);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");//设置请求头

                try
                {
                    var path = configData.Children().FirstOrDefault(x => x.Path == "getIOTConnect").First.ToString();
                    //get
                    var url1 = new Uri(path);
                    // response
                    var response = httpClient.GetAsync(url1).Result;
                    var data = response.Content.ReadAsStringAsync().Result;

                    if (data != null)
                    {

                        logger.Info($"IOT平台连接状态: {DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据: {data}");

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
                    isconnect = false;
                    _edgeGatewayModel.IOTConnectInfo.result = isconnect;

                    logger.Error($"{"HttpGetIOTConnectError:" + ex.Message}");
                }
            }
        }
        public void HttpGetIOTConnectCpro()
        {
            /*
             {"status":200,"msg":"请求成功","result":true}
             */
            bool isconnect = true;
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 0, 3);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");//设置请求头

                try
                {
                    var path = configData.Children().FirstOrDefault(x => x.Path == "getIOTConnectCpro").First.ToString();
                    //get
                    var url1 = new Uri(path);
                    // response
                    var response = httpClient.GetAsync(url1).Result;
                    var data = response.Content.ReadAsStringAsync().Result;

                    if (data != null && data == "OK")
                    {
                        isconnect = true;
                    }
                    else { isconnect = false; }

                    _edgeGatewayModel.IOTConnectInfo.result = isconnect;
                }
                catch (Exception ex)
                {
                    isconnect = false;
                    _edgeGatewayModel.IOTConnectInfo.result = isconnect;

                    logger.Error($"{"HttpGetIOTConnectError:" + ex.Message}");
                }
            }
        }

        /// <summary>
        /// 设备连接状态
        /// </summary>
        public void HttpGetDeviceInitConnect()
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
                httpClient.Timeout = new TimeSpan(0, 0, 3);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");//设置请求头

                logger.Info($"设备连接状态NEW: {DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")}");

                try
                {

                    var path = configData.Children().FirstOrDefault(x => x.Path == "getOption").First.ToString();
                    //get
                    var url1 = new Uri(path);
                    // response
                    var response = httpClient.GetAsync(url1).Result;

                    var data = response.Content.ReadAsStringAsync().Result;

                    if (data != null)
                    {
                        logger.Info($"设备连接状态: {DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据: {data}");

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
                    isconnect = false;
                    _edgeGatewayModel.DeviceConnectInfo.online = isconnect;

                    logger.Error($"{"IsDeviceConnectIonline:" + ex.Message}");
                }
            }
        }

        public void HttpGetDeviceInitConnectCpro()
        {
            bool isconnect = true;
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 0, 3);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");//设置请求头

                logger.Info($"设备连接状态NEW: {DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")}");

                try
                {

                    var path = configData.Children().FirstOrDefault(x => x.Path == "getOptionCpro").First.ToString();
                    //get
                    var url1 = new Uri(path);
                    // response
                    var response = httpClient.GetAsync(url1).Result;

                    var data = response.Content.ReadAsStringAsync().Result;

                    if (data != null)
                    {
                        logger.Info($"设备连接状态: {DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据: {data}");

                        //1 连接状态  0 断开状态
                        if (data.ToString() == "1")
                        {
                            isconnect = true;
                        }
                        else
                        {
                            isconnect = false;
                        }
                    }
                    _edgeGatewayModel.DeviceConnectInfo.online = isconnect;
                }
                catch (Exception ex)
                {
                    isconnect = false;
                    _edgeGatewayModel.DeviceConnectInfo.online = isconnect;

                    logger.Error($"{"IsDeviceConnectIonline:" + ex.Message}");
                }
            }
        }

        /// <summary>
        /// 获取焊接能力
        /// </summary>
        public void HttpQueryByCodeAndType()
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
                httpClient.Timeout = new TimeSpan(0, 0, 3);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");//设置请求头

                try
                {
                    if (!_edgeGatewayModel.HasTask)
                    {
                        var path = configData.Children().FirstOrDefault(x => x.Path == "queryByCodeAndType").First.ToString();
                        //get
                        var url1 = new Uri(path);
                        // response
                        var response = httpClient.GetAsync(url1).Result;

                        var data = response.Content.ReadAsStringAsync().Result;

                        if (data != null)
                        {

                            logger.Info($"设备焊接能力: {DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")} 接收数据: {data}");

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
                            if (WeldMode.IndexOf("0") > -1)
                            {
                                strlist.Add("脉冲");
                            }
                            if (WeldMode.IndexOf("1") > -1)
                            {
                                strlist.Add("恒流");
                            }
                            if (WeldMode.IndexOf("2") > -1)
                            {
                                strlist.Add("埋弧焊");
                            }
                            if (WeldMode.IndexOf("3") > -1)
                            {
                                strlist.Add("埋弧焊-丝");
                            }
                            if (WeldMode.IndexOf("4") > -1)
                            {
                                strlist.Add("埋弧焊-带");
                            }
                        }
                        else
                        {
                            throw new Exception("该设备没有焊接能力，请维护！");
                        }

                        //焊接能力列表
                        _edgeGatewayModel.WeldingMode = strlist;


                        #region 写入WeldMode文件焊接模式，第一条

                        StreamWriter sw = new StreamWriter(Application.StartupPath + "\\WeldMode.txt", false);

                        string first = strlist.FirstOrDefault();

                        switch (first)
                        {
                            case "脉冲":
                                _edgeGatewayModel.WeldMode = "0";
                                break;
                            case "恒流":
                                _edgeGatewayModel.WeldMode = "1";
                                break;
                            case "埋弧焊":
                                _edgeGatewayModel.WeldMode = "2";
                                break;
                            case "埋弧焊-丝":
                                _edgeGatewayModel.WeldMode = "3";
                                break;
                            case "埋弧焊-带":
                                _edgeGatewayModel.WeldMode = "4";
                                break;
                        }

                        sw.WriteLine(_edgeGatewayModel.WeldMode);

                        sw.Close();

                        #endregion
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
        /// 改变灯和喇叭的状态
        /// </summary>
        //private void SetLampStatus()
        //{
        //    //选则USB灯
        //    if (_edgeGatewayModel.LampManageMode == "1")
        //    {
        //        //wifi灯关闭
        //        if (_lampManageWifi != null)
        //        {
        //            //关灯，停止报警
        //            _lampManageWifi.SetLampStatus(0);
        //        }

        //        //如果灯存在
        //        if (_lampManageUSB != null)
        //        {
        //            //如果有任务
        //            if (_edgeGatewayModel.HasTask)
        //            {
        //                //喇叭开着
        //                if (_lampManageUSB._isUseBuzz)
        //                {
        //                    //电压超电流不超
        //                    if (_edgeGatewayModel.IsVoltageAlarm && !_edgeGatewayModel.IsCurrentAlarm)
        //                    {
        //                        _lampManageUSB.SetLampStatus(1);
        //                    }
        //                    //电流超电压不超
        //                    else if (!_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
        //                    {
        //                        _lampManageUSB.SetLampStatus(2);
        //                    }
        //                    //电压和电流都超
        //                    else if (_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
        //                    {
        //                        _lampManageUSB.SetLampStatus(3);
        //                    }
        //                    //电流电压都不超
        //                    else
        //                    {
        //                        _lampManageUSB.SetLampStatus(4);
        //                    }
        //                }
        //                //喇叭关着
        //                else
        //                {
        //                    //电压超电流不超
        //                    if (_edgeGatewayModel.IsVoltageAlarm && !_edgeGatewayModel.IsCurrentAlarm)
        //                    {
        //                        _lampManageUSB.SetLampStatus(5);
        //                    }
        //                    //电流超电压不超
        //                    else if (!_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
        //                    {
        //                        _lampManageUSB.SetLampStatus(6);
        //                    }
        //                    //电压和电流都超
        //                    else if (_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
        //                    {
        //                        _lampManageUSB.SetLampStatus(7);
        //                    }
        //                    //电流电压都不超
        //                    else
        //                    {
        //                        _lampManageUSB.SetLampStatus(8);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //关灯，停止报警
        //                _lampManageUSB.SetLampStatus(0);
        //            }
        //        }
        //    }

        //    //Wifi灯
        //    else if (_edgeGatewayModel.LampManageMode == "0")
        //    {
        //        //本地灯关闭
        //        if (_lampManageUSB != null)
        //        {
        //            _lampManageUSB.SetLampStatus(0);
        //        }

        //        //如果灯存在
        //        if (_lampManageWifi != null)
        //        {
        //            //如果有任务
        //            if (_edgeGatewayModel.HasTask)
        //            {
        //                //喇叭开着
        //                if (_lampManageWifi._isUseBuzz)
        //                {
        //                    //电压超电流不超
        //                    if (_edgeGatewayModel.IsVoltageAlarm && !_edgeGatewayModel.IsCurrentAlarm)
        //                    {
        //                        _lampManageWifi.SetLampStatus(1);
        //                    }
        //                    //电流超电压不超
        //                    else if (!_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
        //                    {
        //                        _lampManageWifi.SetLampStatus(2);
        //                    }
        //                    //电压和电流都超
        //                    else if (_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
        //                    {
        //                        _lampManageWifi.SetLampStatus(3);
        //                    }
        //                    //电流电压都不超
        //                    else
        //                    {
        //                        _lampManageWifi.SetLampStatus(4);
        //                    }
        //                }
        //                //喇叭关着
        //                else
        //                {
        //                    //电压超电流不超
        //                    if (_edgeGatewayModel.IsVoltageAlarm && !_edgeGatewayModel.IsCurrentAlarm)
        //                    {
        //                        _lampManageWifi.SetLampStatus(5);
        //                    }
        //                    //电流超电压不超
        //                    else if (!_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
        //                    {
        //                        _lampManageWifi.SetLampStatus(6);
        //                    }
        //                    //电压和电流都超
        //                    else if (_edgeGatewayModel.IsVoltageAlarm && _edgeGatewayModel.IsCurrentAlarm)
        //                    {
        //                        _lampManageWifi.SetLampStatus(7);
        //                    }
        //                    //电流电压都不超
        //                    else
        //                    {
        //                        _lampManageWifi.SetLampStatus(8);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //关灯，停止报警
        //                _lampManageWifi.SetLampStatus(0);
        //            }
        //        }
        //    }
        //}

        private void SetLampStatus()
        {
            if (_lampManageWifi != null)
            {
                //如果有任务
                if (_edgeGatewayModel.HasTask)
                {
                    //喇叭开着
                    if (_lampManageWifi._isUseBuzz)
                    {
                        //报警,不预警
                        if (_edgeGatewayModel.IsCurrentVoltageAlarm && !_edgeGatewayModel.IsCurrentVoltageAlarmYujing)
                        {
                            logger.Info($"{"喇叭开，报警:" + 1550}");

                            _lampManageWifi.SetLampStatus(1);
                        }

                        //预警，不报警
                        if (_edgeGatewayModel.IsCurrentVoltageAlarmYujing && !_edgeGatewayModel.IsCurrentVoltageAlarm)
                        {
                            logger.Info($"{"喇叭开，预警:" + 1580}");

                            _lampManageWifi.SetLampStatus(2);
                        }

                        //不报警也不预警
                        if (!_edgeGatewayModel.IsCurrentVoltageAlarmYujing && !_edgeGatewayModel.IsCurrentVoltageAlarm)
                        {
                            logger.Info($"{"喇叭开，不报警:" + 1566}");

                            _lampManageWifi.SetLampStatus(4);
                        }
                    }

                    //喇叭关着
                    else
                    {
                        //报警,不预警
                        if (_edgeGatewayModel.IsCurrentVoltageAlarm && !_edgeGatewayModel.IsCurrentVoltageAlarmYujing)
                        {
                            logger.Info($"{"喇叭开，报警:" + 1578}");

                            _lampManageWifi.SetLampStatus(5);
                        }

                        //预警，不报警
                        if (_edgeGatewayModel.IsCurrentVoltageAlarmYujing && !_edgeGatewayModel.IsCurrentVoltageAlarm)
                        {
                            logger.Info($"{"喇叭开，预警:" + 1586}");

                            _lampManageWifi.SetLampStatus(6);
                        }

                        //不报警也不预警
                        if (!_edgeGatewayModel.IsCurrentVoltageAlarmYujing && !_edgeGatewayModel.IsCurrentVoltageAlarm)
                        {
                            logger.Info($"{"喇叭开，不报警:" + 1598}");

                            _lampManageWifi.SetLampStatus(4);
                        }
                    }
                }
                else
                {
                    logger.Info($"{"关灯，停止报警:" + 1603}");
                    //关灯，停止报警
                    _lampManageWifi.SetLampStatus(0);
                }
            }
        }

        /// <summary>
        /// 获取灯的状态
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void getHasTaskLampMode()
        {
            //如果lamp文件不存在。使用本地的灯
            if (!File.Exists(Application.StartupPath + "\\LampManageMode.txt"))
            {
                _edgeGatewayModel.LampManageMode = "1";
            }
            else
            {
                StreamReader sr = new StreamReader(Application.StartupPath + "\\LampManageMode.txt", false);

                string line = sr.ReadLine();

                //如果文件不存在
                if (line == null)
                {
                    _edgeGatewayModel.LampManageMode = "1";
                }
                else
                {
                    //lamp的选择
                    _edgeGatewayModel.LampManageMode = line.ToString();
                }
            }
        }


        public void setEarlyWarning(decimal uName)
        {
            try
            {
                StreamWriter sw = new StreamWriter(Application.StartupPath + "\\EarlyWarning.txt", false);

                _edgeGatewayModel.TaskInfo.earlyWarningS = uName;

                sw.WriteLine(uName);
                sw.Close();//写入
            }
            catch (Exception ex)
            {

            }
        }
        public string getEarlyWarning()
        {
            string str;
            StreamReader sr = new StreamReader(Application.StartupPath + "\\EarlyWarning.txt", false);
            str = sr.ReadLine();
            _edgeGatewayModel.TaskInfo.earlyWarningS = Convert.ToDecimal(str);

            sr.Close();

            return str;
        }

        public string ExitExt(string uName)
        {
            if (uName == "AA")
            {
                Application.Exit();
                return "AA";
            }
            else
            {
                return "密码不正确，请重新输入密码！";
            }
        }

        //public void HttpXXXXX()
        //{
        //    /*
        //     {"status":200,"msg":"请求成功","result":true}
        //     */
        //    bool isconnect = true;
        //    using (var httpClient = new HttpClient())
        //    {
        //        httpClient.Timeout = new TimeSpan(0, 0, 3);
        //        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");//设置请求头

        //        try
        //        {
        //            var path = "http://10.90.18.189:8080/minifi/checkConnect";
        //            //get
        //            var url1 = new Uri(path);
        //            // response
        //            var response = httpClient.GetAsync(url1).Result;
        //            var data = response.Content.ReadAsStringAsync().Result;

        //            var obj = JsonConvert.DeserializeObject<JObject>(data);

        //            if (obj != null)
        //            {
        //                string result = obj.Children().FirstOrDefault(x => x.Path == "status").First.ToString();
        //            }

        //            try
        //            {
        //                StreamWriter sw = new StreamWriter(Application.StartupPath + "\\EarlyWarning.txt", false);

        //                _edgeGatewayModel.earlyWarning = uName;

        //                sw.WriteLine(uName);
        //                sw.Close();//写入
        //            }
        //            catch (Exception ex)
        //            {

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            isconnect = false;
        //            _edgeGatewayModel.IOTConnectInfo.result = isconnect;

        //            logger.Error($"{"HttpGetIOTConnectError:" + ex.Message}");
        //        }
        //    }
        //}



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