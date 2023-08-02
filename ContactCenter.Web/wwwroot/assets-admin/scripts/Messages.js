/*
 * Script para Admin - CRM - Messages
 * @author: Felipe Daiello
 */

var grid;
var currentRow;
var message;
var smartUrl;

var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};

/**
 * Save fields at modal to DB
 * */
$(document).on('click', '#modal-message .btn-save-field', function () {

    var messageType = $("#modal-message-type").val();
    var title = $("#modal-message-title").val();
    var id = $("#modal-message-id").val();
    var fileUri = $("#modal-message-fileuri").val();
    var createdDate = $("#modal-message-createddate").val();

    var content
    if (messageType!="0")
        content = $('.emojionearea .emojionearea-editor')[0].innerHTML;
    
    if (!id)
        id = 0;

    // Validação dos campos obrigatorios
    if (messageType == '') {
        $("#modal-message-type").attr("style", "border-color:red;");
        $("#modal-message-type").focus();
        return false;
    }
    else if (title == '') {
        $("#modal-message-title").attr("style", "border-color:red;");
        $("#modal-message-title").focus();
        return false;
    }

    // Confere se tem open tags
    if ( content && (content.match(/\[/g) || []).length != (content.match(/\]/g) || []).length) {
        alert("Ficou uma tag de customização corrompida, por favor, remova a tag de customização incompleta. As tags de customização tem que começar com colchetes e terminar com colchetes, se remover os colchetes, vai dar erro ao processar a mensagem.")
        return false;
    }

    // Se for SMS
    if (messageType == "2") {
        // Remove as tags de negrito - usadas nos Chips de personalização.
        let msgText = content.replaceAll("<span>", "").replaceAll("</span>","01234567890").replaceAll("&nbsp;", " ").replaceAll("<div>", "").replaceAll("</div>", "").replaceAll("<br>", " ")
        if (msgText.length > 304) {
            alert("Mensagem de SMS pode ter no máximo 304 caracteres. Sua mensagem está com " + content.length);
            return false;
        }
        else if (msgText.length > 160) {
            alert("Atenção! Esta mensagem tem mais de 160 caracteres, e usará 2 créditos em cada envio.");
        }
        else if (!isGsm(msgText)) {
            alert("Mensagens de SMS só podem ter texto sem caracteres especiais.");
            return false;
        }

    }

    //// Se for WhatsApp
    //if (messageType == "1") {
    //    // Tem que ter pelo menos uma TAG de personalização
    //    if (!content.includes('[') || !content.includes(']')) {
    //        alert("Insira uma tag de personalizaçao, para não serem enviadas várias mensagens iguais, o que pode bloquear o envio.");
    //        return false;
    //    }
    //}

    let message = {
        id: id,
        messageType: messageType,
        title: title,
        content: content,
        fileUri: fileUri,
        createdDate: createdDate
    };

    // Salva a mensagem
    SaveMessage(message)

    // Fecha a modal
    $(".modal-close-button")[0].click();
}); 

// Valida um texto se contém caracteres do SMS
function isGsm(text) {

    //GSM não tem colchetes, mas tive que deixar por causa das tags de personalização - serão removidos pelo enviador SMS
    var gsmChars = " _!\"#¤%&'()*+,-./0123456789:;<=>?ABCDEFGHIJKLMNOPQRSTUVWXYZ§¿abcdefghijklmnopqrstuvwxyz@£$¥][Z\n\r";

    for (var i = 0; i < text.length; i++) {
        if (!gsmChars.includes(text.charAt(i))) {
            return false;
        }
    }

    return true;
}

function SaveMessage(message) {

    var method;
    var url = '/api/Messages';
    if (message.id != 0) {
        method = 'PUT';
        url += '/' + message.id;
    }
    else
        method = 'POST'

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        headers: APIHeader,
        data: JSON.stringify(message),
        success: function (newMessage) {
            if (method == 'POST') {
                // Append new row to table
                grid.row.add(newMessage);
                grid.draw();
            }
            else {
                // Update grid row                
                grid.row(currentRow).data(message).draw();

            }
            // Close modal
            $('#modal-message').fadeOut(1000);
            $('.modal-close-button').trigger('click');
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

/*
 * Clear Message Modal
 * @param type 1 - create, 2 - edit
 */
function clearMessageModal() {

    $("#modal-message-type").attr("style", "");
    $("#modal-message-title").attr("style", "");

    $("#boardStageModalLabel").html("Criar mensagem");

    $("#modal-message-content").html("");
    $(".emojionearea-editor").html("");
    $("#modal-message-type").val("");            
    $("#modal-message-title").val("");
    $("#modal-message-fileuri").val("");
    $("#modal-message-id").val("0");
    $("#modal-message-link").html(""); 

    $('#modal-message-content-counter').val("0");

    // Imagem
    $(".attached-view").html("<img src=\"/images/picture-icon.png\" title=\"Clique para enviar uma imagem.\" />");

    // Limpa global que contem o link
    smartUrl = "";

    // Configura os campos
    configFields()

    // Esconde o link desta mensagem
    $("#div-modal-message-link").addClass('hide');

    // Bind função do evento de tratamento do arquivo enviado
    bindAttachedFileChange()
}

function messageTypeChange(e) {

    e.preventDefault();

    // Configura os campos
    configFields()

    // Bind função do evento de tratamento do arquivo enviado
    bindAttachedFileChange()

}
function editMessage(data) {

    // Modal label
    $("#boardStageModalLabel").html("Editar mensagem");

    // Fill modal fields with data from the row
    $("#modal-message-content").html(data.content);
    $('.emojionearea .emojionearea-editor').html(data.content);
    $("#modal-message-id").val(data.id);
    $("#modal-message-type").val(data.messageType);
    $("#modal-message-title").val(data.title);
    $("#modal-message-fileuri").val(data.fileUri);
    $("#modal-message-createddate").val(data.createdDate);

    // Link de smart page,
    if (data.smartCode) {
        let link = "https://smart-page.cc/" + data.smartCode;
        let anchor = "<a href='" + link + "' target='_new'>" + link + "</a>";
        $("#modal-message-link").html(anchor);
    }
    else
        $("#modal-message-link").html("");

    // Message content lenght
    $('#modal-message-content-counter').val($('.emojionearea .emojionearea-editor')[0].innerHTML.replaceAll("<span>", "").replaceAll("</span>", "01234567890").replaceAll("&nbsp;", " ").replaceAll("<div>", "").replaceAll("</div>", "").replaceAll("<br>", " ").length)

    // Se tem arquivo
    if (data.fileUri) {
        // Busca a URL completa do arquivo
        getAjax('/api/Files?filename=' + data.fileUri, APIHeader, false, false).then((res) => {
            // Coloca a imagem na tela
            var out = $(".attached-view");
            var img = document.createElement("img");
            img.src = res.url;
            out.html(img);
            // mostra a imagem
            $(".attached-view").css("display", "block");
        });
    }
    else {
        // Imagem
        $(".attached-view").html("<img src=\"/images/picture-icon.png\" title=\"Clique para enviar uma imagem.\" />");
    }

    // Marca no atributo do botão que estamos editando - data-type=2
    $('#modal-message .btn-save-field').attr('data-type', 2);

    // Configura os campos
    configFields()

    // Bind função do evento de tratamento do arquivo enviado
    bindAttachedFileChange()
}

function configFields() {

    var messageType = $("#modal-message-type").val();

    // Se o tipo for email
    if (messageType === "0") {
        // Esconde o conteudo
        $("#message-content").addClass('hide');
        // Esconde a div de envio de imagem
        $("#div-modal-message-image").addClass('hide');
        // Mostra o botão Editar
        $(".modal-edit-button").removeClass('hide');
        // Esconde o seletor de emojis
        $(".emojionearea-button-open").addClass('hide');
        // Esconde o contador de caracteres
        $("#div-modal-message-content-counter").addClass('hide');
        // Mostra o link desta mensagem - o link da smart page que pode ser inserido em outra mensagem
        if ($("#modal-message-link").html())
            $("#div-modal-message-link").removeClass('hide');
    }
    // Social
    else if (messageType == 1 ) {
        // Mostra o conteudo
        $("#message-content").removeClass('hide');
        // Mostra a div de enviar imagem
        $("#div-modal-message-image").removeClass('hide');
        // Esconde o botão Editar
        $(".modal-edit-button").addClass('hide');
        // Mostra o seletor de emojis
        $(".emojionearea-button-open").removeClass('hide');
        // Esconde o contador de caracteres
        $("#div-modal-message-content-counter").addClass('hide');
        // Esconde o link desta mensagem
        $("#div-modal-message-link").addClass('hide');
        // mostra o botão negrito
        $("#modal-message-btn-bold").removeClass('hide');
    }
    // SMS
    else if ( messageType == 2) {
        // Mostra o conteudo
        $("#message-content").removeClass('hide');
        // Esconde o botão Editar
        $(".modal-edit-button").addClass('hide');
        // Esconde a div de enviar imagem
        $("#div-modal-message-image").addClass('hide');
        // Esconde o seletor de emojis
        $(".emojionearea-button-open").addClass('hide');
        // Mostra o contador de caracteres
        $("#div-modal-message-content-counter").removeClass('hide');
        // Esconde o link desta mensagem
        $("#div-modal-message-link").addClass('hide');
        // Esconde o botão negrito
        $("#modal-message-btn-bold").addClass('hide');
    }
    else {
        // Esconde o botão Editar
        $(".modal-edit-button").addClass('hide');
        // Esconde o seletor de emojis
        $(".emojionearea-button-open").addClass('hide');
        // Esconde o contador de caracteres
        $("#div-modal-message-content-counter").addClass('hide');
        // Esconde o link desta mensagem
        $("#div-modal-message-link").addClass('hide');
        // Esconde a div de enviar imagem
        $("#div-modal-message-image").addClass('hide');
    }
}

// Add Emoji Button to message text area
function enableEmojionarea() {
    // Chek if its not already enabled
    let emojieditor = $('.emojionearea .emojionearea-editor')
    if (!emojieditor.length) {
        // Enable EMOJI Editor
        $("#modal-message-content").emojioneArea({
            pickerPosition: "top",
            shortcuts: false,
            autocomplete: false,
            buttonTitle: "Clique para inserir Emojis!"
        });
    }
}

/*
 * Exclui uma mensagem
 */
function deleteMessage(id,row) {

    $.ajax({
    url: '/api/Messages/' + id,
        type: 'DELETE',
        headers: APIHeader,
        contentType: 'application/json',
        success: function () {
            // Delete row from grid
            grid.row(row).remove().draw();
        },
        error: function (xhr, textStatus, errorThrown) {
            if (xhr.responseText.includes("FK_Sendings_Messages_MessageId")) {
                alert("Não é possível excluir esta mensagem, pois há um envio que depende dela.")
            }
            else {
                alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
            }
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
        url: '/api/Messages',
        headers: APIHeader,
        type: 'GET',
        success: function (data) {
            populateGrid(data);
            stopSpinner();
        },
        error: function (xhr, textStatus, errorThrown) {
            stopSpinner();
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })

    // Ativa o editor de Emojis
    enableEmojionarea();

    // Bind Modal Edit message button event
    $('#modal-message').on('click', '.modal-edit-button', function () {
        let id = $("#modal-message-id").val();
        window.location.href = "/admin/editmailmessage?id=" + id;
    });

    // Bind Modal show custom tags picker event
    $('#modal-message').on('mousedown', '#modal-message-btn-personalizar', function (event) {
        $('#modal-message-customfieldpicker').removeClass('hide');
        event.preventDefault();
    });

    // Bind Modal show link picker event
    $('#modal-message').on('mousedown', '#modal-message-btn-addlink', function (event) {
        $('#modal-message-linkpicker').removeClass('hide');
        event.preventDefault();
    });

    // Bind Modal Bold button
    $('#modal-message').on('mousedown', '#modal-message-btn-bold', function () {
        document.execCommand('bold', false, null);
    });

    // Bind event to hide custom tag picker and link picker
    $('#modal-message-title').focusin(function () {
        $('#modal-message-customfieldpicker').addClass('hide');
        $('#modal-message-linkpicker').addClass('hide');
    })

    // Fill contact fields personalization options
    fillContactFields();

    // Fill smart-page link modal selector
    fillLinkSelector();

});

function populateGrid(data) {

    var renderButtons = `<button class="btn btn-danger btn-delete" title="Excluir mensagem"><i class="fa fa-trash"></i>
                     <button class="btn btn-warning btn-edit" data-toggle="modal" data-target="#modal-message" title="Editar mensagem"><i class="fa fa-edit"></i></button>`;

    var drawButton = '<button class="btn btn-success btn-draw" title="Modo de Design"><i class="far fa-file-image"></i></button>';

    var cols = [
        {
            "mData": 'messageType', "render": function (data, type, row) {
                return (data == 0 ? '<img src="\\assets-chatroom\\images\\channels\\Email.png" alt="Email.png" class="channel-icon">' : data == 1 ? '<img src="\\assets-chatroom\\images\\channels\\WhatsApp.png" alt="WhatsApp.png" class="channel-icon">' : data == 2 ? '<img src="\\assets-chatroom\\images\\channels\\SMS.png" alt="SMS.png" class="channel-icon">' : '' );
            }
        },
        { "mData": 'title' },
        {
            "mData": 'createdDate', "render": function (data, type, row) {
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
        {
            data: null,
            "render": function (data, type, row) {
                if (row.messageType == 0)
                    return drawButton + renderButtons;
                else
                    return renderButtons;
            },
            orderable: false,
            className: 'align-right'
        }];

    var tblDef = {
        data: data,
        searching: false,
        bJQueryUI: true,
        sPaginationType: 'full_numbers',
        aaSorting: [[2, 'desc']],
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

    grid = $('#messages-grid').DataTable(tblDef);

    // Bind grid DELETE button event
    $('#messages-grid tbody').on('click', '.btn-delete', function () {
        currentRow = $(this).parents('tr');
        data = grid.row(currentRow).data();
        let id = data.id;
        deleteMessage(id, currentRow);
    });

    // Bind grid EDIT button event
    $('#messages-grid tbody').on('click', '.btn-edit', function () {
        currentRow = $(this).parents('tr');
        data = grid.row(currentRow).data();
        editMessage(data);
    });

    // Bind grid DRAW button event
    $('#messages-grid tbody').on('click', '.btn-draw', function () {
        currentRow = $(this).parents('tr');
        data = grid.row(currentRow).data();
        // If page already has some content
        if (data.html)
            // Jump to Edit page
            window.location.href = "/admin/editMailMessage?messageId=" + data.id;
        else
            // Jump to pick templates page
            window.location.href = "/admin/TemplatesView?messageId=" + data.id;
    });

}

/* 
 * Insere um trecho de codigo na posição do cursor
 */
function pasteHtmlAtCaret(html) {
    var sel, range;
    if (window.getSelection) {
        // IE9 and non-IE
        sel = window.getSelection();
        if (sel.getRangeAt && sel.rangeCount) {
            range = sel.getRangeAt(0);
            range.deleteContents();

            // Range.createContextualFragment() would be useful here but is
            // non-standard and not supported in all browsers (IE9, for one)
            var el = document.createElement("div");
            el.innerHTML = html;
            var frag = document.createDocumentFragment(), node, lastNode;
            while ((node = el.firstChild)) {
                lastNode = frag.appendChild(node);
            }
            range.insertNode(frag);

            // Preserve the selection
            if (lastNode) {
                range = range.cloneRange();
                range.setStartAfter(lastNode);
                range.collapse(true);
                sel.removeAllRanges();
                sel.addRange(range);
            }
        }
    } else if (document.selection && document.selection.type != "Control") {
        // IE < 9
        document.selection.createRange().pasteHTML(html);
    }
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
        // Exibe o arquivo enviado
        preview(this.files[0]);

        // Form data para salvar o arquivo via POST
        var formData = new FormData();
        formData.append('file', $(this).prop('files')[0]);

        //Salva o arquivo
        postAjax('/api/Files', APIHeader, formData, true, false, false, false, false)
            .then(
                (res) => {
                    // Salva o nome do arquivo recebido, no campo escondido do formulário
                    $("#modal-message-fileuri").val(res.fileName);
                } 
            );

    });

}

/*
 * Mostra o arquivo enviado
 */
//Função de preview de arquivos no chat
function preview(file) {
    $(".attached-view").css("display", "block");

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
 * Preeche a div com as opções de campos personalizados
 * Busca via API todos os ContactFields disponíveis
 * Adiciona no codigo HTML da div com as tags correts
 */
function fillContactFields() {

    // Query Api
    $.ajax({
        url: '/api/ContactFields',
        type: 'GET',
        contentType: 'application/json',
        headers: APIHeader,
        success: function (contactFields) {
            let innerHTML = $('#modal-message-customfieldpicker').html();
            // Para cada contact field retornado
            contactFields.forEach((contactField) => {
                // Monta o codigo html
                if ( contactField.enabled )
                    innerHTML += `<div class="fieldpicker" data-name="${contactField.field.label}" role="button">${contactField.field.label}</div>`;
            });
            // Acrescenta o codigo HTML gerado a div #modal-message-customfieldpicker
            $('#modal-message-customfieldpicker').html(innerHTML);

            // Bind Modal Personalizar button event
            $('.fieldpicker').on('mousedown', function (event) {
                $('.emojionearea .emojionearea-editor').focus();
                let tag = "<span>[" + this.dataset.name + "]</span>&nbsp;";
                // Insere a tag na posição do cursorAcrecenta no final
                pasteHtmlAtCaret(tag);
                $('#modal-message-customfieldpicker').addClass('hide');
                event.preventDefault();
                $('#modal-message-content-counter').val($('.emojionearea .emojionearea-editor')[0].innerHTML.replaceAll("<span>", "").replaceAll("</span>", "01234567890").replaceAll("&nbsp;", " ").replaceAll("<div>", "").replaceAll("</div>", "").replaceAll("<br>", " ").length)
            });

        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });

}
/*
 * Fill smart-page Link modal selector
 */
function fillLinkSelector() {
    getAjax('/api/Landings', APIHeader, false, false).then((res) => {
        let innerHTML = "";
        res.forEach((landing) => {
            innerHTML += `<div class="linkpicker" data-name="${landing.code}" role="button">${landing.title}</div>`;
        });

        // Acrescenta o codigo HTML gerado a div #modal-message-customfieldpicker
        $('#modal-message-linkpicker').html(innerHTML);

        // Bind Modal add-link button event
        $('.linkpicker').on('mousedown', function (event) {
            $('.emojionearea .emojionearea-editor').focus();
            let tag = "\nhttps://landing-pages.cc/" + this.dataset.name;
            // Insere a tag na posição do cursorAcrecenta no final
            pasteHtmlAtCaret(tag);
            $('#modal-message-linkpicker').addClass('hide');
            event.preventDefault();
        });

    });
}