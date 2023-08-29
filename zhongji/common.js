function getWeek() {

/*    $('#resTextTime').html(new Date().toLocaleTimeString());*/

    var date = new Date();
    var day = date.getDay();          //  周一返回的是1，周六是6，但是周日是0
    var arr = ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六",];

    return arr[day];
/*    $('#resTextDate').html(new Date().toLocaleDateString() + '&nbsp;' + arr[day]);*/
}

function convert_to_float(a) {

    // 将字符串类型转换为浮点数
    var floatValue = parseFloat(a);

    // 返回浮点值
    return floatValue;
}


function roundFun(value, decimal) {
    //value——值
    //decimal——保留小数位数
    return Math.round(value * Math.pow(10, decimal)) / Math.pow(10, decimal);
}

function exceeding_threshold(a, b) {

    // 将字符串类型转换为浮点数
    var floatValueA = parseFloat(a);

    var floatValueB = parseFloat(a);

    if (floatValueA > floatValueB) {

        return true;
    }
    else {
        return false;
    }
}