/**
 * This is script for Admin / Field View
 * 
 * @author Daniel
 */
var fieldTypeStr = [' Integer', 'Decimal', 'Money', 'Date', 'Time', 'DateTime', 'Text', 'TextArea', 'DataList', 'Image', 'Document'];

var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};

/**
 * Add new row in table
 * 
 * @param {any} field
 */
function drawTable(field) {
    let strHTML = '';
    strHTML += '<tr data-id="' + field.id +  '">';
    strHTML += '<td class="field-label">' + field.label + '</td>';
    strHTML += '<td class="field-type">' + fieldTypeStr[field.fieldType] + '</td>';
    let isGlobal = field.isGlobal ? 'badge-success' : 'badge-danger';
    strHTML += '<td class="field-isGlobal">' + (field.isGlobal ? '<i class="fas fa-globe forestgreen"></i>' : '' ) + '</td>';
    strHTML += '<td style="display: flex; justify-content: flex-end;">';
    strHTML += '<button data-toggle="modal" data-target="#fieldModal" class="btn btn-warning btn-edit" data-id="' + field.id + '" onclick="showFieldModal(2, this)"><i class="fa fa-edit"></i></button>';
    strHTML += '<button class="btn btn-danger btn-delete"><i class="fa fa-trash"></i></button>';
    strHTML += '<div class="button-group"><button class="btn btn-danger btn-delete-confirm" data-id="' + field.id + '"><i class="fa fa-check"></i></button>';
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
    $('#fieldModal .value-list').append(strHTML);
}

/**
 * Init Fields Table 
 * 
 * */
function initTable() {
    $.ajax({
        url: '/api/Fields',
        type: 'GET',
        headers: APIHeader, 
        success: function (fields) {
            let strHTML = '';
            fields.forEach((field) => {
                strHTML += drawTable(field);
            });
            $('#field-table tbody').html(strHTML);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })
}

/**
 * Show Field Modal
 * 
 * @param type 1 - create, 2 - edit
 * */
function showFieldModal(type, modalBtn) {
    let id = $(modalBtn).attr('data-id');
    // Init Modal
    if (type == 1) {
        $('#fieldModal .modal-title').html('Criar Campo');
        $('#fieldModal .btn-save-field').html('Salvar');
    } else {
        $('#fieldModal .modal-title').html('Editar Campo');
        $('#fieldModal .btn-save-field').html('Salvar');
        $.ajax({
            url: '/api/Fields/' + id,
            type: 'GET',
            headers: APIHeader,
            success: function (field) {
                $('#field_label').val(field.label);
                $('#field_type').val(field.fieldType);
                if (field.fieldType == 8) {
                    $('.datalist-items').show();
                    field.dataListValues.forEach((elmnt) => {
                        drawDataListValue(elmnt.value);
                    });
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
            }
        })
    }
    $('#fieldModal .btn-save-field').attr('data-id', id);
    $('#fieldModal .btn-save-field').attr('data-type', type);

    // Show DataList Items when select
    $('#field_type').on('change', function () {
        if ($(this).val() == 8) $('#fieldModal .datalist-items').show();
        else $('#fieldModal .datalist-items').hide();
    });
    // Add datalist value when press enter at datalist input
    $('#add_datalist_value').on('keyup', function (e) {
        if (e.which == 13 && e.target.value) {
            drawDataListValue(e.target.value);
            $(this).val('');
        }
    });
}

/**
 * Init all fields in modal when close
 * */
$(document).on('hidden.bs.modal', '#fieldModal', function (e) {
    $('#field_label').val('');
    $('#field_type').val(0);
    $('#fieldModal .value-list').html('');
    $('#add_datalist_value').val('');
    $('#fieldModal .datalist-items').hide();
});

/**
 * Save fields to DB
 * 
 * */
$(document).on('click', '#fieldModal .btn-save-field', function () {
    let type = $(this).attr('data-type');
    let id = $(this).attr('data-id');
    let field = {
        id: id,
        label: '',
        fieldType: '',
        dataListValues: []
    };
    field.label = $('#field_label').val();
    field.fieldType = $('#field_type').val();
    // If datalist
    if (field.fieldType == 8) {
        $('#fieldModal .data-list-value span').each(function() {
            field.dataListValues.push({
                id: 0,
                fieldId: 0,
                value: $(this).html()
            });
        });
    }
    // if create
    if (type == 1) {
        $.ajax({
            url: '/api/Fields',
            headers: APIHeader,
            type: 'POST',
            data: JSON.stringify(field),
            contentType: 'application/json',
            success: function (res) {
                // Append new low to table
                let strHTML = drawTable(res);
                $('#field-table tbody').append(strHTML);
                // Hide Modal
                $('#fieldModal').modal('hide');
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
            url: '/api/Fields/' + id,
            headers: APIHeader,
            type: 'PUT',
            data: JSON.stringify(field),
            contentType: 'application/json',
            success: function (res) {
                // Update Table row
                let row = $('#field-table tr[data-id="' + id + '"]');
                $(row).find('.field-id').html(id);
                $(row).find('.field-label').html(field.label);
                $(row).find('.field-type').html(fieldTypeStr[field.fieldType]);
                // Hide Modal
                $('#fieldModal').modal('hide');
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
        url: '/api/Fields/' + id,
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
    initTable();    
});