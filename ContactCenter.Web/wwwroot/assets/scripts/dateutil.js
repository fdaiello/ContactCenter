function getDateObj(ds) {
    var a = ds.split("T");
    var b = a[0].split("-");
    var c = a[1].split(".")[0].split(":");
    return new Date(b[0], b[1] - 1, b[2], c[0], c[1], c[2]);
}
function pad(s) { return (s < 10) ? '0' + s : s; }
function getValitDateFormat(date) {
    var day = date.getDate();       // yields date
    var month = date.getMonth() + 1;    // yields month (add one as '.getMonth()' is zero indexed)
    var year = date.getFullYear();  // yields year
    var hour = date.getHours();     // yields hours
    var minute = date.getMinutes(); // yields minutes
    var second = date.getSeconds(); // yields seconds
    // After this construct a string with the above results as below
    var time = pad(day) + "/" + pad(month) + "/" + year + " " + pad(hour) + ':' + pad(minute) + ':' + pad(second);
    return time;
}
function getLastTime(dtt) {//  2/1/2020/0:11:11:22
    //11:01 AM    |    June 9
    var dateObj = new Date();
    var month = dateObj.getUTCMonth() + 1; //months from 1-12
    var day = dateObj.getUTCDate();
    var year = dateObj.getUTCFullYear();
    var week = dateObj.getDay();//sun-0
    var time = dateObj.getHours() + ":" + dateObj.getMinutes() + ":" + dateObj.getSeconds();

    var weeks = (new String("Sun,Mon,Thu,Wen,Thu,Fri,Sat")).split(",");
    var res = month + "/" + day + "/" + year;
    var str = dtt.split("/");
    if (str[0] == month && str[2] == year) {
        if (str[1] == day) {
            var str1 = str[4].split(":");
            res = str[4] + ":" + str[5] + " " + getAmOrPm(str1[0]);
        }
        else res = weeks[str[3]];
    }
    return res;
}
function getNowTime(d) {//  2/1/2020/0:11:11:22
    var dateObj = d;
    var month = dateObj.getMonth() + 1; //months from 1-12
    var day = dateObj.getDate();
    var year = dateObj.getFullYear();

    var nowdate = dateObj.getHours() + ":" + pad(dateObj.getMinutes(),2) + " | " + day + "/" + month + "/" + year;

    return nowdate;
}
function getFormattedTime() {
    var today = new Date();
    var y = today.getFullYear();
    // JavaScript months are 0-based.
    var m = today.getMonth() + 1;
    var d = today.getDate();
    var h = today.getHours();
    var mi = today.getMinutes();
    var s = today.getSeconds();
    return y + "-" + m + "-" + d + "-" + h + "-" + mi + "-" + s;
}

function pad(num, size) {
    var s = "000000000" + num;
    return s.substr(s.length - size);
}

function getAmOrPm(hours) {
    var hours = (hours + 24 - 2) % 24;
    var mid = 'AM';
    if (hours == 0) { //At 00 hours we need to show 12 am
        hours = 12;
    }
    else if (hours > 12) {
        hours = hours % 12;
        mid = 'PM';
    }
    return mid
}
/* Get ISO week in month, based on first Monday in month
** @param {Date} date - date to get week in month of
** @returns {Object} month: month that week is in week: week in month
*/
function getISOWeekInMonth(date) {
    // Copy date so don't affect original
    var d = new Date(+date);
    if (isNaN(d)) return 0;
    // Move to previous Monday
    d.setDate(d.getDate() - d.getDay() + 1);
    // Week number is ceil date/7
    return Math.ceil(d.getDate() / 7);
}
/**
 * getLastTimeFromServerTime from  2/1/2020 02:11:22 AM
 */
function abbreviateDate(d) {//  2/1/2020 02:11:22 AM

    if (d == "") return "";

    var weeks = (new String("Dom,Seg,Ter,Qua,Qui,Sex,Sab")).split(",");
    var arr = d.split(" ");
    var date = arr[0];
    var time_arr = (arr.length > 1 ? arr[1] : "00:00:00").split(":");
    var time = time_arr[0] + ":" + time_arr[1];
    var apm = arr.length > 2 ? arr[2] : "AM";
    var date_arr = date.split("/");
    var date_obj = new Date(date_arr[2], date_arr[0] - 1, date_arr[1], time[0], time_arr[1]);//.toLocaleString("en-US", { timeZone: "America/New_York" });
    var today_obj = new Date();

    if (date_arr[0] == (today_obj.getUTCMonth() + 1) && date_arr[2] == today_obj.getUTCFullYear()) {
        if (date_arr[1] == today_obj.getUTCDate()) {
            return time + " " + apm;
        } else if (getISOWeekInMonth(date_obj) == getISOWeekInMonth(today_obj)) {
            return weeks[date_obj.getDay()];
        } else {
            return date_arr[1] + "/" + date_arr[0];
        }
    } else return date_arr[1] + "/" + date_arr[0] + "/" + date_arr[2];
}
/*
 * 2020-06-22 to 6/22/2020
 */
function GetKindlyDateFormat(date) {
    var arr = date.split('-');
    if (arr.length < 3) return "";
    return eval(arr[1]) + '/' + eval(arr[2]) + '/' + eval(arr[0]);
}
/*
 * get days from date range(2020-06-10 2020-06-22)
 */
function GetDaysFromDateRange(fdate, tdate) {
    return (new Date(new Date(tdate) - new Date(fdate))) / 1000 / 60 / 60 / 24 + 1;
}
/*
 * get date from start date and days
 * for example, startdate=2020-06-10, 7, function return 2020-06-16
 */
function GetDateFromStartAndOffset(fdate, days) {
    var tdate = new Date(fdate);
    tdate.setDate(tdate.getDate()+eval(days-1));
    return tdate.getUTCFullYear() + '-' + pad(tdate.getUTCMonth() + 1,2) + '-' + pad(tdate.getUTCDate(),2);
}
function getBrasilianDateTime(d) {
    if (d) {
        var data = new Date(d);
        var mes = data.getMonth()+1;
        var dia = data.getDate();
        var ano = data.getFullYear();
        var h = "" + data.getHours();
        var mi = "" + data.getMinutes();
        return dia + "/" + mes + "/" + ano + " " + h.padStart(2, '0') + ":" + mi.padStart(2, '0');
    }
    else {
        return '';
    }
}
function getBrasilianShortDateTime(d) {
    if (d) {
        var data = new Date(d);
        var mes = "" + ( data.getMonth() + 1 );
        var dia = "" + data.getDate();
        var h = "" + data.getHours();
        var mi = "" + data.getMinutes();
        return dia.padStart(2, '0') + "/" + mes.padStart(2, '0') + " " + h.padStart(2, '0') + ":" + mi.padStart(2, '0');
    }
    else {
        return '';
    }
}
function getBrasilianDate(d) {
    if (d) {
        var data = new Date(d);
        var mes = data.getMonth()+1;
        var dia = data.getDate();
        var ano = data.getFullYear();
        return dia + "/" + mes + "/" + ano;
    }
    else {
        return '';
    }
}
function getTime(d) {
    var data = new Date(d);
    var h = data.getHours();
    var mi = data.getMinutes();
    return h + ":" + mi;
}
