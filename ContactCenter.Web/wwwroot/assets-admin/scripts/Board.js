/**
 * This is script for Admin / Board View
 * 
 * @author Daniel
 */
var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};
var applicationUsers = {};
var departments = {};

/**
 * Add new row in table
 * 
 * @param {any} board
 */
function drawTable(board) {
    let strHTML = '';
    strHTML += '<tr data-id="' + board.id +  '">';
    strHTML += '<td class="board-name">' + board.name + '</td>';
    strHTML += '<td class="board-label">' + board.label + '</td>';
    strHTML += '<td class="board-department-id">' + ( (board.departmentId == null) ? '' : departments[board.departmentId] ) + '</td>';
    strHTML += '<td class="board-application-id">' + ( (board.applicationUserId == null) ? '' : applicationUsers[board.applicationUserId] ) + '</td>';
    let allowMultiple = board.allowMultipleCardsForSameContact ? 'badge-success' : 'badge-danger';
    strHTML += '<td class="board-is-multiple"><span class="badge ' + allowMultiple + '">' + board.allowMultipleCardsForSameContact + '</span></td>';
    strHTML += '<td style="display: flex; justify-content: flex-end;">';
    strHTML += '<button data-toggle="modal" data-target="#boardModal" class="btn btn-warning btn-edit" data-id="' + board.id + '" onclick="showBoardModal(2, this)"><i class="fa fa-edit"></i></button>';
    strHTML += '<button class="btn btn-danger btn-delete"><i class="fa fa-trash"></i></button>';
    strHTML += '<div class="button-group"><button class="btn btn-danger btn-delete-confirm" data-id="' + board.id + '"><i class="fa fa-check"></i></button>';
    strHTML += '<button class="btn btn-secondary btn-delete-cancel"><i class="fa fa-window-close"></i></button></div>';
    strHTML += '</td>';
    strHTML += '</tr>';
    return strHTML;
}

function drawDataListValue(value) {
    let strHTML = '<div class="data-list-value">';
    strHTML += '<button class="btn btn-danger btn-sm btn-delete-list-value"><i class="fa fa-minus"></i></button>';
    strHTML += '<span>' + value + '</span>';
    strHTML += '</div>';
    $('#boardModal .value-list').append(strHTML);
}

/**
 * Init Fields Table 
 * 
 * */
function initView() {
    initUserOption();
    initDepartmentsOption();
    initBoardsGrid();
}

function initUserOption() {
    $.ajax({
        url: '/api/ApplicationUsers',
        type: 'GET',
        headers: APIHeader,
        success: function (users) {
            let strHTML = '<option value=""></option>';
            users.forEach((user) => {
                applicationUsers[user.id] = user.fullName;
                strHTML += '<option value="' + user.id + '">' + user.fullName + '</option>';
            });
            $('#board_application_user_id').html(strHTML);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}
function initDepartmentsOption() {
    $.ajax({
        url: '/api/Departments',
        type: 'GET',
        headers: APIHeader,
        success: function (dept) {
            let strHTML = '<option value=""></option>';
            dept.forEach((department) => {
                departments[department.id] = department.name;
                strHTML += '<option value="' + department.id + '">' + department.name + '</option>';
            });
            $('#board_department_id').html(strHTML);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}
function initBoardsGrid() {
    $.ajax({
        url: '/api/Boards',
        type: 'GET',
        headers: APIHeader,
        success: function (boards) {
            let strHTML = '';
            boards.forEach((board) => {
                strHTML += drawTable(board);
            });
            $('#field-table tbody').html(strHTML);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });    
}
/**
 * Update table when select board
 * 
 * */
$(document).on('change', '#board_selector', function () {
    let boardId = $(this).val();
    $.ajax({
        url: '/api/Boards/' + boardId,
        type: 'GET',
        headers: APIHeader, 
        success: function (board) {
            let strHTML = '';
            board.boardFields.forEach((boardField) => {
                strHTML += drawTable(boardField);
            });
            $('#field-table tbody').html(strHTML);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })
});

/**
 * Show Board Modal
 * 
 * @param type 1 - create, 2 - edit
 * */
function showBoardModal(type, modalBtn) {
    let id = $(modalBtn).attr('data-id');
    // Init Modal
    if (type == 1) {
        $('#boardModal .modal-title').html('Criar Lista');
        $('#boardModal .btn-save-field').html('Salvar');
    } else {
        $('#boardModal .modal-title').html('Editar Lista');
        $('#boardModal .btn-save-field').html('Salvar');
        $.ajax({
            url: '/api/Boards/' + id,
            type: 'GET',
            headers: APIHeader,
            success: function (board) {
                $('#board_name').val(board.name);
                $('#board_label').val(board.label);
                $('#board_department_id').val(board.departmentId);
                $('#board_application_user_id').val(board.applicationUserId);
                $('#board_is_multiple').val(board.allowMultipleCardsForSameContact);
                $('#board_is_multiple').prop('checked', board.allowMultipleCardsForSameContact);
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
            }
        })
    }
    $('#boardModal .btn-save-field').attr('data-id', id);
    $('#boardModal .btn-save-field').attr('data-type', type);
}

/**
 * Init all fields in modal when close
 * */
$(document).on('hidden.bs.modal', '#boardModal', function (e) {
    $('#board_name').val('');
    $('#board_label').val('');
    $('#board_card_name').val('');
    $('#board_department_id').val('');
    $('#board_application_user_id').val('');
    $('#board_is_multiple').val(false);
    $('#board_is_multiple').prop('checked', false);
});


// Listen switches
$(document).on('change', '#board_is_multiple', function () {
    let flag = ($(this).val() == 'true');
    $(this).val(!flag);
});

/**
 * Save fields to DB
 * 
 * */
$(document).on('click', '#boardModal .btn-save-field', function () {
    let type = $(this).attr('data-type');
    let id = $(this).attr('data-id');
    let board = {
        id: id,
        name: '',
        label: '',
        departmentId: null,
        applicationUserId: null,
        allowMultipleCardsForSameContact: false
    };
    board.name = $('#board_name').val();
    board.label = $('#board_label').val();
    if ($('#board_application_user_id').val() != "")
        board.applicationUserId = $('#board_application_user_id').val();
    if ($('#board_department_id').val() != "")
        board.departmentId = $('#board_department_id').val();
    board.allowMultipleCardsForSameContact = $('#board_is_multiple').val();
    // if create
    if (type == 1) {
        $.ajax({
            url: '/api/Boards',
            headers: APIHeader,
            type: 'POST',
            data: JSON.stringify(board),
            contentType: 'application/json',
            success: function (res) {
                // Append new low to table
                let strHTML = drawTable(res);
                $('#field-table tbody').append(strHTML);
                // Hide Modal
                $('#boardModal').modal('hide');
                $('.modal-backdrop').remove();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
            }
        })
    }
    // else update
    else {
        $.ajax({
            url: '/api/Boards/' + id,
            headers: APIHeader,
            type: 'GET',
            success: function (res) {
                board.stages = res.stages;
                board.boardFields = res.boardFields;
                $.ajax({
                    url: '/api/Boards/' + id,
                    headers: APIHeader,
                    type: 'PUT',
                    data: JSON.stringify(board),
                    contentType: 'application/json',
                    success: function (res) {
                        // Update Table row
                        let row = $('#field-table tr[data-id="' + id + '"]');
                        $(row).find('.board-name').html(board.name);
                        $(row).find('.board-label').html(board.label);
                        $(row).find('.board-department-id').html((board.departmentId==null)?'':departments[board.departmentId]);
                        $(row).find('.board-application-id').html((board.applicationUserId==null)?'':applicationUsers[board.applicationUserId]);
                        let isMultipleClass = (board.allowMultipleCardsForSameContact == 'true') ? 'badge-success' : 'badge-danger';
                        $(row).find('.board-is-multiple').html('<span class="badge ' + isMultipleClass + '">' + board.allowMultipleCardsForSameContact + '</span>');
                        // Hide Modal
                        $('#boardModal').modal('hide');
                        $('.modal-backdrop').remove();
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
                    }
                });
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
            }
        });
        
    }
}); 

/**
 * Show delete button group
 * */
$(document).on('click', 'table .btn-delete', function () {
    let parent = $(this).closest('td');
    let buttonGroup = $(parent).find('.button-group');
    $(this).toggle();
    $(buttonGroup).show();
});

/**
 * Delete Field when click confirm
 * */
$(document).on('click', 'table .btn-delete-confirm', function () {
    let id = $(this).attr('data-id');
    let row = $(this).closest('tr');
    $.ajax({
        url: '/api/Boards/' + id,
        type: 'DELETE',
        headers: APIHeader,
        success: function () {
            $(row).remove();
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })
});

/**
 * Hide delete button group
 * */
$(document).on('click', '.btn-delete-cancel', function () {
    let parentCell = $(this).closest('td');
    let buttonGroup = $(parentCell).find('.button-group');
    let deleteBtn = $(parentCell).find('.btn-delete');
    $(deleteBtn).show();
    $(buttonGroup).hide();
});

/**
 * Delete Datalist values when click remove button
 * */
$(document).on('click', '.btn-delete-list-value', function () {
    let parent = $(this).closest('.data-list-value');
    $(parent).remove();
});


$(document).ready(function () {
    initView();    
});