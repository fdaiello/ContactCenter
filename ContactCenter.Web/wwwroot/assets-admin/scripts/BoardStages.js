/**
 * This is script for Admin / Board Stage View
 * 
 * @author Daniel
 */

var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};

/**
 * Add new row in table
 * 
 * @param {any} stage
 */
function drawTable(stage) {
    let strHTML = '';
    strHTML += '<tr data-id="' + stage.id + '">';
    strHTML += '<td class="stage-name">' + ( stage.name.length == 0 ? '--estágio sem nome--' : stage.name ) + '</td>';
    strHTML += '<td class="stage-label">' + stage.label + '</td>';
    strHTML += '<td class="stage-order">' + stage.order + '</td>';
    strHTML += '<td style="display: flex; justify-content: flex-end;">';
    strHTML += '<button data-toggle="modal" data-target="#boardStageModal" class="btn btn-warning btn-edit" data-id="' + stage.id + '" onclick="showStageModal(2, this)"><i class="fa fa-edit"></i></button>';
    strHTML += '<button class="btn btn-danger btn-delete"><i class="fa fa-trash"></i></button>';
    strHTML += '<div class="button-group"><button class="btn btn-danger btn-delete-confirm" data-id="' + stage.id + '"><i class="fa fa-check"></i></button>';
    strHTML += '<button class="btn btn-secondary btn-delete-cancel"><i class="fa fa-window-close"></i></button></div>';
    strHTML += '</td>';
    strHTML += '</tr>';
    return strHTML;
}
/**
 * Init Fields Table 
 * 
 * */
function initView() {
    $.ajax({
        url: '/api/Boards/CurrentUser',
        type: 'GET',
        headers: APIHeader,
        success: function (boards) {
            let boardStages = boards[0].stages;
            let boardOptions = '';
            boards.forEach((board) => {
                boardOptions += '<option value="' + board.id + '">' + board.name + '</option>';
            });
            $('#board_selector').html(boardOptions);
            let strHTML = '';
            boardStages.forEach((stage) => {
                strHTML += drawTable(stage);
            });
            $('#field-table tbody').html(strHTML);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })
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
            board.stages.forEach((stage) => {
                strHTML += drawTable(stage);
            });
            $('#field-table tbody').html(strHTML);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })
});

/**
 * Show Stage Modal
 * 
 * @param type 1 - create, 2 - edit
 * */
function showStageModal(type, modalBtn) {
    let id = $(modalBtn).attr('data-id');
    // Init Modal
    if (type == 1) {
        $('#boardStageModal .modal-title').html('Adicionar Estágio');
        $('#boardStageModal .btn-save-field').html('Salvar');
    } else {
        $('#boardStageModal .modal-title').html('Editar Estágio');
        $('#boardStageModal .btn-save-field').html('Salvar');
        $.ajax({
            url: '/api/Stages/' + id,
            type: 'GET',
            headers: APIHeader,
            success: function (stage) {
                $('#board_stage_name').val(stage.name);
                $('#board_stage_label').val(stage.label);
                $('#board_stage_order').val(stage.order);
                $('#board_stage_showmax').val(stage.showMax);
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
            }
        })
    }
    $('#boardStageModal .btn-save-field').attr('data-id', id);
    $('#boardStageModal .btn-save-field').attr('data-type', type);
}
/**
 * Init all stage in modal when close
 * */
$(document).on('hidden.bs.modal', '#boardStageModal', function (e) {
    $('#board_stage_name').val('');
    $('#board_stage_label').val('');
    $('#board_stage_order').val('0');
    $('#board_stage_showmax').val('0');
});

/**
 * Save fields to DB
 * 
 * */
$(document).on('click', '#boardStageModal .btn-save-field', function () {
    let type = $(this).attr('data-type');
    let id = $(this).attr('data-id');
    let boardId = $('#board_selector').val();
    let stage = {
        id: id,
        boardId: boardId,
        name: '',
        label: '',
        order: 0,
        showMax: 0,
        cards: []
    };
    stage.name = $('#board_stage_name').val();
    stage.label = $('#board_stage_label').val();
    stage.order = $('#board_stage_order').val();
    stage.showMax = $('#board_stage_showmax').val();
    // if create
    if (type == 1) {
        $.ajax({
            url: '/api/Stages',
            headers: APIHeader,
            type: 'POST',
            data: JSON.stringify(stage),
            contentType: 'application/json',
            success: function (res) {
                // Append new low to table
                let strHTML = drawTable(res);
                $('#field-table tbody').append(strHTML);
                // Hide Modal
                $('#boardStageModal').modal('hide');
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
            url: '/api/Stages/' + id,
            headers: APIHeader,
            type: 'PUT',
            data: JSON.stringify(stage),
            contentType: 'application/json',
            success: function (res) {
                // Update Table row
                let row = $('#field-table tr[data-id="' + id + '"]');
                $(row).find('.stage-name').html(stage.name);
                $(row).find('.stage-label').html(stage.label);
                $(row).find('.stage-order').html(stage.order);
                // Hide Modal
                $('#boardStageModal').modal('hide');
                $('.modal-backdrop').remove();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
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
        url: '/api/Stages/' + id,
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