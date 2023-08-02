var grid;
var cols = [];
function saveGroup() {
    if ($("#add-group-name").val() == '') {
        $("#add-group-name").focus();
        return;
    }
    if ($("#add-group-descr").val() == '') {
        $("#add-group-descr").focus();
        return;
    }
    form_data = new FormData();
    form_data.append('id', $("#add-group-id").val());
    form_data.append('name', $("#add-group-name").val());
    form_data.append('descr', $("#add-group-descr").val());
    form_data.append('botUrl', $("#add-group-boturl").val());
    $.ajax({
        url: 'SaveGroup',
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
                $('#modal-group-add').fadeOut(1000);
                $('.modal-close-button').trigger('click');
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
function editGroup(id, name, descr, boturl) {
    $('#add-group-id').val(id);
    $('#add-group-name').val(name);
    $('#add-group-descr').val(descr);
    $('#add-group-boturl').val(boturl);
}
function deleteGroup(id) {
    form_data = new FormData();
    form_data.append('id', id);
    $.ajax({
        url: 'DeleteGroup',
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

cols.push({ "mData": 'id', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'name', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'descr', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'botUrl', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'createdAt', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'userCount', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'action', 'visible': true, 'bSearchable': false, 'bSortable': false });

$(document).ready(function () {
    var tblDef = {
        "ajax": {
            url: 'GetGroupList',
            dataSrc: "groupList",
            "type": "POST"
        },
        bJQueryUI: true,
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
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
}
    };
    grid = $('#group-grid').DataTable(tblDef);
});