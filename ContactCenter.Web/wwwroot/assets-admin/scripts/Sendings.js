/*
 * Script para Admin - CRM - Sendings
 * @author: Felipe Daiello
 * 
 */

var grid;
var currentRow;

var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};

/**
 * Save fields at modal to DB
 * */
$(document).on('click', '#modal-sending .btn-save-field', function () {

    $("#modal-sending-type").attr("style", "1px solid #ced4da");
    $("#modal-sending-title").attr("style", "1px solid #ced4da");
    $("#modal-sending-board").attr("style", "1px solid #ced4da");
    $("#modal-sending-message").attr("style", "1px solid #ced4da");

    var messageType = $("#modal-sending-type").val();
    var title = $("#modal-sending-title").val();
    var id = $("#modal-sending-id").val();
    var chatChannelId = $("#modal-sending-channel").val();
    var messageId = $("#modal-sending-message").val();
    var boardId = $("#modal-sending-board").val();
    var filterId = $("#modal-sending-filter").val();
    var scheduledDate = $("#modal-sending-scheduleddate").val();
    var status = $("#modal-sending-status").val();
     
    // Inicializa o Id, se for nulo
    if (!id)
        id = 0;

    // Inicializa o Status, se for nulo
    if (!status)
        status = 0;

    // Validação dos campos obrigatorios
    if (messageType == '') {
        $("#modal-sending-type").attr("style", "border-color:red;");
        alert('Preencha o tipo');
        $("#modal-sending-type").focus();
        return false;
    }
    else if (title == '') {
        $("#modal-sending-title").attr("style", "border-color:red;");
        alert('Preencha o título');
        $("#modal-sending-title").focus();
        return false;
    }
    else if (boardId == '0') {
        $("#modal-sending-board").attr("style", "border-color:red;");
        alert('Escolha uma lista');
        $("#modal-sending-board").focus();
        return false;
    }
    else if (messageId == '0') {
        $("#modal-sending-message").attr("style", "border-color:red;");
        alert('Escolha a mensagem');
        $("#modal-sending-message").focus();
        return false;
    }

    // Validação da data do envio
    if (scheduledDate)
        if (!isValidDate(scheduledDate)) {
            alert('Data inválida');
            scheduleddate.focus();
            return false;
        }

    // Cria um objeto SENDING para ser salvo
    let sending = {
        id: id,
        status: status,
        messageType: messageType,
        title: title,
        chatChannelId: chatChannelId,
        messageId: messageId,
        boardId: boardId,
        filterId: filterId == 0 ? null : filterId,
        scheduledDate: scheduledDate ? convertPtDateString(scheduledDate) : null
    };

    SaveGroupCampaign(sending)

}); 

/*
 * Salva o envio
 */
function SaveGroupCampaign(sending) {

    var method;
    var url = '/api/Sendings';
    if (sending.id != 0) {
        method = 'PUT';
        url += '/' + sending.id;
    }
    else
        method = 'POST'

    startSpinner();

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        headers: APIHeader,
        data: JSON.stringify(sending),
        success: function (newSending) {
            if (method == 'POST') {
                // Append new row to table
                grid.row.add(newSending); 
                grid.draw();
            }
            else {
                // Update sending descriptors data
                sending.board = { name: $("#modal-sending-board option:selected").text() };
                sending.message = { title: $("#modal-sending-message option:selected").text() };
                sending.chatChannel = { name: $("#modal-sending-channel option:selected").text() };

                // Update grid row                
                grid.row(currentRow).data(sending).draw();

            }
            // Close modal
            $('#modal-sending').fadeOut(1000);
            $('.modal-close-button').trigger('click');

            stopSpinner();

            // Recarrega a Grid depois de 5 segundos
            setTimeout(() => {
                reloadGridData();
            }, 5000);
        },
        error: function (xhr, textStatus, errorThrown) {
            stopSpinner;
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

/*
 * Clear Sending Modal
 * @param type 1 - create, 2 - edit
 */
function clearSendingModal() {

    // Limpa os campos
    $("#modal-sending-type").val("");              // Fixo no tipo whatsapp
    $("#modal-sending-title").val("");
    $("#modal-sending-id").val("0");
    $("#modal-sending-board").val("");
    $("#modal-sending-channel").val("");
    $("#modal-sending-message").val("");
    $("#modal-sending-channel").val("");
    $("#modal-sending-filter").val("");
    $("#modal-sending-scheduleddate").val("");

    // Titulo da tela
    $("#boardStageModalLabel").html("Criar envio");

    // Configura o select dos Boards
    initBoardSelector(0);

    // Habilita o botão salvar
    $(".btn-save-field").prop('disabled', false)

}

function SendingTypeChange(e) {

    e.preventDefault();

}

function editSending(data) {

    // Fill modal fields with data from the row
    $("#modal-sending-title").val(data.title);
    $("#modal-sending-type").val(data.messageType);
    $("#modal-sending-id").val(data.id);
    $("#modal-sending-status").val(data.status);
    $("#boardStageModalLabel").html("Editar envio");

    // Init selectors with selected option
    initBoardSelector(data.boardId);
    initchannelSelector(data.messageType, data.chatChannelId);
    initMessageSelector(data.messageType, data.messageId);
    initFilterSelector(data.boardId, data.filterId);

    // Init scheduled date
    $("#modal-sending-scheduleddate").val(getBrasilianDateTime(data.scheduledDate));

    // Se já foi processado ou estiver cancelado, impede editar
    $(".btn-save-field").prop('disabled', data.status != 0)

}

/**
 * Init Chat Channel selector
 * 
 */
function initchannelSelector(messageType, chatChannelId) {

    /* Mapeia o tipo da mensagem no tipo do canal
    *         messageType: 0-email, 1-whatsapp, 2-sms
    *         channelType: 0-None, 1-WhatsApp, 2-WebChat, 3-Messenger, 4-Emulator, 5-Instagram, 6-other, 7-Email, 8-SMS
    */
    let channelType;
    if (messageType == 0)
        channelType = 7;
    else if (messageType == 1)
        channelType = 1;
    else if (messageType == 2)
        channelType = 8;
    else
        channelType = 0;

    let channelSelector = $('#modal-sending-channel');
    getAjax('/api/chatChannels', APIHeader, false, false).then((res) => {
        let strHTML = '';
        let foundChannel = false;
        let channelCount = 0;
        res.forEach((channel) => {
            // So da opção de escolher os canais compatíveis com o tipo da mensagem
            if (channel.channelType == channelType) {
                strHTML += '<option value="' + channel.id + '"' + (channel.id == chatChannelId ? ' selected' : '') + '>' + channel.name + '</option>';
                foundChannel = true;
                channelCount ++
            }
        });

        // If there are more than one channel
        if (channelCount > 1 ) {
            // Set default empty option
            strHTML = '<option value="0"></option>' + strHTML;
            // Show channel selector
            $('#sending-channel').removeClass('hide');
        }
        else {
            // Hide channel selector
            $('#sending-channel').addClass('hide');
        }

        if (!foundChannel && channelType > 0) {
            alert("Sua conta ainda não tem nenhum canal configurado para este tipo de envio!");
        }

        channelSelector.html(strHTML);
    });
}

/**
 * Init Board selector
 * 
 */
function initBoardSelector(boardId) {
    let boardSelector = $('#modal-sending-board');
    getAjax('/api/Boards', APIHeader, false, false).then((res) => {
        let strHTML = '<option value="0"></option>';
        res.forEach((board) => {
            strHTML += '<option value="' + board.id + '"' + (board.id == boardId ? ' selected' : '') + '>' + board.name + '</option>';
        });
        boardSelector.html(strHTML);
    });
}

/**
 * Init Message selector
 * 
 */
function initMessageSelector(sendingType, messageId) {
    let messageSelector = $('#modal-sending-message');
    getAjax('/api/Messages', APIHeader, false, false).then((res) => {
        let strHTML = '<option value="0"></option>';
        res.forEach((message) => {
            if ( message.messageType == sendingType)
                strHTML += '<option value="' + message.id + '"' + (message.id == messageId ? ' selected' : '') + '>' + message.title + '</option>';
        });
        messageSelector.html(strHTML);
    });
}

/**
 * Init filter selector
 * 
 */
function initFilterSelector(boardId, filterId) {
    let filterSelector = $('#modal-sending-filter');
    if (boardId) {
        getAjax('/api/Filters?boardId=' + boardId, APIHeader, false, false).then((res) => {
            let strHTML = '<option value="0"></option>';
            res.forEach((filter) => {
                strHTML += '<option value="' + filter.id + '"' + (filter.id == filterId ? ' selected' : '') +'>' + filter.title + '</option>';
            });
            filterSelector.html(strHTML);
        });
    }
    else {
        filterSelector.html('<option value="0"></option>')
    }
}

function deleteSending(id,row) {

    $.ajax({
    url: '/api/Sendings/' + id,
        type: 'DELETE',
        headers: APIHeader,
        contentType: 'application/json',
        success: function () {
            // Delete row from grid
            grid.row(row).remove().draw();
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

/*
 * Ao carregar o documento, inicializa a Grid
 */
$(document).ready(function () {

    startSpinner();

    // Get data from API
    $.ajax({
        url: '/api/Sendings',
        headers: APIHeader,
        type: 'GET',
        success: function (data) {
            populateGrid(data);
            stopSpinner();
            // Se tem alguma linha com status importando
            if (hasSendingRow(data))
                // Recarrega a Grid depois de 5 segundos
                setTimeout(() => {
                    reloadGridData();
                }, 5000);
        },
        error: function (xhr, textStatus, errorThrown) {
            stopSpinner();
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })

    // Configure date time picker
    // https://www.jqueryscript.net/time-clock/Clean-jQuery-Date-Time-Picker-Plugin-datetimepicker.html
    // https://xdsoft.net/jqplugins/datetimepicker/
    $('#modal-sending-scheduleddate').datetimepicker({
        format: 'd/m/Y H:i'
    });
    $.datetimepicker.setLocale('pt-BR');

});

/*
 * Chama a API para ler os dados da Grid
 * Chama a rotina que preenche a Grid com os dados
 */
function reloadGridData() {
    // Get data from API
    $.ajax({
        url: '/api/Sendings',
        headers: APIHeader,
        type: 'GET',
        success: function (data) {
            let datatable = $('#sendings-grid').DataTable();
            datatable.clear();
            datatable.rows.add(data);
            datatable.draw();
            // Se tem alguma linha com status importando
            if (hasSendingRow(data))
                // Recarrega a Grid depois de 3 segundos
                setTimeout(() => {
                    reloadGridData();
                }, 3000);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })
}

/* 
 * Confere se tem alguma linha com status ENVIANDO
 */
function hasSendingRow(data) {
    let has = false;
    data.forEach(function (row) {
        if (row.status == 0 || row.status == 1)
            has = true;
    })
    return has;
}

function populateGrid(data) {

    var renderButtons = `<button class="btn btn-danger btn-delete" title="Excluir envio"><i class="fa fa-trash"></i>
                     <button class="btn btn-warning btn-edit" data-toggle="modal" data-target="#modal-sending" title="Editar envio"><i class="fa fa-edit"></i></button>
                     <button class="btn btn-info" data-toggle="modal" data-target="#modal-sending-report" title="Visuallizar relatório"><i class="fas fa-search"></i></button>`;


    var cols = [
        {
            "mData": 'messageType', "render": function (data, type, row) {
                return (data == 0 ? '<img src="\\assets-chatroom\\images\\channels\\Email.png" alt="Email.png" class="channel-icon">' : data == 1 ? '<img src="\\assets-chatroom\\images\\channels\\WhatsApp.png" alt="WhatsApp.png" class="channel-icon">' : data == 2 ? '<img src="\\assets-chatroom\\images\\channels\\SMS.png" alt="SMS.png" class="channel-icon">' : '');
            }
        },
        {
            "mData": 'status', "render": function (data, type, row) {
                let status = '';
                if (data != 1) {
                    status = `<span style='color:${data == 0 ? 'yellow' : data == 2 ? 'blue' : 'red'}'><i class="fas fa-circle"></i></span> `;
                    status = status + (data == 0 ? 'na fila' : data == 2 ? 'enviado' : 'cancelado');
                }
                else {
                    status = '<progress value="' + (row.qtdSent / row.qtdContacts * 100) + '" max="100"></progress>';
                }
                return status;
            }
        },
        { "mData": 'title' },
        {
            "mData": 'scheduledDate', "render": function (data, type, row) {
                if (type === "sort" || type === 'type') {
                    return data;
                }
                else {
                    if (data) {
                        return getBrasilianDateTime(new Date(data));
                    }
                    else
                        return "";
                }
            }
        },

        { "mData": 'qtdContacts' },
        { "mData": 'qtdSent' },
        {
            data: null,
            defaultContent: renderButtons,
            orderable: false,
            width: 70
        }];

    var tblDef = {
        data: data,
        searching: false,
        bJQueryUI: true,
        sPaginationType: 'full_numbers',
        aoColumns: cols,
        dom: 'Bfrtip',
        order: [[3,'desc']],
        language: {
            search: "Procurar",
            paginate: {
                first: "Início",
                last: "Fim",
                previous: "Anterior",
                next: "Proximo",
            }
        },
            columnDefs: [
                {
                    targets: [-2, -3],
                    className: "dt-right"
                }
            ]
    };

    grid = $('#sendings-grid').DataTable(tblDef);

    // Bind DELETE button event
    $('#sendings-grid tbody').on('click', '.btn-delete', function () {
        var row = $(this).parents('tr');
        var data = grid.row(row).data();
        id = data.id;
        deleteSending(id,row);
    });

    // Bind EDIT button event
    $('#sendings-grid tbody').on('click', '.btn-edit', function () {
        currentRow = $(this).parents('tr');
        var data = grid.row(currentRow).data();
        editSending(data);
    });

    // Bind Modal Edit button event
    $('#modal-sending').on('click', '.modal-edit-button', function () {
        id = $("#modal-sending-id").val();
        window.location.href = "/admin/editmailSending?id=" + id;
    });

    // Bind sendingType select change event
    $(document).on('change', '#modal-sending-type', function () {
        let sendingType = $(this).val();
        // Configura o select das Mensagens
        initMessageSelector(sendingType);
        // Configura o select dos canais
        initchannelSelector(sendingType);

    });

    // Bind board select change event
    $(document).on('change', '#modal-sending-board', function () {
        let boardId = $(this).val();
        initFilterSelector(boardId);
    });

    // Bind REPORT button event
    $('#sendings-grid tbody').on('click', '.btn-info', function () {
        currentRow = $(this).parents('tr');
        var sending = grid.row(currentRow).data();
        showReport(sending);
    });

}

/*
 * Show Report at modal
 */
function showReport(sending) {

    // Title
    $(".sending-report h1").html(sending.title);
    // Sent Date
    $("#modal-sending-report-sentdate").val(getBrasilianDateTime(sending.sentDate));

    // Get Report Data
    getAjax('/api/SendingsReport/' + sending.id, APIHeader, false, false).then((res) => {
        // Total
        $("#modal-sending-report-total").val(res.total);
        // Enviadas
        $("#modal-sending-report-sent").val(res.sent);
        // Entregues
        $("#modal-sending-report-delivered").val(res.delivered);
        // Lidas
        $("#modal-sending-report-read").val(res.read);
        // Falhas
        $("#modal-sending-report-failed").val(res.failed);

    });

    
}

/*
 * Validação de data
 */
function isValidDate(date) {
    d = new Date(date);
    if (Object.prototype.toString.call(d) === "[object Date]") {
        // it is a date
        return true;
        if (isNaN(d.getTime())) {  // d.valueOf() could also work
            // date is not valid
            return false;
        } else {
            // date is valid
            return false;
        }
    } else {
        // not a date
        return false;
    }
}

/*
 * Converte data hora padrão brasileiro no padrão americano ( para salvar no banco )
 */
function convertPtDateString(date) {
    var d1 = date.split(' ');
    var d2 = d1[0].split('/');

    return ( d2[2] + '/' + d2[1] + '/' + d2[0] + ' ' + d1[1] )
}
