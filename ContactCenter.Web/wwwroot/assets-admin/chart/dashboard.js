var presets = window.chartColors;
var utils = Samples.utils;
var options = {
    maintainAspectRatio: false,
    spanGaps: true,
    elements: {
        line: {
            tension: 0.000001
        }
    },
    plugins: {
        filler: {
            propagate: false
        }
    },
    scales: {
        x: {
            ticks: {
                autoSkip: false,
                maxRotation: 0
            }
        },
        yAxes: [{
            ticks: {
                stepSize: 1
            }
        }]
    },
    legend: {
        display: false,
    },
    tooltips: {
        callbacks: {
            label: function (tooltipItem) {
                return tooltipItem.yLabel;
            }
        }
    },
    decimals: 2,
    continuity: 1
};

//*pie setting*//
var randomScalingFactor = function () {
    return Math.round(Math.random() * 100);
};

var conversationByDay = null;
var userByDay = null;
var messageByDay = null;
var channelByDay = null;

function getConversationByDayLabels() {
    //label generate
    var labels = [];
    for (var i = 0; i < conversationByDay.count; i++) {
        labels.push(conversationByDay.list[i].MessageDate);
    }
    return labels;
}
function getConversationByDayData() {
    var data = [];
    for (var i = 0; i < conversationByDay.count; ++i) {
        data.push(conversationByDay.list[i].MessageCount);
    }
    return data;
}
function getConversationByDayStep() {
    return Math.round((conversationByDay.max - conversationByDay.min+5) / 5); alert(v);
}
function getUserByDayLabels() {
    //label generate
    var labels = [];
    for (var i = 0; i < userByDay.count; i++) {
        labels.push(userByDay.list[i].MessageDate);
    }
    return labels;
}
function getUserByDayData() {
    var data = [];
    for (var i = 0; i < userByDay.count; ++i) {
        data.push(userByDay.list[i].MessageCount);
    }
    return data;
}
function getUserByDayStep() {
    return Math.round((userByDay.max - userByDay.min+5) / 5);
}
function getMessageByDayLabels() {
    //label generate
    var labels = [];
    for (var i = 0; i < messageByDay.count; i++) {
        labels.push(messageByDay.list[i].MessageDate);
    }
    return labels;
}
function getMessageByDayData() {
    var data = [];
    for (var i = 0; i < messageByDay.count; ++i) {
        data.push(messageByDay.list[i].MessageCount);
    }
    return data;
}
function getMessageByDayStep() {
    return Math.round((messageByDay.max - messageByDay.min+5) / 5);
}
function getChannelByDayLabels() {
    //label generate
    var labels = [];
    for (var i = 0; i < channelByDay.count; i++) {
        labels.push(channelByDay.list[i].MessageDate + ' : ' + channelByDay.list[i].MessageCount);
    }
    return labels;
}
function getChannelByDayData() {
    var data = [];
    var sum = 0;
    for (var i = 0; i < channelByDay.count; ++i) {
        sum += eval(channelByDay.list[i].MessageCount);
    }
    var sump = 0;
    for (var i = 0; i < channelByDay.count - 1; ++i) {
        var v = Math.floor(channelByDay.list[i].MessageCount*100/sum);
        sump += v;
        data.push(v);
    }
    data.push(100 - sump);
    return data;
}

$("#filter_fdate").on('change', function () {
    location.href = 'Dashboard?fdate=' + $('#filter_fdate').val() + '&tdate=' + $('#filter_tdate').val();
});
$("#filter_tdate").on('change', function () {
    location.href = 'DashBoard?fdate=' + $('#filter_fdate').val() + '&tdate=' + $('#filter_tdate').val();
});

$(document).ready(function () {
    //conversation By Day Chart generate
    conversationByDay = JSON.parse($("#message_by_day").val()).Value;
    userByDay = JSON.parse($("#user_by_day").val()).Value;
    messageByDay = JSON.parse($("#message_by_day").val()).Value;
    channelByDay = JSON.parse($("#channel_by_day").val()).Value;

    new Chart('chart-user-by-day', {
        type: 'line',
        data: {
            labels: getUserByDayLabels(),
            datasets: [{
                backgroundColor: utils.transparentize(presets.red),
                borderColor: presets.red,
                data: getUserByDayData()
            }]
        },
        options: Chart.helpers.merge(options, {
            title: {
                display: false
            },
            scales: {
                yAxes: [{
                    ticks: {
                        stepSize: getUserByDayStep()
                    }
                }]
            }
        })
    });
    new Chart('chart-message-by-day', {
        type: 'line',
        data: {
            labels: getMessageByDayLabels(),
            datasets: [{
                backgroundColor: utils.transparentize(presets.red),
                borderColor: presets.red,
                data: getMessageByDayData()
            }]
        },
        options: Chart.helpers.merge(options, {
            title: {
                display: false
            },
            scales: {
                yAxes: [{
                    ticks: {
                        stepSize: getMessageByDayStep()
                    }
                }]
            }
        })
    });

    var pie_config = {
        type: 'pie',
        data: {
            datasets: [{
                data: getChannelByDayData(),
                backgroundColor: [
                    window.chartColors.blue,
                    window.chartColors.orange,
                    window.chartColors.green,
                    window.chartColors.yellow,
                ],
                label: 'Dataset 1'
            }],
            position: 'bottom',
            labels: getChannelByDayLabels()
        },
        options: {
            responsive: true,
            legend: {
                position: 'right',
            }
        }
    };
    var ctx = document.getElementById('chart-channel-day').getContext('2d');
    window.myPie = new Chart(ctx, pie_config);
});

// eslint-disable-next-line no-unused-vars
function toggleSmooth(btn) {
    var value = btn.classList.toggle('btn-on');
    Chart.helpers.each(Chart.instances, function (chart) {
        chart.options.elements.line.tension = value ? 0.4 : 0.000001;
        chart.update();
    });
}