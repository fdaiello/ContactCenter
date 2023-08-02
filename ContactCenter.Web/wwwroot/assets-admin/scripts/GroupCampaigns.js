/*
 * Script para Admin - Group Campaigns
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
$(document).on('click', '#modal-groupcampaign .btn-save-field', function () {

    $("#modal-groupcampaign-name").attr("style", "1px solid #ced4da");
    $("#modal-groupcampaign-channel").attr("style", "1px solid #ced4da");
    $("#modal-groupcampaign-MaxClicksPerGroup").attr("style", "1px solid #ced4da");
    $("#modal-groupcampaign-linksufix").attr("style", "1px solid #ced4da");
    $("#modal-groupcampaign-qtdGroups").attr("style", "1px solid #ced4da");
    $("#modal-groupcampaign-channel-msg").attr("style", "1px solid #ced4da");
    $("#modal-groupcampaign-GroupAdmins").attr("style", "1px solid #ced4da");

    var name = $("#modal-groupcampaign-name").val();
    var description = $("#modal-groupcampaign-description").val();
    var id = $("#modal-groupcampaign-id").val();
    var chatChannelId = $("#modal-groupcampaign-channel").val();
    var sendMsgChatChannelId = $("#modal-groupcampaign-channel-msg").val();
    var endDate = $("#modal-groupcampaign-enddate").val();
    var maxClicksPerGroup = $("#modal-groupcampaign-MaxClicksPerGroup").val();
    var qtdGroups = $("#modal-groupcampaign-qtdGroups").val();
    var linkSufix = $("#modal-groupcampaign-linksufix").val();
    var facePixelCode = $("#modal-groupcampaign-FacePixelCode").val();
    var googleAdsCode = $("#modal-groupcampaign-GoogleAdsCode").val();
    var closedUrl = $("#modal-groupcampaign-ClosedUrl").val();
    var imageFileName = $("#modal-groupcampaign-ImageFileName").val();
    var groupAdmins = $("#modal-groupcampaign-GroupAdmins").val();
    var permissions = $("#modal-groupcampaign-permissions").val();
    var welcomeMessageId = $("#welcomeMessageId").val();
    var groupWelcomeMessageId = $("#groupWelcomeMessageId").val();
    var leaveMessageId = $("#leaveMessageId").val();
    var firstGroupNumber = $("#modal-groupcampaign-FirstGroupNumber").val();
    var groupAction = $("#modal-groupcampaign-groupaction").val();

    // Inicializa o Id, se for nulo
    if (!id)
        id = 0;

    // Inicializa o Status, se for nulo
    if (!status)
        status = 0;

    // Validação dos campos obrigatorios
    if (name == '') {
        openTab(event, 'Dados');
        $("#modal-groupcampaign-name").attr("style", "border-color:red;");
        alert('Preencha o título');
        $("#modal-groupcampaign-name").focus();
        return false;
    }
    else if (chatChannelId == '') {
        openTab(event, 'Grupos');
        $("#modal-groupcampaign-channel").attr("style", "border-color:red;");
        alert('Escolha um canal para a criação e envio das mensagens de grupos');
        $("#modal-groupcampaign-channel").focus();
        return false;
    }
    else if (qtdGroups == '0' || qtdGroups == '') {
        openTab(event, 'Grupos');
        $("#modal-groupcampaign-qtdGroups").attr("style", "border-color:red;");
        alert('Informe quantos grupos serão criados inicialmente.');
        $("#modal-groupcampaign-qtdGroups").focus();
        return false;
    }
    else if (maxClicksPerGroup == '0') {
        openTab(event, 'Grupos');
        $("#modal-groupcampaign-MaxClicksPerGroup").attr("style", "border-color:red;");
        alert('Informe quantas vagas estarão disponíveis em cada grupo');
        $("#modal-groupcampaign-MaxClicksPerGroup").focus();
        return false;
    }
    else if (linkSufix == '') {
        openTab(event, 'Dados');
        $("#modal-groupcampaign-linksufix").attr("style", "border-color:red;");
        alert('Informe o nome do link para esta campanha');
        $("#modal-groupcampaign-linksufix").focus();
        return false;
    }
    else if (linkSufix.length < 5) {
        openTab(event, 'Dados');
        $("#modal-groupcampaign-linksufix").attr("style", "border-color:red;");
        alert('O nome do link precisa ter pelo menos 5 caracteres');
        $("#modal-groupcampaign-linksufix").focus();
        return false;
    }
    else if (linkSufix.includes(' ')) {
        openTab(event, 'Dados');
        $("#modal-groupcampaign-linksufix").attr("style", "border-color:red;");
        alert('O nome do link não pode ter espaços');
        $("#modal-groupcampaign-linksufix").focus();
        return false;
    }
    else if (sendMsgChatChannelId == '' && (welcomeMessageId != '' || leaveMessageId != '')) {
        openTab(event, 'Mensagens');
        $("#modal-groupcampaign-channel-msg").attr("style", "border-color:red;");
        alert('Escolha um canal para o envio das mensagens individuais.');
        $("#modal-groupcampaign-channel-msg").focus();
        return false;
    }
    else if (groupAdmins.length == 0 && groupAction == "0") {
        openTab(event, 'Grupos');
        $("#modal-groupcampaign-GroupAdmins").attr("style", "border-color:red;");
        $("#modal-groupcampaign-GroupAdmins").focus();
        alert('Informe um número de celular para participar do grupo e ser administrador.');
        return false;
    }
    else if (groupAdmins.length < 11 && groupAction == "0") {
        openTab(event, 'Grupos');
        $("#modal-groupcampaign-GroupAdmins").attr("style", "border-color:red;");
        $("#modal-groupcampaign-GroupAdmins").focus();
        alert('Informe um número de celular com DDD, para outro administrador.');
        return false;
    }
    else if (!imageFileName) {
        openTab(event, 'Dados');
        alert('Escolha uma imagem para a campanha.');
        return false;
    }

    // Validação da data de termino da campanha
    if (endDate)
        if (!isValidDate(endDate)) {
            alert('Data inválida');
            endDate.focus();
            return false;
        }

    // Cria um objeto GroupCampaign para ser salvo
    let groupCampaign = {
        id: id,
        name: name,
        description: description,
        chatChannelId: chatChannelId,
        sendMsgChatChannelId: sendMsgChatChannelId,
        endDate: endDate ? convertPtDateString(endDate) : null,
        maxClicksPerGroup: maxClicksPerGroup,
        linkSufix: linkSufix,
        facePixelCode: facePixelCode,
        googleAdsCode: googleAdsCode,
        closedUrl: closedUrl,
        imageFileName: imageFileName,
        groupAdmins: groupAdmins,
        permissions: permissions,
        welcomeMessageId: welcomeMessageId,
        groupWelcomeMessageId: groupWelcomeMessageId,
        leaveMessageId: leaveMessageId,
        closedUrl: closedUrl,
        groups: qtdGroups,
        firstGroupNumber: firstGroupNumber,
        groupAction: groupAction
    };

    SaveGroupCampaign(groupCampaign)

}); 

/*
 * Salva a campanha de grupos
 */
function SaveGroupCampaign(groupCampaign) {

    var method;
    var url = '/api/GroupCampaigns';
    if (groupCampaign.id != 0) {
        method = 'PUT';
        url += '/' + groupCampaign.id;
    }
    else
        method = 'POST'

    startSpinner();

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        headers: APIHeader,
        data: JSON.stringify(groupCampaign),
        success: function (newGroupCampaign) {
            if (method == 'POST') {
                // Append new row to table
                grid.row.add(newGroupCampaign); 
                grid.draw();
            }
            else {
                // Update grid row                
                grid.row(currentRow).data(newGroupCampaign);
                grid.draw();
            }
            // Close modal
            $('#modal-groupcampaign').fadeOut(1000);
            $('.modal-close-button').trigger('click');

            stopSpinner();
        },
        error: function (xhr, textStatus, errorThrown) {
            stopSpinner();
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

/*
 * Clear GroupCampaign Modal
 * @param type 1 - create, 2 - edit
 */
function clearGroupCampaignModal() {

    // Limpa os campos
    $("#modal-groupcampaign-name").val("");
    $("#modal-groupcampaign-description").val("");
    $("#modal-groupcampaign-id").val("0");
    $("#modal-groupcampaign-channel").val("");
    $("#modal-groupcampaign-channel-msg").val("");
    $("#modal-groupcampaign-enddate").val("");
    $("#modal-groupcampaign-MaxClicksPerGroup").val("200");
    $("#modal-groupcampaign-qtdGroups").val("1");
    $("#modal-groupcampaign-linksufix").val("");
    $("#modal-groupcampaign-FacePixelCode").val("");
    $("#modal-groupcampaign-GoogleAdsCode").val("");
    $("#modal-groupcampaign-closedUrl").val("");
    $("#modal-groupcampaign-ImageFileName").val("");
    $('#attached-file').val("");
    $("#modal-groupcampaign-GroupAdmins").val("");
    $("#modal-groupcampaign-permissions").val("0");
    $("#modal-groupcampaign-groupaction").val("0");
    $("#modal-groupcampaign-FirstGroupNumber").val("1");
    $("#modal-groupcampaign-FirstGroupNumber").removeAttr('disabled');

    // Imagem
    $(".attached-view").html("<img src=\"/images/picture-icon.png\" title=\"Clique para enviar uma imagem.\" />");

    // Titulo da tela
    $("#groupcampaignModalLabel").html("Nova campanha");

    // Init selectors with selected option
    initchannelSelector();

    // Init Welcome and LeaveGroup Message selectors
    let messageTypeWhatsApp = 1
    initMessageSelectors(messageTypeWhatsApp);

    // Abre a primeira aba
    openTab(event, 'Dados');
}

function editGroupCampaign(groupCampaign) {

    // Fill modal fields with data from the row
    $("#modal-groupcampaign-name").val(groupCampaign.name);
    $("#modal-groupcampaign-description").val(groupCampaign.description);
    $("#modal-groupcampaign-id").val(groupCampaign.id);
    $("#modal-groupcampaign-channel").val(groupCampaign.chatChannelId);
    $("#modal-groupcampaign-channel-msg").val(groupCampaign.sendMsgChatChannelId);
    $("#modal-groupcampaign-enddate").val(groupCampaign.endDate ? getBrasilianDateTime(new Date(groupCampaign.endDate)) : '');
    $("#modal-groupcampaign-MaxClicksPerGroup").val(groupCampaign.maxClicksPerGroup);
    $("#modal-groupcampaign-qtdGroups").val(groupCampaign.groups);
    $("#modal-groupcampaign-linksufix").val(groupCampaign.linkSufix);
    $("#modal-groupcampaign-FacePixelCode").val(groupCampaign.facePixelCode);
    $("#modal-groupcampaign-GoogleAdsCode").val(groupCampaign.googleAdsCode);
    $("#modal-groupcampaign-closedUrl").val(groupCampaign.closedUrl);
    $("#modal-groupcampaign-ImageFileName").val(groupCampaign.imageFileName);
    $("#modal-groupcampaign-GroupAdmins").val(groupCampaign.groupAdmins);
    $("#modal-groupcampaign-permissions").val(groupCampaign.permissions);
    $("#modal-groupcampaign-FirstGroupNumber").val(groupCampaign.firstGroupNumber);
    $("#modal-groupcampaign-FirstGroupNumber").attr("disabled", 'disabled');
    $("#modal-groupcampaign-groupaction").val(groupCampaign.groupAction);

    // Titulo da tela
    $("#groupcampaignModalLabel").html("Editar campanha");

    // Se tem arquivo
    if (groupCampaign.imageFileName) {
        // Coloca a imagem na tela
        var out = $(".attached-view");
        var img = document.createElement("img");
        img.src = groupCampaign.imageFileName;
        out.html(img);
    }
    else {
        $(".attached-view").html ("<img src=\"/images/picture-icon.png\" title=\"Clique para enviar uma imagem.\" />");
    }

    // Init selectors with selected option
    initchannelSelector(groupCampaign);

    // Init Welcome and LeaveGroup Message selectors
    let messageTypeWhatsApp = 1
    initMessageSelectors(messageTypeWhatsApp, groupCampaign.groupWelcomeMessageId, groupCampaign.welcomeMessageId, groupCampaign.leaveMessageId);

}

/**
 * Init Chat Channel selector
 * 
 *     Group messages channel
 *     Notification messages channel
 * 
 */
function initchannelSelector(groupCampaign) {

    getAjax('/api/chatChannels', APIHeader, false, false).then((res) => {
        // Primeiro configura o select do canal principal que será usado para criar os links dos grupos
        let channelSelector = $('#modal-groupcampaign-channel');
        let strHTML = '';
        let foundChannel = false;
        let channelCount = 0;
        res.forEach((channel) => {
            // So da opção de escolher os canais do subtipo Alternate1 - Wassenger - Id = 2
            if (channel.channelSubType == 2 ) {
                strHTML += '<option value="' + channel.id + '"' + (channel.id == groupCampaign?.chatChannelId ? ' selected' : '') + '>' + channel.name + '</option>';
                foundChannel = true;
                channelCount ++
            }
        });

        // If there are more than one channel
        if (channelCount > 1 ) {
            // Set default empty option
            strHTML = '<option></option>' + strHTML;
        }

        if (!foundChannel > 0) {
            alert("Sua conta ainda não tem nenhum canal configurado para este tipo de campanha!");
        }

        // Preenche os valores do select
        channelSelector.html(strHTML);

        // Agora configura o select do canal que será usado para enviar as mensagens automáticas individuais de entrada e saída do grupo
        let channelSelectorMsg = $('#modal-groupcampaign-channel-msg');
        strHTML = '';
        channelCount = 0;

        res.forEach((channel) => {
            // So da opção de escolher os canais do tipo WhatsApp Wassenger ou MayTapi
            if (channel.channelSubType == 2 || channel.channelSubType == 3) {
                strHTML += '<option value="' + channel.id + '"' + (channel.id == groupCampaign?.sendMsgChatChannelId ? ' selected' : '') + '>' + channel.name + '</option>';
                foundChannel = true;
                channelCount++
            }
        });

        // If there are more than one channel
        if (channelCount > 1) {
            // Set default empty option
            strHTML = '<option></option>' + strHTML;
        }

        // Preenche os valores do select do canal das mensagens
        channelSelectorMsg.html(strHTML);

    });
}

/**
 * Init Welcome and LeaveGroup Message selectors
 * 
 */
function initMessageSelectors(sendingType, groupWelcomeMessageId, welcomeMessageId, leaveMessageId) {

    let welcomeMessageSelector = $('#welcomeMessageId');
    let groupWelcomeMessageSelector = $('#groupWelcomeMessageId');
    let leaveMessageSelector = $('#leaveMessageId');

    getAjax('/api/Messages', APIHeader, false, false).then((res) => {
        let welcomeMessageHTML = '<option value=""></option>';
        let leavegroupMessageHTML = welcomeMessageHTML;
        let groupWelcomeMessageHTML = welcomeMessageHTML;
        res.forEach((message) => {
            if (message.messageType == sendingType) {
                welcomeMessageHTML += '<option value="' + message.id + '"' + (message.id == welcomeMessageId ? ' selected' : '') + '>' + message.title + '</option>';
                groupWelcomeMessageHTML += '<option value="' + message.id + '"' + (message.id == groupWelcomeMessageId ? ' selected' : '') + '>' + message.title + '</option>';
                leavegroupMessageHTML += '<option value="' + message.id + '"' + (message.id == leaveMessageId ? ' selected' : '') + '>' + message.title + '</option>';
            }
        });
        welcomeMessageSelector.html(welcomeMessageHTML);
        groupWelcomeMessageSelector.html(groupWelcomeMessageHTML);
        leaveMessageSelector.html(leavegroupMessageHTML);
    });
}


function deleteGroupCampaign(id,row) {

    $.ajax({
    url: '/api/GroupCampaigns/' + id,
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
        url: '/api/GroupCampaigns',
        headers: APIHeader,
        type: 'GET',
        success: function (data) {
            populateGrid(data);
            stopSpinner();
            // Se tem alguma linha com status inicializando
            if (hasInitRow(data))
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
    $('#modal-groupcampaign-enddate').datetimepicker({
        format: 'd/m/Y H:i'
    });
    $.datetimepicker.setLocale('pt-BR');

    // Bind função do evento de tratamento do arquivo enviado
    bindAttachedFileChange()

});

function populateGrid(data) {

    var renderButtons = `<button class="btn btn-danger btn-delete" title="Excluir a campanha"><i class="fa fa-trash"></i>
                     <button class="btn btn-warning btn-edit" data-toggle="modal" data-target="#modal-groupcampaign" title="Editar a campanha"><i class="fa fa-edit"></i></button>
                     <button class="btn btn-info" data-toggle="modal" data-target="#modal-groupcampaign-report" title="Visualizar o relatório de clicks"><i class="fas fa-search"></i></button>
                     <button class="btn btn-success btn-whatsgroups" data-toggle="modal" data-target="#modal-groupcampaign-whatsgroups" title="Visualizar os grupos"><i class="fas fa-users"></i></button>`;


    var cols = [
        {
            "mData": 'imageFileName', "render": function (data, type, row) {
                if (data)
                    return data ? '<img src="' + data + '" class="groupCampaignImage">' : '';
                else
                    return '';
            }
        },
        {
            "mData": 'status', "render": function (data, type, row) {
                status = `<span style='color:${data == 0 ? 'blue' : data == 1 ? 'green' : data == 2 ? 'yellow' : data =='3' ? 'red' : 'orange'}; margin-right:3px' class='tooltip1'><i class="fas fa-circle"></i> `;
                if (row.obs)
                    status += '<span class="tooltiptext gridtooltip">' + row.obs + '</span></span>';
                status = status + (data == 0 ? "config" : data == 1 ? "ativa" : data == 2 ? "finalizada" : data == 3 ? "erro" : "");
                return status;
            }
        },
        { "mData": 'name' },
        {
            "mData": 'createdDate', "render": function (data, type, row) {
                if (type === "sort" || type === 'type') {
                    return data;
                }
                else {
                    if (data) {
                        return getBrasilianShortDateTime(new Date(data));
                    }
                    else
                        return "";
                }
            }},

        {
            "mData": 'linkSufix', "render": function (data, type, row) {
                let link = "smart-page.cc/" + data
                return "<a href='https://" + link + "' target='_new'>" + link + "</a>";
            }
        },
        { "mData": 'groups' },
        { "mData": 'clicks' },
        { "mData": 'leads' },
        { "mData": 'members' },
        {
            data: null,
            defaultContent: renderButtons,
            orderable: false,
            className: "text-right"
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
        }
    };

    // Cria a grid das GroupCampaigns
    grid = $('#groupcampaigns-grid').DataTable(tblDef);

    // Inicializa a grid dos grupos da campanha
    if (data[0] && data[0].whatsGroups)
        initWhatsGroupsGrid(data[0].whatsGroups);

    // Bind DELETE button event
    $('#groupcampaigns-grid tbody').on('click', '.btn-delete', function () {
        var row = $(this).parents('tr');
        var data = grid.row(row).data();
        id = data.id;
        deleteGroupCampaign(id,row);
    });

    // Bind EDIT button event
    $('#groupcampaigns-grid tbody').on('click', '.btn-edit', function () {
        currentRow = $(this).parents('tr');
        var data = grid.row(currentRow).data();
        editGroupCampaign(data);
    });

    // Bind REPORT button event
    $('#groupcampaigns-grid tbody').on('click', '.btn-info', function () {
        currentRow = $(this).parents('tr');
        var groupCampaign = grid.row(currentRow).data();
        showGcReport(groupCampaign);
    });

    // Bind WhatsGroups button event
    $('#groupcampaigns-grid tbody').on('click', '.btn-whatsgroups', function () {
        currentRow = $(this).parents('tr');
        var groupCampaign = grid.row(currentRow).data();
        reloadWhatsGroupsGrid(groupCampaign);
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

/*
 * Message attached file button click event
 * Fires hidden input field click
 */
function messageAttachedClick() {
    $('#attached-file').trigger('click')
}

function bindAttachedFileChange() {

    // Bind attached-file change event
    $('#attached-file').on('change', function () {

        var file = this.files[0];
        var image = new Image();

        image.onload = function () {
            // Check if image is bad/invalid
            if (this.width + this.height === 0) {
                this.onerror();
                return;
            }

            // Check the image resolution anb size
            if (this.width <= 192 && this.height <= 192) {
                alert("A imagem é muito pequena. Por favor escolha uma imagem com pelo menos 192 x 192 pixels de resolução.");
            } else if (file.size >= 1024 * 1024) {
                alert("A imagem é muito grande. Por favor escolha um arquivo com no máximo 1Mb de tamanho.");
            }
            else {
                // Exibe o arquivo enviado
                preview(file);

                // Form data para salvar o arquivo via POST
                var formData = new FormData();
                formData.append('file', file);

                //Salva o arquivo
                postAjax('/api/Files', APIHeader, formData, true, false, false, false, false)
                    .then(
                        (res) => {
                            // Salva o nome do arquivo recebido, no campo escondido do formulário
                            $("#modal-groupcampaign-ImageFileName").val(res.fileName);
                        }
                    );
            }
        };

        image.onerror = function () {
            alert("Imagem inválida. Por favor escolha outro tipo de imagem.");
            deferred.resolve(false);
        }

        var _URL = window.URL || window.webkitURL;
        image.src = _URL.createObjectURL(file);

    });

}

/*
 * Mostra o arquivo enviado
 */
//Função de preview de arquivos no chat
function preview(file) {
    //Previne possíveis conflitos com a função createObjectURL
    var getBlobUrl = (window.URL && URL.createObjectURL.bind(URL)) ||
        (window.webkitURL && webkitURL.createObjectURL.bind(webkitURL)) ||
        window.createObjectURL;

    var out = $(".attached-view");
    var img = document.createElement("img");
    img.src = getBlobUrl(file); //getBlobUrl transforma o files, em formato blob, para uma url válida a ser usada no src de preview
    out.html(img);
}

/* 
 * Confere se tem alguma linha com status 0 - inicializando
 */
function hasInitRow(data) {
    let has = false;
    data.forEach(function (row) {
        if (row.status == 0)
            has = true;
    })
    return has;
}
/*
 * Chama a API para ler os dados da Grid
 * Chama a rotina que preenche a Grid com os dados
 */
function reloadGridData() {
    // Get data from API
    $.ajax({
        url: '/api/GroupCampaigns',
        headers: APIHeader,
        type: 'GET',
        success: function (data) {
            let datatable = $('#groupcampaigns-grid').DataTable();
            datatable.clear();
            datatable.rows.add(data);
            datatable.draw();
            // Se tem alguma linha com status importando
            if (hasInitRow(data))
                // Recarrega a Grid depois de 5 segundos
                setTimeout(() => {
                    reloadGridData();
                }, 5000);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })
}


/*
 * Show Group Campaign Report at modal
 */
function showGcReport(groupCampaign) {

    var presets = window.chartColors;
    var utils = Samples.utils;

    // Title
    $(".groupcampaign-report h1").html(groupCampaign.name);

    // Get Report Data
    getAjax('/api/groupCampaigns/' + groupCampaign.id + "/PageViewsDay", APIHeader, false, false).then((pageViewsDay) => {

        var ctx = document.getElementById('chart-groupcampaign-pageviews-by-day').getContext('2d');
        var myChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: getPageViewsByDayLabels(pageViewsDay),
                datasets: [{
                    label: 'Clicks por dia',
                    data: getPageViewsByDayData(pageViewsDay),
                    backgroundColor: utils.transparentize(presets.red),
                    borderColor: presets.red,
                    borderWidth: 1
                }]
            },
            options: {
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });

    });

    // Devolve array com os dias - para o eixo X - Labels
    function getPageViewsByDayLabels(pageViewsDay) {
        var labels = [];
        for (var i = 0; i < pageViewsDay.length; i++) {
            labels.push(pageViewsDay[i].dia);
        }
        return labels;
    }
    // Devolve array com os valores - para o eixo Y - Click por dia
    function getPageViewsByDayData(pageViewsDay) {
        var data = [];
        for (var i = 0; i < pageViewsDay.length; ++i) {
            data.push(pageViewsDay[i].pageViews);
        }
        return data;
    }
}

/*
 * Tabs do formulário
 */
function openTab(evt, tabName) {
    // Declare all variables
    var i, tabcontent, tablinks;

    // Get all elements with class="tabcontent" and hide them
    tabcontent = document.getElementsByClassName("tabcontent");
    for (i = 0; i < tabcontent.length; i++) {
        tabcontent[i].style.display = "none";
    }

    // Get all elements with class="tablinks" and remove the class "active"
    tablinks = document.getElementsByClassName("tablinks");
    for (i = 0; i < tablinks.length; i++) {
        tablinks[i].className = tablinks[i].className.replace(" active", "");
    }

    // Show the current tab, and add an "active" class to the button that opened the tab
    document.getElementById(tabName).style.display = "block";
    evt.currentTarget.className += " active";
}

/*
 * Configura a grid dos WhatsGroups da campanha, na modal
 */
function initWhatsGroupsGrid(groupCampaign) {

    var cols = [
        { "mData": 'name' },
        { "mData": 'inviteUrl' },
        { "mData": 'clicks' },
        { "mData": 'leads' },
        { "mData": 'members' }
    ];

    var tblDef2 = {
        data: groupCampaign.whatsGroups,
        searching: false,
        bJQueryUI: true,
        sPaginationType: 'full_numbers',
        aoColumns: cols,
        dom: 'Bfrtip',
        order: [[2, 'desc']],
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

    grid2 = $('#whatsgroups-grid').DataTable(tblDef2);
}
/*
 * Recarrega a grid dos whats groups, na modal
 */
function reloadWhatsGroupsGrid(groupCampaign) {

    let datatable = $('#whatsgroups-grid').DataTable();
    datatable.clear();
    datatable.rows.add(groupCampaign.whatsGroups);
    datatable.draw();
}