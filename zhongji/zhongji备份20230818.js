
//获取当前时间
function getTime() {

    $('#resTextTime').html(new Date().toLocaleTimeString());

    $('#resTextDate').html(new Date().toLocaleDateString() + '&nbsp;' + getWeek());
}

//报警消音按钮
function checkclick() {

    $.ajax({
        method: 'get',
        url: "/api/setAgreeStatus",//路径，就是你要交互的后台的路径
        success: function (data) {    //交互成功后的回调函数，data为返回的内容
        },
        error: function (data) {
            console.log(data);
        }
    })

    var checkimg = document.getElementById("checkimg");
    if ($("#agree").is(':checked')) {
        $("#agree").attr("checked", "unchecked");
        checkimg.src = "./img/mute.png";
        $("#resSilence").css('display', '');
        $("#resMute").css('display', 'none');
    }
    else {
        $("#agree").attr("checked", "checked");
        checkimg.src = "./img/SketchPnge1389b91eaa2f2aa35899e1ecb63d6c1376b820d9d58ca8609dff81f9bd2e362.png";
        $("#resMute").css('display', '');
        $("#resSilence").css('display', 'none');
    }
}

//三色灯（0：wifi，1：USB）
var lamMode = null;
//焊机模式
var weldMode = null;

//主页面切换焊接模式按钮
function mainfunswitchweldMode() {
    $("#_switchMode").css('display', '');
    $("#_switchweldMode").css('display', '');
    $("#_switchLamMode").css('display', 'none');
}

//主页面切换三色灯模式
function mainfunswitchlampMode() {
    $("#_switchMode").css('display', '');
    $("#_switchLamMode").css('display', '');
    $("#_switchweldMode").css('display', 'none');
}

//关闭子页面
function funcloseswitchMode() {
    $("#_switchMode").css('display', 'none');
}


//子页面USB灯确认
function funconfirmswitchLampMode() {

    //console.log("当前灯的状态：" + lamMode + "(0:wifi,1:USB)");


    $.ajax({
        type: "Get",//提交方式，分为get和post两种
        url: "/api/setHasTaskLampMode?name=" + lamMode + "",//路径，就是你要交互的后台的路径

        success: function (data) {    //交互成功后的回调函数，data为返回的内容
        },
        error: function (data) {
            console.log("setHasTaskLampMode Error");
        }
    });

    $("#_switchMode").css('display', 'none');
}

//子页面焊接模式确认
function funconfirmswitchweldMode() {

    if (weldMode == "脉冲") {
        selectIndex = "0";
    } else if (weldMode == "恒流") {
        selectIndex = "1";
    } else if (weldMode == "埋弧焊") {
        selectIndex = "2";
    }

    $.ajax({
        type: "Get",//提交方式，分为get和post两种
        url: "/api/setHasTaskWeldingMode?name=" + selectIndex + "",//路径，就是你要交互的后台的路径

        success: function (data) {    //交互成功后的回调函数，data为返回的内容

        },
        error: function (data) {

            console.log("setHasTaskWeldingMode Error");
        }
    });



    $("#_switchMode").css('display', 'none');
}

//子页面选中三色灯样式改变
function funswitchLamMode(data) {

    lamMode = data;


    if (data === 0) {

        $("#_lampManageWifi").css({ "background-color": "rgba(231, 239, 255, 1)", "border": "1px solid rgba(26, 97, 250, 1)" });
        $('#_lampManageUSB').css({ "background-color": "", "border": "" });
    }
    else {

        $("#_lampManageUSB").css({ "background-color": "rgba(231, 239, 255, 1)", "border": "1px solid rgba(26, 97, 250, 1)" });
        $('#_lampManageWifi').css({ "background-color": "", "border": "" });
    }
}


//子页面选中焊接模式样式改变
function funswitchweldMode(data) {

    weldMode = data;

    $('#_list_1').children().css({ "background-color": "", "border": "" });
    $('#' + data).css({ "background-color": "rgba(231, 239, 255, 1)", "border": "1px solid rgba(26, 97, 250, 1)" });

}


//获取焊接模式列表给子页面
function getHasTaskWeldingMode() {

    $.ajax({
        type: "Get",//提交方式，分为get和post两种
        url: "/api/getTaskInfoNew",//路径，就是你要交互的后台的路径
        success: function (data) {    //交互成功后的回调函数，data为返回的内容

            //console.log("焊接能力个数：" + data.WeldingMode.length);

            for (var i = 0; i < data.WeldingMode.length; i++) {

                $("#_list_1").append('<div class="image-text_4-0 flex-col" style="cursor:pointer;" id="' + data.WeldingMode[i] + '"' + 'onclick="funswitchweldMode(' + '\'' + data.WeldingMode[i] + '\'' + ')" >'
                    + '<img class="label_5-0" referrerpolicy = "no-referrer" + src = "./img/' + data.WeldingMode[i] + '.png"/>'
                    + '<span class="text-group_11-0"> ' + data.WeldingMode[i] + '</span ></div>')
            }
        },
        error: function (data) {

            console.log("getHasTaskWeldingMode Error");
        }
    })
}

//获取设备号
function getDevice() {

    var parameter;

    $.ajax({
        type: "Get",//提交方式，分为get和post两种
        url: "/api/GetDevice",//路径，就是你要交互的后台的路径
        async: false,
        success: function (data) {    //交互成功后的回调函数，data为返回的内容
            $('#resTextDeviceNo').html(data);
            parameter = data;
        },
        error: function (data) {

            console.log("getDevice Error");
        }
    })

    return parameter;
}

//报警消音控制按钮
function getAgreeStatus() {

    $.ajax({
        method: 'get',
        url: "/api/getAgreeStatus",//路径，就是你要交互的后台的路径

        error: function (data) {
            console.log(data);
        }
    })
}

//退出程序
function funExit() {

    //console.log("退出");


    $.ajax({
        method: 'get',
        url: "/api/exit",//路径，就是你要交互的后台的路径

        error: function (data) {
            console.log(data);
        }
    })
}

//获取设备实时状态
//function getDeviceRealConnect() {

//    var resTextDeviceNo = getDevice();//获取已绑定的设备编号

//    var parameter = getControl();

//    var rescheckDeviceConnect = document.getElementById("rescheckDeviceConnect");

//    if (!window.WebSocket) {
//        if (window.MozWebSocket) {
//            window.WebSocket = window.MozWebSocket;
//        } else {
//            $('#resTextDeviceNo').prepend("<p>你的浏览器不支持websocket</p>");
//        }
//    }

//    var wsConnect = new WebSocket(parameter.getDeviceRealConnect);

//    wsConnect.onopen = function (evt) {
//        console.log("建立连接");
//        wsConnect.send("{\"cmd\":\"sub\",\"topic\":\"all_device_state\"}");

//    }
//    wsConnect.onclose = function (evt) {
//        console.log('连接关闭')
//        wsConnect.send("{\"cmd\":\"cancel\",\"topic\":\"all_device_state\"}");
//    }

//    wsConnect.onmessage = function (evt) {

//        console.log(JSON.parse(evt.data));

//        var resrecord = JSON.parse(evt.data).data;

//        var uniqueArray = resrecord.filter(i => i.deviceCode.split("-")[0] === resTextDeviceNo)

//        if (uniqueArray != null) {

//            let flag = uniqueArray.every(i => i.online == 1)

//            if (flag) {

//                console.log(flag)

//                $('#rescheckDeviceConnect').attr("src", "./img/open.png");
//                if (rescheckDeviceConnect.style.visibility == 'hidden') {
//                    rescheckDeviceConnect.style.visibility = 'visible';
//                }
//            }
//            else {
//                $('#rescheckDeviceConnect').attr("src", "./img/close.png");
//            }
//        }

//        console.log(uniqueArray);

//    }

//    wsConnect.onerror = function () {
//        // notify.warn("websocket通信发生错误！");
//        // initWebSocket()
//    }

//    wsConnect.onbeforeunload = function () {
//        wsConnect.close();
//    }
//}

function myfunction() {

    $.ajax({
        type: "Get",//提交方式，分为get和post两种
        url: "/api/getTaskInfoNew",//路径，就是你要交互的后台的路径

        success: function (res) {    //交互成功后的回调函数，data为返回的内容

            if (res.LampManageMode !== undefined) {

                var LampManageMode = res.LampManageMode;

                //console.log("当前选择的三色灯模式（0：Wifi，1：USB）："+LampManageMode);

                if (LampManageMode === "0") {

                    $('#resLampMode').html("网络报警灯");//网络报警灯
                    $('#resLampModelImage').attr("src", "./img/网络报警灯.png");
                }

                else {


                    $('#resLampMode').html("本地报警灯");//本地报警灯
                    $('#resLampModelImage').attr("src", "./img/本地报警灯.png");
                }
            }

            //如果任务存在
            if (res.HasTask === true) {

                //console.log("任务存在");

                if (res.TaskInfo !== undefined) {

                    $('#_task').show();//任务显示

                    $('#_tasknone').hide();//无任务隐藏

                    var taskInfo = res.TaskInfo;

                    if (taskInfo.IsCalcWeldModeequal) {
                        $('#_alert').show();
                    }
                    else {
                        $('#_alert').hide();
                    }


                    //console.log("存在任务，任务状态是（0脉冲，1恒流,2埋弧焊）：" + taskInfo.weld_mode);

                    //console.log("采集到的脉冲电流阈值：" + taskInfo.current_base);
                    //console.log("采集到的脉冲电流阈值：" + taskInfo.current_peak);
                    //console.log("采集到的脉冲电压阈值：" + taskInfo.voltage_base);
                    //console.log("采集到的脉冲电压阈值：" + taskInfo.voltage_peak);

                    //console.log("采集到的恒流电流阈值：" + taskInfo.current_min);
                    //console.log("采集到的恒流电流阈值：" + taskInfo.current_max);
                    //console.log("采集到的恒流电压阈值：" + taskInfo.voltage_min);
                    //console.log("采集到的恒流电压阈值：" + taskInfo.voltage_max);

                    //console.log("模式是否一致（如果结果true，那么就显示这几个字）：" + taskInfo.IsCalcWeldModeequal);


                    $("#current_base").html(taskInfo.current_base);//电流脉冲基值最小值
                    $('#current_peak').html(taskInfo.current_peak);//电流脉冲峰值最大值
                    $('#voltage_base').html(taskInfo.voltage_base);//电压脉冲基值最小值
                    $('#voltage_peak').html(taskInfo.voltage_peak);//电压脉冲峰值最大值

                    $("#current_min").html(taskInfo.current_min);//电流恒流基值最小值
                    $('#current_max').html(taskInfo.current_max);//电流恒流峰值最大值
                    $('#voltage_min').html(taskInfo.voltage_min);//电压恒流基值最小值
                    $('#voltage_max').html(taskInfo.voltage_max);//电压恒流峰值最大值


                    //脉冲
                    if (taskInfo.weld_mode !== undefined && taskInfo.weld_mode != null && taskInfo.weld_mode === "0") {


                        $('#hengliuCurrent').hide();//恒流电流隐藏
                        $('#hengliuVoltage').hide();//恒流电压隐藏

                        $('#maichongCurrent').show();//脉冲电流显示
                        $('#maichongVoltage').show();//脉冲电压显示


                        $('#resModel').html("脉冲模式");//脉冲模式
                        $('#resModelImage').attr("src", "./img/脉冲.png");

                    }
                    //恒流
                    else if (taskInfo.weld_mode==="1"){

                        $('#maichongCurrent').hide();//脉冲电流隐藏
                        $('#maichongVoltage').hide();//脉冲电压隐藏

                        $('#hengliuCurrent').show();//脉冲电流显示
                        $('#hengliuVoltage').show();//脉冲电压显示


                        $('#resModel').html("恒流模式");//恒流模式
                        $('#resModelImage').attr("src", "./img/恒流.png");
                    }
                    else {
                        $('#maichongCurrent').hide();//脉冲电流隐藏
                        $('#maichongVoltage').hide();//脉冲电压隐藏


                        $('#hengliuCurrent').show();//脉冲电流显示
                        $('#hengliuVoltage').show();//脉冲电压显示


                        $('#resModel').html("埋弧焊模式");//埋弧焊模式
                        $('#resModelImage').attr("src", "./img/埋弧焊.png");
                    }

                    $('#st_no').html(taskInfo.st_no);//流转卡号
                    $('#op_no').html(taskInfo.op_no);//工序号
                    $('#op_content').html(taskInfo.op_content);//工序内容
                    $('#op_remarks').html(taskInfo.op_remarks);//工序备注


                    $('#weld_code').html(taskInfo.weld_code);//焊缝代号
                    $('#weld_type').html(taskInfo.weld_type);//焊缝类型
                    $('#operator_type').html(taskInfo.operator_type);//人员类型

                    $('#wo_no').html(taskInfo.wo_no);//WPS编号
                    $('#wps_code').html(taskInfo.wps_code);//WPS编号
                    $('#classes').html(taskInfo.classes);//班次

                    $('#task_sort').html(taskInfo.task_sort);//任务分离
                    $('#weld_method').html(taskInfo.weld_method);//焊接方法
                    $('#task_sort').html(taskInfo.weld_layer);//焊层/道

                    $('#remarks').html(taskInfo.remarks);//备注

                    $('#task_create_time').html(taskInfo.task_create_time);//创建时间
                    $('#task_updte_time').html(taskInfo.task_updte_time);//更新时间

                }
                else {
                    $('#_task').hide();//任务隐藏
                    $('#_tasknone').show();//无任务显示

                    //console.log("没有任务");

                    $('#_alert').hide();

                    //没有任务，取采集的焊接模式
                    if (res.CVWMInfo !== undefined) {
                        var CVWMInfo = res.CVWMInfo;

                        //console.log("没有任务，算法给的焊接模式（0脉冲，1恒流，2埋弧焊）：" + CVWMInfo.calcWeldMode)
                        //如果采集的模式不为空
                        if (CVWMInfo.calcWeldMode != undefined) {
                            //脉冲
                            if (CVWMInfo.calcWeldMode === "0") {

                                $('#hengliuCurrent').hide();//恒流电流隐藏
                                $('#hengliuVoltage').hide();//恒流电压隐藏

                                $('#maichongCurrent').show();//脉冲电流显示
                                $('#maichongVoltage').show();//脉冲电压显示

                                $('#resModel').html("脉冲模式");//脉冲模式
                                $('#resModelImage').attr("src", "./img/脉冲.png");

                            }
                            //恒流
                            else if (CVWMInfo.calcWeldMode === "1"){
                                $('#maichongCurrent').hide();//脉冲电流隐藏
                                $('#maichongVoltage').hide();//脉冲电压隐藏


                                $('#hengliuCurrent').show();//脉冲电流显示
                                $('#hengliuVoltage').show();//脉冲电压显示


                                $('#resModel').html("恒流模式");//恒流模式
                                $('#resModelImage').attr("src", "./img/恒流.png");

                            }
                            else {
                                $('#maichongCurrent').hide();//脉冲电流隐藏
                                $('#maichongVoltage').hide();//脉冲电压隐藏


                                $('#hengliuCurrent').show();//脉冲电流显示
                                $('#hengliuVoltage').show();//脉冲电压显示


                                $('#resModel').html("埋弧焊模式");//埋弧焊模式
                                $('#resModelImage').attr("src", "./img/埋弧焊.png");
                            }
                        }
                    }
                }
            }
            //任务不存在
            else {

                $('#_task').hide();//任务隐藏
                $('#_tasknone').show();//无任务显示

                //console.log("手工选择焊接模式（0脉冲，1恒流，2埋弧焊）：" + res.WeldMode)


                $("#current_base").html(null);//电流脉冲基值最小值
                $('#current_peak').html(null);//电流脉冲峰值最大值
                $('#voltage_base').html(null);//电压脉冲基值最小值
                $('#voltage_peak').html(null);//电压脉冲峰值最大值

                $("#current_min").html(null);//电流恒流基值最小值
                $('#current_max').html(null);//电流恒流峰值最大值
                $('#voltage_min').html(null);//电压恒流基值最小值
                $('#voltage_max').html(null);//电压恒流峰值最大值


                $('#_alert').hide();

                //没有任务，取采集的焊接模式
                if (res.CVWMInfo !== undefined) {

                    var CVWMInfo = res.CVWMInfo;

                    //如果采集的模式不为空
                    if (CVWMInfo.calcWeldMode != undefined) {

                        //console.log("没有任务的情况下，采集的焊机模式（0脉冲，1恒流）:" + CVWMInfo.calcWeldMode)

                        //脉冲
                        if (CVWMInfo.calcWeldMode === "0") {

                            $('#hengliuCurrent').hide();//恒流电流隐藏
                            $('#hengliuVoltage').hide();//恒流电压隐藏

                            $('#maichongCurrent').show();//脉冲电流显示
                            $('#maichongVoltage').show();//脉冲电压显示

                            $('#resModel').html("脉冲模式");//脉冲模式
                            $('#resModelImage').attr("src", "./img/脉冲.png");
                        }
                        //恒流
                        else if (CVWMInfo.calcWeldMode === "1") {

                            $('#maichongCurrent').hide();//脉冲电流隐藏
                            $('#maichongVoltage').hide();//脉冲电压隐藏


                            $('#hengliuCurrent').show();//脉冲电流显示
                            $('#hengliuVoltage').show();//脉冲电压显示


                            $('#resModel').html("恒流模式");//恒流模式
                            $('#resModelImage').attr("src", "./img/恒流.png");
                        } else {
                            $('#maichongCurrent').hide();//脉冲电流隐藏
                            $('#maichongVoltage').hide();//脉冲电压隐藏


                            $('#hengliuCurrent').show();//脉冲电流显示
                            $('#hengliuVoltage').show();//脉冲电压显示


                            $('#resModel').html("埋弧焊模式");//恒流模式
                            $('#resModelImage').attr("src", "./img/埋弧焊.png");
                        }

                    }
                    //如果采集的模式为空，那么显示脉冲的
                    else {

                        $('#hengliuCurrent').hide();//恒流电流隐藏
                        $('#hengliuVoltage').hide();//恒流电压隐藏

                        $('#maichongCurrent').show();//脉冲电流显示
                        $('#maichongVoltage').show();//脉冲电压显示

                        $('#resModel').html("脉冲模式");//脉冲模式
                        $('#resModelImage').attr("src", "./img/脉冲.png");
                    }
                }
                //如果采集也停了（显示脉冲的）
                else {

                    //console.log("没有任务,采集也停了的情况下");

                    $('#hengliuCurrent').hide();//恒流电流隐藏
                    $('#hengliuVoltage').hide();//恒流电压隐藏

                    $('#maichongCurrent').show();//脉冲电流显示
                    $('#maichongVoltage').show();//脉冲电压显示

                    $('#resModel').html("脉冲模式");//脉冲模式
                    $('#resModelImage').attr("src", "./img/脉冲.png");
                }
            }

            //如果平台状态能抓到
            if (res.IOTConnectInfo !== undefined) {

                var th = undefined;

                var rescheckIOTConnect = document.getElementById("rescheckIOTConnect");

                if (res.IOTConnectInfo.result !== undefined && res.IOTConnectInfo.result === true) {

                    clearInterval(th);


                    $('#rescheckIOTConnect').attr("src", "./img/open.png");
                }
                //获取到的状态是False
                else {

                    console.log("IOTResultError：" + res.IOTConnectInfo.result);

                    $('#rescheckIOTConnect').attr("src", "./img/close.png");

                    //th = setInterval(function () {


                    //    if (rescheckIOTConnect.style.visibility == 'hidden') {
                    //        rescheckIOTConnect.style.visibility = 'visible';
                    //    } else {
                    //        rescheckIOTConnect.style.visibility = 'hidden';
                    //    }
                    //}, 1000);
                }
            }

            //电流电压焊接模式
            if (res.CVWMInfo !== undefined) {

                var CVWMInfo = res.CVWMInfo;

                //脉冲峰值电流
                if (CVWMInfo.maxCurrent !== undefined && CVWMInfo.maxCurrent != null) {

                    var maxCurrent = convert_to_float(CVWMInfo.maxCurrent).toFixed(1);
                }
                else {
                    var maxCurrent = 0.0;
                }

                //脉冲基值电流
                if (CVWMInfo.minCurrent !== undefined && CVWMInfo.minCurrent != null) {
                    var minCurrent = convert_to_float(CVWMInfo.minCurrent).toFixed(1);
                }
                else {
                    var minCurrent = 0.0;
                }
                //脉冲峰值电压
                if (CVWMInfo.maxVoltage !== undefined && CVWMInfo.maxVoltage != null) {
                    var maxVoltage = convert_to_float(CVWMInfo.maxVoltage).toFixed(1);
                }
                else {
                    var maxVoltage = 0.0;
                }

                //脉冲基值电压
                if (CVWMInfo.minVoltage !== undefined && CVWMInfo.minVoltage != null) {
                    var minVoltage = convert_to_float(CVWMInfo.minVoltage).toFixed(1);
                }
                else {
                    var minVoltage = 0.0;
                }

                //恒流电流
                if (CVWMInfo.medianCurrent !== undefined && CVWMInfo.medianCurrent != null) {
                    var medianCurrent = convert_to_float(CVWMInfo.medianCurrent).toFixed(1);
                }
                else {
                    var medianCurrent = 0.0;
                }

                //恒流电压
                if (CVWMInfo.medianVoltage !== undefined && CVWMInfo.medianVoltage != null) {
                    var medianVoltage = convert_to_float(CVWMInfo.medianVoltage).toFixed(1);
                }
                else {
                    var medianVoltage = 0.0;
                }

                //脉冲峰值电流
                var stringmaxCurrent = maxCurrent.toString();
                var nummaxCurrent = parseInt(stringmaxCurrent.substring(0, stringmaxCurrent.indexOf('.')));
                var decmaxCurrent = stringmaxCurrent.replace(/\d+\.(\d*)/, '$1')

                //脉冲基值电流
                var stringminCurrent = minCurrent.toString();
                var numminCurrent = parseInt(stringminCurrent.substring(0, stringminCurrent.indexOf('.')));
                var decminCurren = stringminCurrent.replace(/\d+\.(\d*)/, '$1')

                //脉冲峰值电压
                var stringmaxVoltage = maxVoltage.toString();
                var nummaxVoltage = parseInt(stringmaxVoltage.substring(0, stringmaxVoltage.indexOf('.')));
                var decmaxVoltage = stringmaxVoltage.replace(/\d+\.(\d*)/, '$1')

                //脉冲基值电压
                var stringminVoltage = minVoltage.toString();
                var numminVoltage = parseInt(stringminVoltage.substring(0, stringminVoltage.indexOf('.')));
                var decminVoltage = stringminVoltage.replace(/\d+\.(\d*)/, '$1')

                //恒流电流
                var stringmedianCurrent = medianCurrent.toString();
                var nummedianCurrent = parseInt(stringmedianCurrent.substring(0, stringmedianCurrent.indexOf('.')));
                var decmedianCurrent = stringmedianCurrent.replace(/\d+\.(\d*)/, '$1');

                //恒流电压
                var stringmedianVoltage = medianVoltage.toString();
                var nummedianVoltage = parseInt(stringmedianVoltage.substring(0, stringmedianVoltage.indexOf('.')));
                var decmedianVoltage = stringmedianVoltage.replace(/\d+\.(\d*)/, '$1');

                $('#resCurrentPeakInteger').html(nummaxCurrent);
                $('#resCurrentPeakDecimal').html("." + decmaxCurrent);

                $('#resCurrentBaseInteger').html(numminCurrent);
                $('#resCurrentBaseDecimal').html("." + decminCurren);


                $('#resVoltagePeakInteger').html(nummaxVoltage);
                $('#resVoltagePeakDecimal').html("." + decmaxVoltage);

                $('#resVoltageBaseInteger').html(numminVoltage);
                $('#resVoltageBaseDecimal').html("." + decminVoltage);


                $('#reshengliuAinteger').html(nummedianCurrent);
                $('#reshengliuAintegerDecimal').html("." + decmedianCurrent);

                $('#reshengliuVinteger').html(nummedianVoltage);
                $('#reshengliuVintegerDecimal').html("." + decmedianVoltage);

                //console.log("采集到的脉冲电流峰值：" + stringmaxCurrent);
                //console.log("采集到的脉冲电流基值：" + stringminCurrent);
                //console.log("采集到的脉冲电压峰值：" + stringmaxVoltage);
                //console.log("采集到的脉冲电压基值：" + stringminVoltage);

                //console.log("采集到的恒流电流：" + stringmedianCurrent);
                //console.log("采集到的恒流电压：" + stringmedianVoltage);

                //脉冲电流峰值
                if (CVWMInfo.maxCurrentexceed) {

                    document.getElementById("resCurrentPeakInteger").style.color = "rgba(255, 15, 51, 1)";
                    document.getElementById("resCurrentPeakDecimal").style.color = "rgba(255, 15, 51, 1)";
                }
                else {

                    document.getElementById("resCurrentPeakInteger").style.color = null;
                    document.getElementById("resCurrentPeakDecimal").style.color = null;
                }

                //脉冲电流基值
                if (CVWMInfo.minCurrentexceed) {
                    document.getElementById("resCurrentBaseInteger").style.color = "rgba(255, 15, 51, 1)";
                    document.getElementById("resCurrentBaseDecimal").style.color = "rgba(255, 15, 51, 1)";
                }
                else {
                    document.getElementById("resCurrentBaseInteger").style.color = null;
                    document.getElementById("resCurrentBaseDecimal").style.color = null;

                }
                //脉冲脉冲电压峰值
                if (CVWMInfo.maxVoltageexceed) {

                    document.getElementById("resVoltagePeakInteger").style.color = "rgba(255, 15, 51, 1)";
                    document.getElementById("resVoltagePeakDecimal").style.color = "rgba(255, 15, 51, 1)";
                }
                else {
                    document.getElementById("resVoltagePeakInteger").style.color = null;
                    document.getElementById("resVoltagePeakDecimal").style.color = null;
                }
                //脉冲电压基值
                if (CVWMInfo.minVoltageexceed) {
                    document.getElementById("resVoltageBaseInteger").style.color = "rgba(255, 15, 51, 1)";
                    document.getElementById("resVoltageBaseDecimal").style.color = "rgba(255, 15, 51, 1)";
                }
                else {
                    document.getElementById("resVoltageBaseInteger").style.color = null;
                    document.getElementById("resVoltageBaseDecimal").style.color = null;

                }
                //恒流，埋弧焊电流
                if (CVWMInfo.medianCurrentexceed) {
                    document.getElementById("reshengliuAinteger").style.color = "rgba(255, 15, 51, 1)";
                    document.getElementById("reshengliuAintegerDecimal").style.color = "rgba(255, 15, 51, 1)";
                }
                else {
                    document.getElementById("reshengliuAinteger").style.color = null;
                    document.getElementById("reshengliuAintegerDecimal").style.color = null;
                }
                //恒流，埋弧焊电压
                if (CVWMInfo.medianVoltageexceed) {
                    document.getElementById("reshengliuVinteger").style.color = "rgba(255, 15, 51, 1)";
                    document.getElementById("reshengliuVintegerDecimal").style.color = "rgba(255, 15, 51, 1)";
                }
                else {
                    document.getElementById("reshengliuVinteger").style.color = null;
                    document.getElementById("reshengliuVintegerDecimal").style.color = null;
                }
            }
            //报警消音按钮状态
            if (res.MuteInfo !== undefined) {
                var MuteInfo = res.MuteInfo;

                var checkimg = document.getElementById("checkimg");

                //报警开启
                if (MuteInfo.Mute) {

                    $("#agree").attr("checked", "unchecked");
                    checkimg.src = "./img/mute.png";
                    $("#resSilence").css('display', '');
                    $("#resMute").css('display', 'none');
                }
                //报警关闭状态
                else {

                    $("#agree").attr("checked", "checked");
                    checkimg.src = "./img/SketchPnge1389b91eaa2f2aa35899e1ecb63d6c1376b820d9d58ca8609dff81f9bd2e362.png";
                    $("#resMute").css('display', '');
                    $("#resSilence").css('display', 'none');
                }
            }

            //设备连接状态
            if (res.DeviceConnectInfo !== undefined) {

                var th = undefined;

                var DeviceConnectInfo = res.DeviceConnectInfo;

                var rescheckDeviceConnect = document.getElementById("rescheckDeviceConnect");


                if (DeviceConnectInfo.online !== undefined && DeviceConnectInfo.online) {

                    clearInterval(th);

                    $('#rescheckDeviceConnect').attr("src", "./img/open.png");
                }
                else {

                    $('#rescheckDeviceConnect').attr("src", "./img/close.png");

                    //th = setInterval(function () {

                    //    if (rescheckDeviceConnect.style.visibility == 'hidden') {
                    //        rescheckDeviceConnect.style.visibility = 'visible';
                    //    } else {
                    //        rescheckDeviceConnect.style.visibility = 'hidden';
                    //    }
                    //}, 1000);
                }

            }
        },
        error: function (data) {

            console.log("myfunction Error");
        }
    })
}