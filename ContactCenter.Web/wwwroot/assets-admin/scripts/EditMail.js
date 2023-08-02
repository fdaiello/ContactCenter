/* 
 * Scripts do Editor de Email
 * 
 */
var messageId;
var templateId;
var message;
var projectId = 15648;
var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};

/*
 * Ao carregar o documento, 
 */
$(document).ready(function () {

    // Bind SAVE button event
    $('#edit-mail-header').on('click', '#btnSave', function () {
        SaveMessage();
    });

    // Bind CLOSE button event
    $('#edit-mail-header').on('click', '#btnClose', function () {
        window.location.href = "/admin/MessagesView";
    });

    // Get messageId from URL
    messageId = getUrlParameter("messageId");

    // Get templateId from URL
    templateId = getUrlParameter("templateId");

    if (!templateId && !messageId) {
        alert("URL inválida!");
        window.location.href = "/admin/MessagesView";

    }
    else {
        // Initialize Editor
        unlayer.init({
            id: 'editor',
            projectId: projectId,
            locale: 'pt-BR',
            displayMode: 'email',
            appearance: {
                theme: 'light',
                panels: {
                    tools: {
                        dock: 'left'
                    }
                }
            }
        })

        if (messageId) {
            // Load message
            LoadMessage(messageId);
        }
        if (templateId) {
            // Load template
            LoadTemplate(templateId);
        }

    }
});

/*
 * Save Message
 */
function SaveMessage() {

    unlayer.exportHtml(function (data) {

        message.content = JSON.stringify(data.design);
        message.html = data.html;

        // Se tem Id de mensagem - estamos editando uma mensagem existente
        if (messageId) {
            // PUT message to the API
            $.ajax({
                url: '/api/Messages/' + messageId,
                contentType: 'application/json',
                headers: APIHeader,
                data: JSON.stringify(message),
                method: 'PUT',
                success: function (data) {
                    alert("Mensagem salva!");
                },
                error: function (xhr, textStatus, errorThrown) {
                    alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
                }
            })
        }
        // Criando nova mensagem a partir de template
        else {
            // Zera o Id - tinha o Id do template
            message.id = 0;
            // POST new message to the API
            $.ajax({
                url: '/api/Messages',
                contentType: 'application/json',
                headers: APIHeader,
                data: JSON.stringify(message),
                method: 'POST',
                success: function (data) {
                    alert("Nova mensagem criada!");
                    messageId = data.id;
                    message.Id = data.id;
                },
                error: function (xhr, textStatus, errorThrown) {
                    alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
                }
            })

        } 

    });

}

/*
 * Load Message
 */
function LoadMessage(messageId) {

    // Get data from API
    $.ajax({
        url: '/api/Messages/' + messageId,
        headers: APIHeader,
        type: 'GET',
        success: function (data) {
            message = data;
            if (data.content) {
                unlayer.loadDesign(JSON.parse(data.content));
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })

}

/*
 * Load Template
 */
function LoadTemplate(templateId) {

    // Get data from API
    $.ajax({
        url: '/api/Messages/Templates/' + templateId,
        headers: APIHeader,
        type: 'GET',
        success: function (data) {
            if (data.content) {
                unlayer.loadDesign(JSON.parse(data.content));
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })

}

/*
 * Busca parametros da URL
 */
//Função que busca o valor de váriaveis de url (get)
function getUrlParameter(sParam) {
    var sPageURL = window.location.search.substring(1),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;
    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');
        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);
        }
    }
};
