/**
 * This is script for Admin / Board Field View
 * 
 * @author Daniel
 */
var fieldTypeStr = [' Integer', 'Decimal', 'Money', 'Date', 'Time', 'DateTime', 'Text', 'TextArea', 'DataList', 'Image', 'Document'];
var currentBoardField = null;
var allBoards = null;
var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};

/**
 * Add new row in table
 * 
 * @param {any} fieldcreate
 */
function drawTable(boardField) {
    let strHTML = '';
    strHTML += '<tr data-id="' + boardField.id +  '">';
    strHTML += '<td>' + boardField.id + '</td>';
    strHTML += '<td class="board-field-label">' + boardField.field.label + '</td>';
    strHTML += '<td class="board-field-type">' + fieldTypeStr[boardField.field.fieldType] + '</td>';
    strHTML += '<td class="board-field-order">' + boardField.order + '</td>';
    let enabledClass = boardField.enabled ? 'badge-success' : 'badge-danger';
    strHTML += '<td class="board-field-enabled"><span class="badge ' + enabledClass + '">' + boardField.enabled + '</span></td>';
    strHTML += '<td style="display: flex; justify-content: flex-end;">';
    strHTML += '<button data-toggle="modal" data-target="#boardFieldModal" class="btn btn-warning btn-edit" data-id="' + boardField.id + '" onclick="showFieldModal(2, this)"><i class="fa fa-edit"></i></button>';
    strHTML += '<button class="btn btn-danger btn-delete"><i class="fa fa-trash"></i></button>';
    strHTML += '<div class="button-group"><button class="btn btn-danger btn-delete-confirm" data-id="' + boardField.id + '"><i class="fa fa-check"></i></button>';
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
    $('#boardFieldModal .value-list').append(strHTML);
}

/**
 * Init Fields Table 
 * 
 * */
function initView() {
    $.ajax({
        url: '/api/Boards',
        type: 'GET',
        headers: APIHeader, 
        success: function (boards) {
            allBoards = boards;
            let boardFields = boards[0].boardFields;
            let boardOptions = '';
            boards.forEach((board) => {
                boardOptions += '<option value="' + board.id + '">' + board.name  + '</option>';
            });
            $('#board_selector').html(boardOptions);
            let strHTML = '';
            boardFields.forEach((boardField) => {
                strHTML += drawTable(boardField);
            });
            $('#field-table tbody').html(strHTML);
        }
    })
}

/**
 * Update table when select board
 * 
 * */
$(document).on('change', '#board_selector', function () {
    let boardId = $(this).val();
    allBoards.forEach((board) => {
        if (board.id == boardId) {
            let strHTML = '';
            board.boardFields.forEach((boardField) => {
                strHTML += drawTable(boardField);
            });
            $('#field-table tbody').html(strHTML);
        }
    });
});

/**
 * Show Field Modal
 * 
 * @param type 1 - create, 2 - edit
 * */
function showFieldModal(type, modalBtn) {
    let id = $(modalBtn).attr('data-id');
    // Init Modal
    if (type == 1) {
        $('#boardFieldModal .modal-title').html('Criar Campo');
        $('#boardFieldModal .btn-save-field').html('Crio');
    } else {
        $('#boardFieldModal .modal-title').html('Editar Campo');
        $('#boardFieldModal .btn-save-field').html('Editar');
        $.ajax({
            url: '/api/BoardFields/' + id,
            type: 'GET',
            headers: APIHeader,
            success: function (boardField) {
                currentBoardField = boardField;
                let enabled = boardField.enabled;
                $('#board_field_label').val(boardField.field.label);
                $('#board_field_label').prop('disabled', !enabled);
                $('#board_field_order').val(boardField.order);
                $('#board_field_order').prop('disabled', !enabled);
                $('#board_field_type').val(boardField.field.fieldType);
                $('#board_field_type').prop('disabled', !enabled);
                $('#board_field_enabled').val(enabled);
                $('#board_field_enabled').prop('checked', enabled);
                if (boardField.field.fieldType == 8) {
                    $('.datalist-items').show();
                    boardField.field.dataListValues.forEach((elmnt) => {
                        drawDataListValue(elmnt.value);
                    });
                }
            }
        })
    }
    $('#boardFieldModal .btn-save-field').attr('data-id', id);
    $('#boardFieldModal .btn-save-field').attr('data-type', type);

    // Show DataList Items when select
    $('#board_field_type').on('change', function () {
        if ($(this).val() == 8) $('#boardFieldModal .datalist-items').show();
        else $('#boardFieldModal .datalist-items').hide();
    });
    // Add datalist value when press enter at datalist input
    $('#add_board_datalist_value').on('keyup', function (e) {
        if (e.which == 13 && e.target.value) {
            drawDataListValue(e.target.value);
            $(this).val('');
        }
    });
}


// Listen switches
$(document).on('change', '#board_field_enabled', function () {
    let flag = ($(this).val() == 'true');
    $('#board_field_label').prop('disabled', flag);
    $('#board_field_order').prop('disabled', flag);
    $('#board_field_type').prop('disabled', flag);
    $(this).val(!flag);
});


/**
 * Init all fields in modal when close
 * */
$(document).on('hidden.bs.modal', '#boardFieldModal', function (e) {
    $('#board_field_label').val('');
    $('#board_field_order').val('');
    $('#board_field_type').val(0);
    $('#boardFieldModal .value-list').html('');
    $('#add_board_datalist_value').val('');
    $('#boardFieldModal .datalist-items').hide();
    $('#board_field_enabled').val(false);
    $('#board_field_enabled').prop('checked', false);
});

/**
 * Save fields to DB
 * 
 * */
$(document).on('click', '#boardFieldModal .btn-save-field', function () {
    let type = $(this).attr('data-type');
    let id = $(this).attr('data-id');
    let boardField = {
        id: id,
        fieldId: 0,
        boardId: $('#board_selector').val(),
        enabled: false,
        order: 0,
        field: {
            id: 0,
            label: '',  
            fieldType: '',
            dataListValues: []
        }
    };
    boardField.field.label = $('#board_field_label').val();
    boardField.field.order = $('#board_field_order').val();
    boardField.field.fieldType = $('#board_field_type').val();
    boardField.enabled = $('#board_field_enabled').val();
    // If datalist
    if (boardField.field.fieldType == 8) {
        $('#boardFieldModal .data-list-value span').each(function() {
            boardField.field.dataListValues.push({
                id: 0,
                fieldId: 0,
                value: $(this).html()
            });
        });
    }
    // if create
    if (type == 1) {
        $.ajax({
            url: '/api/BoardFields',
            headers: APIHeader,
            type: 'POST',
            data: JSON.stringify(boardField),
            contentType: 'application/json',
            success: function (res) {
                // Append new low to table
                let strHTML = drawTable(res);
                $('#field-table tbody').append(strHTML);
                // Hide Modal
                $('#boardFieldModal').modal('hide');
                $('.modal-backdrop').remove();
            }
        })
    }
    // else update
    else {
        boardField.fieldId = currentBoardField.fieldId;
        boardField.field.id = currentBoardField.field.id;
        $.ajax({
            url: '/api/BoardFields/' + id,
            headers: APIHeader,
            type: 'PUT',
            data: JSON.stringify(boardField),
            contentType: 'application/json',
            success: function (res) {
                console.log(boardField);
                // Update Table row
                let row = $('#field-table tr[data-id="' + id + '"]');
                $(row).find('.board-field-label').html(boardField.field.label);
                $(row).find('.board-field-type').html(fieldTypeStr[boardField.field.fieldType]);
                $(row).find('.board-field-order').html(boardField.order);
                let enabledClass = (boardField.enabled == 'true') ? 'badge-success' : 'badge-danger';
                $(row).find('.board-field-enabled').html('<span class="badge ' + enabledClass + '">' + boardField.enabled + '</span>');

                // Hide Modal
                $('#boardFieldModal').modal('hide');
                $('.modal-backdrop').remove();
            }
        })
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
        url: '/api/BoardFields/' + id,
        type: 'DELETE',
        headers: APIHeader,
        success: function () {
            $(row).remove();
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