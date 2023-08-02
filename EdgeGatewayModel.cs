using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeGateway
{
    public class EdgeGatewayModel
    {
        /// <summary>
        /// 当前绑定的设备编码
        /// </summary>
        public string currentID { get; set; }

        /// <summary>
        /// 当前是否有任务
        /// </summary>
        public bool HasTask { get; set; } = true;


        /// <summary>
        /// 当前三色灯的模式
        /// 0:Wifi
        /// 1:USB
        /// </summary>
        public string LampManageMode { get; set; }

        /// <summary>
        /// 无任务时前端选择的焊接模式
        /// 0:脉冲
        /// 1:恒流
        /// 2:埋弧焊
        /// </summary>
        public string WeldMode { get; set; }


        /// <summary>
        /// 设备的焊接能力
        /// 恒流1，
        /// 脉冲2，
        /// 埋弧焊3
        /// </summary>
        public List<string> WeldingMode { get; set; } = new List<string>();

        //TODO:上下限报警用或门？
        /// <summary>
        /// 电流的上限下限报警
        /// </summary>
        public bool IsCurrentAlarm
        {
            get
            {
                return CVWMInfo.medianCurrentexceed || CVWMInfo.maxCurrentexceed || CVWMInfo.minCurrentexceed;
            }
        }

        /// <summary>
        /// 电压的上下限报警
        /// </summary>
        public bool IsVoltageAlarm
        {
            get
            {
                return CVWMInfo.medianVoltageexceed || CVWMInfo.maxVoltageexceed || CVWMInfo.minVoltageexceed;
            }
        }


        /// <summary>
        /// 任务模块
        /// </summary>
        public TaskInfo TaskInfo { get; set; } = new TaskInfo();
        /// <summary>
        /// 平台状态
        /// </summary>
        public IOTConnectInfo IOTConnectInfo { get; set; } = new IOTConnectInfo();

        /// <summary>
        /// ws电流电压模式
        /// </summary>
        public CVWMInfo CVWMInfo { get; set; } = new CVWMInfo();

        /// <summary>
        /// 消音按钮状态
        /// </summary>
        public MuteInfo MuteInfo { get; set; } = new MuteInfo();

        /// <summary>
        /// 设备连接状态
        /// </summary>
        public DeviceConnectInfo DeviceConnectInfo { get; set; } = new DeviceConnectInfo();
    }

    /// <summary>
    /// 当前任务模块
    /// </summary>
    public class TaskInfo
    {
        /// <summary>
        /// 是否显示“模式不一致”默认是false，如果结果true，那么就显示这几个字
        /// </summary>
        public bool IsCalcWeldModeequal { get; set; }

        /// <summary>
        /// 焊接状态（0-未开始，1-进行中，2-已完成，3-已暂停，9-已删除）
        /// </summary>
        public string task_status { get; set; }

        /// <summary>
        /// taskID
        /// </summary>
        public string task_id { get; set; }
        /// <summary>
        /// 流转卡号
        /// </summary>
        public string st_no { get; set; }

        /// <summary>
        /// 工序号
        /// </summary>
        public string op_no { get; set; }
        /// <summary>
        /// 工序内容
        /// </summary>
        public string op_content { get; set; }

        /// <summary>
        /// 焊缝代号
        /// </summary>
        public string weld_code { get; set; }

        /// <summary>
        /// 焊缝类型
        /// </summary>
        public string weld_type { get; set; }

        /// <summary>
        /// 人员类型
        /// </summary>
        public string operator_type { get; set; }
        /// <summary>
        /// 工作令号
        /// </summary>
        public string wo_no { get; set; }

        /// <summary>
        /// WPS编号
        /// </summary>
        public string wps_code { get; set; }

        /// <summary>
        /// 焊接模式：0:脉冲，:1恒流，2，埋弧焊
        /// </summary>
        public string weld_mode { get; set; }

        /// <summary>
        /// 任务分类
        /// </summary>
        public string task_sort { get; set; }

        /// <summary>
        /// 工序备注
        /// </summary>
        public string op_remarks { get; set; }
        /// <summary>
        /// 班次
        /// </summary>
        public string classes { get; set; }

        /// <summary>
        /// 焊层/道
        /// </summary>
        public string weld_layer { get; set; }

        /// <summary>
        /// 焊接方法
        /// </summary>
        public string weld_method { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remarks { get; set; }

        /// <summary>
        /// 任务创建
        /// </summary>
        public string task_create_time { get; set; }

        /// <summary>
        /// 任务更新
        /// </summary>
        public string task_updte_time { get; set; }


        /// <summary>
        /// 电流阈值最小值
        /// </summary>
        public decimal current_min { get; set; }

        /// <summary>
        ///  电流阈值最大值
        /// </summary>
        public decimal current_max { get; set; }

        /// <summary>
        /// 电压阈值最小值
        /// </summary>
        public decimal voltage_min { get; set; }
        /// <summary>
        /// 电压阈值最大值
        /// </summary>
        public decimal voltage_max { get; set; }


        /// <summary>
        /// 电流阈基值最小值
        /// </summary>
        public decimal current_base { get; set; }

        /// <summary>
        ///  电流峰值最大值
        /// </summary>
        public decimal current_peak { get; set; }

        /// <summary>
        /// 电压基值最小值
        /// </summary>
        public decimal voltage_base { get; set; }
        /// <summary>
        /// 电压峰值最大值
        /// </summary>

        public decimal voltage_peak { get; set; }

    }

    /// <summary>
    /// IOT平台链接状态模块
    /// </summary>
    public class IOTConnectInfo
    {
        /// <summary>
        /// 请求状态（200,500）
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 成功失败
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// true、false
        /// </summary>
        public bool result { get; set; }
    }

    /// <summary>
    /// 电流电压焊接模式
    /// </summary>
    public class CVWMInfo
    {
        /// <summary>
        /// 任务焊接模式（焊接：0，恒流：1）
        /// </summary>
        public string taskWeldMode { get; set; }
        /// <summary>
        /// 计算焊接模式（焊接：0，恒流：1）
        /// </summary>
        public string calcWeldMode { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public string taskId { get; set; }

        /// <summary>
        /// 脉冲峰值电流
        /// </summary>
        public decimal maxCurrent { get; set; }
        public bool maxCurrentexceed { get; set; }
        /// <summary>
        /// 脉冲基值电流
        /// </summary>
        public decimal minCurrent { get; set; }
        public bool minCurrentexceed { get; set; }
        /// <summary>
        /// 脉冲峰值电压
        /// </summary>
        public decimal maxVoltage { get; set; }
        public bool maxVoltageexceed { get; set; }
        /// <summary>
        /// 脉冲基值电压
        /// </summary>
        public decimal minVoltage { get; set; }
        public bool minVoltageexceed { get; set; }


        /// <summary>
        /// 恒流电流
        /// </summary>
        public decimal medianCurrent { get; set; }
        public bool medianCurrentexceed { get; set; }
        /// <summary>
        /// 恒流电压
        /// </summary>
        public decimal medianVoltage { get; set; }
        public bool medianVoltageexceed { get; set; }
    }

    /// <summary>
    /// 消音按钮启用停用（true：消音按钮开，可以报警；false 消音按钮关闭，报警失效）
    /// </summary>
    public class MuteInfo
    {
        public bool Mute { get; set; }
    }

    /// <summary>
    /// 设备连接状态
    /// </summary>
    public class DeviceConnectInfo
    {
        public bool online { get; set; }
    }

    public class nifiWeldMode
    {
        /// <summary>
        /// 无任务时前端选择的焊接模式(0脉冲，1恒流，2埋弧焊)
        /// </summary>
        public string weldMode { get; set; } = "0";
    }
}
