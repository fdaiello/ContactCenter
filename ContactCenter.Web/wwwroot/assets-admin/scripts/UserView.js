var grid;
var cols = [];

function readURL(input, f) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#' + f).attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}

function addUser() {
    if ($("#add-user-name").val() == '') {
        $("#add-user-name").focus();
        return;
    }
    if ($("#add-user-name").val().includes(' ')) {
        alert ('O nome de usuário não pode conter espaço.')
        $("#add-user-name").focus();
        return;
    }
    if ($("#add-role-fullname").val() == '') {
        $("#add-role-fullname").focus();
        return;
    }
    if ($("#add-role-nickname").val() == '') {
        $("#add-role-nickname").focus();
        return;
    }
    if ($("#add-role-email").val() == '') {
        $("#add-role-email").focus();
        return;
    }
    form_data = new FormData();
    var file_data = $('#add-user-avatar').prop('files')[0];
    form_data.append('id', $("#add-user-id").val());
    form_data.append('avatar', file_data);
    form_data.append('username', $("#add-user-name").val());
    form_data.append('fullname', $("#add-user-fullname").val());
    form_data.append('nickname', $("#add-user-nickname").val());
    form_data.append('email', $("#add-user-email").val());
    form_data.append('groupId', $("#add-user-group").val());
    form_data.append('departmentId', $("#add-user-department").val());
    form_data.append('role', $("#add-user-role").val());
    form_data.append('notification', $("#add-user-notification").val());
    $.ajax({
        url: 'SaveUser',
        cache: false,
        contentType: false,
        processData: false,
        data: form_data,
        type: 'POST',
        dataType: "text",
        success: function (response) {
            //é retirado a tela de processamento liberando a tela para novos eventos
            var res = JSON.parse(response);
            if (res.msg == 'ok') {
                grid.ajax.reload();
                $('#modal-user-add').fadeOut(1000);
                $('.modal-close-button').trigger('click');
            }
            else {
                alert(res.msg);
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            //alerta de erro
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}
function editUser(id, name, fullname, nickname, email, group, role, avatar, notification) {
    $('#add-user-id').val(id);
    $('#add-user-name').val(name);
    $('#add-user-fullname').val(fullname);
    $('#add-user-nickname').val(nickname);
    $('#add-user-email').val(email);
    $('#add-user-group').val(group);
    initDepartmentSelect();
    $('#add-user-role').val(role);
    $('#add-user-notification').val(notification);
    $('#temp_add_img').prop('src', avatar);
    $('#add-user-name').prop("disabled", true);
    var data = grid.rows().data();
    for (var i = 0; i < data.length; i++)
        if (data[i].id == id) {
            $('#temp_add_img').prop('src', data[i].avatar);
            $('#add-user-department').val(data[i].departmentId);
            $('#add-user-notification').val(data[i].notification);
            break;
        }

}
function deleteUser(id) {
    form_data = new FormData();
    form_data.append('id', id);
    $.ajax({
        url: 'DeleteUser',
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
            }
            else {
                alert(res.msg);
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}
function resetPassword(id) {
    form_data = new FormData();
    form_data.append('id', id);
    $.ajax({
        url: 'ResetUserPassword',
        cache: false,
        contentType: false,
        processData: false,
        data: form_data,
        type: 'POST',
        dataType: "text",
        success: function (response) {
            var res = JSON.parse(response);
            if (res.msg == 'ok') {
                alert("Successfull!");
            }
            else {
                alert(res.msg);
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}
function initDepartmentSelect() {
    departs = JSON.parse($('#DepartmentList').val());
    var html = '<option value="0"></option>';
    for (var i = 0; i < departs.length; i++) if ($('#add-user-group').val() == departs[i].GroupId) {
        html += '<option value="' + departs[i].Id + '">' + departs[i].Name + '</option>';
    }
    $('#add-user-department').html(html);
}
cols.push({
    "mData": null, 'visible': true, 'bSearchable': false, 'bSortable': false,
    'sDefaultContent': '<div class= "expand /" > ', 'sWidth': "30px"
});
cols.push({ "mData": 'id', 'visible': false, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'groupName', 'visible': false, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'fullName', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'departmentName', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'roleName', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'userName', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'email', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'nickName', 'visible': false, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'createdAt', 'visible': false, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'notification', 'visible': false, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'action', 'visible': true, 'bSearchable': false, 'bSortable': false, 'width': 50, 'className': 'dt-body-right' });

$(document).ready(function () {
    $("#add-user-avatar").on('change', function () {
        readURL(this, "temp_add_img");
    });
    $('#add-user-group').on('change', function () {
        initDetartmentSelect();
    });
    var tblDef = {
        "ajax": {
            url: 'GetUserList',
            dataSrc: "userList",
            "type": "POST"
        },
        bJQueryUI: true,
        searching: false,
        sPaginationType: 'full_numbers',
        aaSorting: [[0, 'asc']],
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
    grid = $('#user-grid').DataTable(tblDef);

    function fnFormatDetails(oTable, nTr) {
        var aData = oTable.fnGetData(nTr);
        sOut = "error";
        sOut = '<div style=""><table width="100%">' +
            '<tr><td width="250px"><img style="border-radius:50%;width:200px;height:200px;" src="' + aData['avatar'] + '"></td><td width="*"><table width="100%">' +
            '<tr height="28px">' +
            '<td width="20px"></td>' +
            '<td width="150px">Usuario:</td>' +
            '<td width="*">' + aData['userName'] + '</td>' +
            '</tr>' +
            '<tr height="28px">' +
            '<td width="20px"></td>' +
            '<td width="150px">Email:</td>' +
            '<td width="*">' + aData['email'] + '</td>' +
            '</tr>' +
            '<tr height="28px">' +
            '<td width="20px"></td>' +
            '<td width="150px">Nome:</td>' +
            '<td width="*">' + aData['fullName'] + '</td>' +
            '</tr>' +
            '<tr height="28px">' +
            '<td width="20px"></td>' +
            '<td width="150px">Apelido:</td>' +
            '<td width="*">' + aData['nickName'] + '</td>' +
            '</tr>' +
            '<tr height="28px">' +
            '<td width="20px"></td>' +
            '<td width="150px">Criação:</td>' +
            '<td width="*">' + aData['createdAt'] + '</td>' +
            '</tr>' +
            '<tr height="28px">' +
            '<td width="20px"></td>' +
            '<td width="150px"></td>' +
            '<td width="*"><button onclick="resetPassword(\'' + aData['id'] + '\');">redefinir senha</button></td>' +
            '</tr>' +
            '</table></td></tr></table></div>';
        return sOut;
    }
    $('#user-grid tbody').on('click', 'tr', function () {
        var grid = $('#user-grid').dataTable();
        var nTr = $(this);
        if (grid.fnIsOpen($(this))) {
            grid.fnClose(nTr);
            $(this).find('td div.open').removeClass('open');
        } else {
            $(this).parent().find('tr').each(function (i, el) {
                $(this).find('td div.open').removeClass('open');
                grid.fnClose($(this));
            });

            $.fn.dataTableExt.sErrMode = 'throw';
            $(this).find('td div.expand').addClass('open');
            grid.fnOpen(nTr, fnFormatDetails(grid, nTr), 'details');
        }
    });
});