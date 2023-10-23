using System.IO;
using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace EdgeGateway
{
    [ServiceContract]
    internal interface IRest
    {
        [OperationContract]
        [WebGet(UriTemplate = "/exit", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void Exit();

        /// <summary>
        /// 关闭带参数
        /// </summary>
        [OperationContract]
        [WebGet(UriTemplate = "/exitExt?name={uName}", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string ExitExt(string uName);



        [OperationContract]
        [WebGet(UriTemplate = "/getControl", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string GetControl();

        /// <summary>
        /// 后端数据输出到前端
        /// </summary>
        /// <returns></returns>

        [OperationContract]
        [WebGet(UriTemplate = "/getTaskInfoNew", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        EdgeGatewayModel GetTaskInfoNew();


        [OperationContract]
        [WebGet(UriTemplate = "/getDevice", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string GetDevice();

        /// <summary>
        /// 设置报警消音按钮状态
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "/setAgreeStatus", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void SetAgreeStatus();

        /// <summary>
        /// 绑定设备（作废）
        /// </summary>
        /// <param name="uName"></param>

        [OperationContract]
        [WebGet(UriTemplate = "/setDevice?name={uName}", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void SetDevice(string uName);

        /// <summary>
        /// 焊接模式
        /// </summary>
        /// <param name="uName"></param>
        [OperationContract]
        [WebGet(UriTemplate = "/setHasTaskWeldingMode?name={uName}", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void setHasTaskWeldingMode(string uName);

        /// <summary>
        /// 三色灯模式
        /// </summary>
        /// <param name="uName"></param>
        [OperationContract]
        [WebGet(UriTemplate = "/setHasTaskLampMode?name={uName}", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void setHasTaskLampMode(string uName);


        /// <summary>
        /// 获取上次三色灯模式
        /// </summary>
        /// <param name="uName"></param>
        [OperationContract]
        [WebGet(UriTemplate = "/getHasTaskLampMode", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void getHasTaskLampMode();

        /// <summary>
        /// 预警数值
        /// </summary>
        /// <param name="uName"></param>
        [OperationContract]
        [WebGet(UriTemplate = "/setEarlyWarning?name={uName}", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void setEarlyWarning(decimal uName);

        /// <summary>
        /// 获取上次预警数值
        /// </summary>
        /// <param name="uName"></param>
        [OperationContract]
        [WebGet(UriTemplate = "/getEarlyWarning", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string getEarlyWarning();

        [OperationContract]
        [WebGet(UriTemplate = "/getHasTaskWeldingMode", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        nifiWeldMode getHasTaskWeldingMode();
    }
}
