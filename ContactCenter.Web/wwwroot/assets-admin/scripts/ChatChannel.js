var grid;

function saveChatChannel() {

    $("#add-chatchannels-id").attr("style", "1px solid #ced4da");
    $("#add-chatchannels-name").attr("style", "1px solid #ced4da");
    $("#add-chatchannels-channeltype").attr("style", "1px solid #ced4da");

    var id = $("#add-chatchannels-id");
    var name = $("#add-chatchannels-name");
    var phoneno = $("#add-chatchannels-phoneno");
    var appname = $("#add-chatchannels-appname");
    var department = $("#add-chatchannels-department");
    var applicationuser = $("#add-chatchannels-applicationuser");
    var host = $("#add-chatchannels-host");
    var login = $("#add-chatchannels-login");
    var password = $("#add-chatchannels-password");
    var status = $("#add-chatchannels-status");
    var from = $("#add-chatchannels-from");

    if (id.val() == '') {
    $("#add-chatchannels-id").attr("style", "border-color:red;");
        id.focus();
        return false;
    }
    else if (name.val() == '') {
        $("#add-chatchannels-name").attr("style", "border-color:red;");
        name.focus();
        return false;
    }

    form_data = new FormData();

    form_data.append('id', id.val());
    form_data.append('Name', name.val());
    form_data.append('PhoneNumber', phoneno.val());
    form_data.append('AppName', appname.val());
    form_data.append('ChannelType', $("#add-chatchannels-channeltype option:selected").val());
    form_data.append('ChannelSubType', $("#add-chatchannels-channelsubtype option:selected").val());
    form_data.append('DepartmentId', department.val());
    form_data.append('ApplicationUserId', applicationuser.val());
    form_data.append('Host', host.val());
    form_data.append('Login', login.val());
    form_data.append('Password', password.val());
    form_data.append('Status', status.val());
    form_data.append('From', from.val());

    $.ajax({
    url: 'SaveChatChannel',
        cache: false,
        contentType: false,
        processData: false,
        data: form_data,
        type: 'POST',
        dataType: "text",
        success: function (response) {
            var res = JSON.parse(response);
            if (res.msg == 'ok') {
                grid.ajax.reload();
                $('#modal-chatchannel-add').fadeOut(1000);
                $('.modal-close-button').trigger('click');
            }
            else {
                alert ( res.msg)
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

function channeltypeChange(e) {

    e.preventDefault();

    InitializeFields();
}

function InitializeFields() {

    $("#divQRCode").html("");

    var channeltype = $("#add-chatchannels-channeltype option:selected").text();
    var channelsubtype = $("#add-chatchannels-channelsubtype option:selected").text();

    if (channeltype == "WhatsApp") {
        $('#div-chatchannels-subtipo').removeClass('hide')
        $('#div-chatchannels-phoneno').removeClass('hide')
    }
    else {
        $('#div-chatchannels-subtipo').addClass('hide')
        $('#div-chatchannels-phoneno').addClass('hide')
    }

    if (channeltype == "Email") {
        $('#div-chatchannels-host').removeClass('hide')
        $('#div-chatchannels-from').removeClass('hide')
    }
    else {
        $('#div-chatchannels-host').addClass('hide')
        $('#div-chatchannels-from').addClass('hide')
    }

        $("#btn_chatchannel_import").addClass('hide');

    if (channeltype == "WhatsApp" && (channelsubtype == "Alternate1" || channelsubtype == "Alternate2" )) {
        $("#add-chatchannels-status").attr("disabled", "disabled");
        $("#div-chatchannels-status").removeClass('hide');

        if (channelsubtype == "Alternate2" | channelsubtype == "Alternate1" ) {
            var status = $("#add-chatchannels-status").val();
            var deviceId = $("#add-chatchannels-id").val();

            if (status == 'authorize' | status == 'SCAN' | status == 'inicializando' ) {
                //Spinner que indica que determinada função está sendo processada!
                var template = `<div class="spinner-start" style="width:500px;margin:auto;padding-left:100px">gerando QrCode...
                    <div class="spinner-border text-info">
                    </div>
                </div>`;
                $("#divQRCode").html(template);
                scanDevice(deviceId);
            }
            else if (status == "CONNECTED" | status == "online") {
                $("#btn_chatchannel_import").removeClass('hide');
            }
        }
    }
    else {
        $("#add-chatchannels-status").removeAttr("disabled");
        $("#div-chatchannels-status").addClass('hide');
    }


    if ((channeltype == "WhatsApp" && channelsubtype == "Oficial") | channeltype == "Email") {
        $("#div-chatchannels-appname").removeClass('hide');
    }
    else {
        $("#div-chatchannels-appname").addClass('hide');
    }

    if (channeltype == "Email" || channeltype == "Instagram" || channeltype == "SMS" ) {
        $("#div-chatchannels-login").removeClass('hide');
        $("#div-chatchannels-password").removeClass('hide');
    }
    else {
        $("#div-chatchannels-login").addClass('hide');
        $("#div-chatchannels-password").addClass('hide');
    }

}

function scanDevice(deviceId) {

    form_data = new FormData();

    form_data.append('deviceId', deviceId);

    $.ajax({
    url: 'ScanDevice',
        cache: false,
        contentType: false,
        processData: false,
        data: form_data,
        type: 'POST',
        dataType: "text",
        success: function (response) {
            var res = JSON.parse(response);
            $("#divQRCode").html(res.content);

        },
        error: function (xhr, textStatus, errorThrown) {
            alert(errorThrown);
        }
    });
}

function clearChatChannelFormFields() {

    $("#add-chatchannels-id").val("");
    $("#add-chatchannels-name").val("");
    $("#add-chatchannels-phoneno").val("");
    $("#add-chatchannels-appname").val("");
    $("#add-chatchannels-host").val("");
    $("#add-chatchannels-login").val("");
    $("#add-chatchannels-password").val("");
    $("#add-chatchannels-status").val("");
    $("#add-chatchannels-from").val("");
}

function editChatChannel(data) {

    $("#add-chatchannels-id").val(data.id);
    $("#add-chatchannels-name").val(data.name);
    $("#add-chatchannels-phoneno").val(data.phoneNumber);
    $("#add-chatchannels-appname").val(data.appName);
    $("#add-chatchannels-channeltype").val(data.channelType);
    $("#add-chatchannels-channelsubtype").val(data.channelSubType);
    $("#add-chatchannels-department").val(data.departmentId);
    $("#add-chatchannels-applicationuser").val(data.applicationUserId);
    $("#add-chatchannels-host").val(data.host);
    $("#add-chatchannels-login").val(data.login);
    $("#add-chatchannels-password").val(data.password);
    $("#add-chatchannels-status").val(data.status);
    $("#add-chatchannels-from").val(data.from);

    InitializeFields();
}

/*
* Delete Chat Channel
*/
function deleteChatChannel(id,row) {
    form_data = new FormData();

    form_data.append('id', id);
    $.ajax({
    url: 'DeleteChatChannel',
        cache: false,
        contentType: false,
        processData: false,
        data: form_data,
        type: 'POST',
        dataType: "text",
        success: function (response) {
            var res = JSON.parse(response);
            if (res.msg == 'ok') {
                // Delete row from grid
                grid.row(row).remove().draw();
            }
            else {
                alert(res.msg);
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(errorThrown);
        }
    });
}

/*
* Redeploy Channel
*/
function redeployChatChannel(id) {
    form_data = new FormData();

    form_data.append('id', id);
    $.ajax({
        url: 'RedeployChatChannel',
        cache: false,
        contentType: false,
        processData: false,
        data: form_data,
        type: 'POST',
        dataType: "text",
        success: function (response) {
            var res = JSON.parse(response);
            alert(res.message);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(errorThrown);
        }
    });
}


function ImportContacts() {

    var deviceId = $("#add-chatchannels-id").val();
    form_data = new FormData();
    form_data.append('deviceId', deviceId);

    $.ajax({
        url: 'SynchronizeContacts',
        cache: false,
        processData: false,
        contentType: false,
        data: form_data,
        type: 'POST',
        dataType: "text",
        success: function (response) {
            var res = JSON.parse(response);
            alert(res.content);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(errorThrown);
        }
    });
}

$(document).ready(function () {

    var mayTapiButtons = '<button class="btn btn-success btn-redeploy" title="Reiniciar"><i class="fas fa-power-off"></i>';
    var renderButtons = `<button class="btn btn-danger btn-delete" title="Excluir" ><i class="fa fa-trash"></i>
                    <button class="btn btn-warning btn-edit" data-toggle="modal" data-target="#modal-chatchannels" title="Editar"><i class="fa fa-edit"></i></button>`;

    var cols = [];
    cols.push({
        "mData": 'icon', 'bSearchable': false, 'bSortable': false, "render": function (data, type, row) {
            let icon = `<img src="\\assets-chatroom\\images\\channels\\${row.icon}" alt="${row.icon}" class="channel-icon">`;
            return icon;
        } });
    cols.push({ "mData": 'typeDescr', 'bSearchable': false, 'bSortable': true });
    cols.push({ "mData": 'id', 'bSearchable': false, 'bSortable': true });
    cols.push({ "mData": 'name', 'bSearchable': false, 'bSortable': true });
    cols.push({ "mData": 'phoneNumber', 'bSearchable': false, 'bSortable': false });
    cols.push({ "mData": 'status', 'bSearchable': false, 'bSortable': false });
    cols.push({
        "mData": 'action', 'visible': true, 'bSearchable': false, 'bSortable': false, 'width': '50px', 'className': 'dt-body-right', "render": function (data, type, row) {
            if (row.channelSubType == "3")
                return mayTapiButtons + renderButtons;
            else
                return renderButtons;
        },
        orderable: false,
        className: 'align-right'
    });
    cols.push({ "mData": 'channelType', "visible": false });
    cols.push({ "mData": 'channelSubType', "visible": false });
    cols.push({ "mData": 'host', "visible": false });
    cols.push({ "mData": 'login', "visible": false });
    cols.push({ "mData": 'password', "visible": false });
    cols.push({ "mData": 'from', "visible": false });
    cols.push({ "mData": 'appName', "visible": false });

    $("#div-chatchannels-status").addClass('hide');
    $("#div-chatchannels-host").addClass('hide');
    $("#div-chatchannels-login").addClass('hide');
    $("#div-chatchannels-password").addClass('hide');
    $("#div-chatchannels-from").addClass('hide');
    $("#div-chatchannels-appname").addClass('hide');
    $("#btn_chatchannel_import").addClass('hide');

    var tblDef = {
        "ajax": {
            url: 'GetChatChannelList',
            dataSrc: "chatChannelList",
            "type": "POST"
        },
        searching: false,
        bJQueryUI: true,
        sPaginationType: 'full_numbers',
        aaSorting: [[2, 'asc']],
        aoColumns: cols,
        dom: 'Bfrtip',
        language: {
            search: "Procurar",
            paginate: {
                first: "Início",
                last: "Fim",
                previous: "Anterior",
                next: "Proximo",
            }
        }
    };

    grid = $('#chatchannel-grid').DataTable(tblDef);

    // Bind DELETE button event
    $('#chatchannel-grid tbody').on('click', '.btn-delete', function () {
        var row = $(this).parents('tr');
        var data = grid.row(row).data();
        id = data.id;
        deleteChatChannel(id, row);
    });

    // Bind EDIT button event
    $('#chatchannel-grid tbody').on('click', '.btn-edit', function () {
        currentRow = $(this).parents('tr');
        var data = grid.row(currentRow).data();
        editChatChannel(data);
    });

    // Bind REDEPLOY button event
    $('#chatchannel-grid tbody').on('click', '.btn-redeploy', function () {
        var row = $(this).parents('tr');
        var data = grid.row(row).data();
        id = data.id;
        redeployChatChannel(id);
    });
});
