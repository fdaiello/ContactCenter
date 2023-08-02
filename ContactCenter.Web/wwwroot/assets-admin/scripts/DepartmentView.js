var grid;
var cols = [];
function addDepartment() {
    if ($("#add-department-name").val() == '') {
        $("#add-department-name").focus();
        return;
    }
    form_data = new FormData();
    form_data.append('id', $("#add-department-id").val());
    form_data.append('name', $("#add-department-name").val());
    form_data.append('gid', $("#gid").val());
    $.ajax({
        url: 'SaveDepartment',
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
                $('#modal-department-add').fadeOut(1000);
                $('.modal-close-button').trigger('click');
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}
function editDepartment(id, name) {
    $('#add-department-id').val(id);
    $('#add-department-name').val(name);
}
function deleteDepartment(id) {
    form_data = new FormData();
    form_data.append('id', id);
    $.ajax({
        url: 'DeleteDepartment',
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
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

cols.push({ "mData": 'id', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'name', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'createdAt', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'userCount', 'visible': true, 'bSearchable': false, 'bSortable': false });
cols.push({ "mData": 'action', 'visible': true, 'bSearchable': false, 'bSortable': false, 'width': 50, 'className': 'dt-body-right' });

$(document).ready(function () {
    var tblDef = {
        "ajax": {
            url: 'GetDepartmentList?gid=' + $('#gid').val(),
            dataSrc: "departmentList",
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
    grid = $('#department-grid').DataTable(tblDef);
});