// Globais
var currentSelectedId = "";                 // Id do agente ou Contato selecionado no LeftSideBar
var currentSelectedType = 0;                // 0 = selected is agent; 1 = selected is Contact
var msgHistoryFirstTime;                    // menor ChattingLog.time que foi exibido na janela; nulo se nao estiver carregada - necessario pra fazer paginacao pra traz no tempo
var msgHistoryLastTime;                     // maior ChattingLog.time que foi exibido na janela; nulo se nao estiver carregada - necessario pra atualizar novas mensagens que chegam
var msgHistoryPage = 1;                     // contador quando ha paginacao pra traz no tempo buscando ChattingLogs mais antigos
var msgHistoryLock = 0;                     // trava pra nao atualizar o MsgHistory pela funcaoo que atualiza o SideBar peridicamente, quando estiver atualizando pelo clique no sidebar
var audio_status = 0;                       //record start/pause flag
var audio = document.querySelector('audio');//audio object
var audio_tracks = null;
var audio_recorder = null;
var audio_context = null;
var audio_stream = null;
var currentContact = null;                  // Store current contact data
var currentCard = null;                     // Store current card data when click card on right side bar
var file_data = '';                         //Atributo usado para a mensagem enviado na função sendMessageAction
var APIHeader = { Authorization: 'Bearer ' + localStorage.getItem('token') };
var is_searching = false;
var agentList;                              // lista dos atendentes - será usada como memória temporária - unicamente para preencher right side bar details quando selecionar atendente
var lastActivity;                           // LastActivity do contato mais recente
var firstActivity;                          // LastActivity do contato mais antigo
var searchName;
var lastAgentActivity;                      // LastActivity do atendente com conversa mais recente

/* Get API Token, and save it to local storage*/
$.ajax({
    url: '/api/Token',
    type: 'GET',
    success: function (res) {
        localStorage.setItem('token', res.token);
        APIHeader = { Authorization: 'Bearer ' + localStorage.getItem('token') };
    },
    error: function (xhr, textStatus, errorThrown) {
        alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
    }
})

/* Ao carregar o documento
 */
window.onload = function () {

    // Checks for search param within URL - used when coming from CRM - chat link
    searchName = getUrlParameter('search');
    if (searchName) {
        var currentSelectedId = getUrlParameter('contactId').toString();
        currentSelectedType = 1;
        $('#sidebar_search_input').val(searchName);
        $('.x svg').css('display', 'inline');
        is_searching = true;
        getSingleContact(currentSelectedId);
        // Show search bar
        $("#vertical-nav-menu-search").removeClass("hide");
        // Hide contact list
        $("#vertical-nav-menu").addClass("hide");
    }

    $('.app-footer__inner').addClass('actived-theme');
    if ($("#isadmin").val() == '0') $('#edit_role').prop('disabled', true);
    $(".app-footer").css('display', 'none');

    // Carrega o SideBar
    $(".validate-label").css('display', 'none');

    // Configuracoes que dependem da resolucaoo
    resizeAction();

    // Configuracoes gerais para o Agente
    $(".app-container").addClass("fixed-footer");

    // Get current logged in user Avatar
    if ($("#current_avatar").val() == "NULL") {
        $("#current_avatar_img").html(getIconByName($("#current_username").val(), ''));
        if ($("#edit_avatarPreview").attr("src") == "NULL") {
            $("#edit_avatarPreview").attr("src", "/assets-chatroom/images/avatars/default.png");
        }
    }

    // Show left side bar
    getSideBar();

    // Add Emoji Button to message text area
    $("#send-message-obj").emojioneArea({
        pickerPosition: "top",
        shortcuts: false,
        autocomplete: false,
        events: {
            keypress: function (editor, event) {
                if ($(editor).html() == '') return;
                if (event.keyCode == 13 && !event.shiftKey) {
                    $('#send-message-obj').val($(editor).html());
                    sendMessageAction();
                    event.preventDefault();
                }
            },
            paste: function (event) { handlePaste(event) },
        }
    });

    /*
     * audio record code
     * https://blog.addpipe.com/using-recorder-js-to-capture-wav-audio-in-your-html5-web-site/
     */
    window.URL = window.URL || window.webkitURL;
    /** 
     * Detecte the correct AudioContext for the browser 
     * */
    window.AudioContext = window.AudioContext || window.webkitAudioContext;
    navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia || navigator.msGetUserMedia;
}

let onAudioFail = function (e) {
    alert('Error ' + e);
};
let onAudioSuccess = function (s) {
    audio_stream = s;
    audio_tracks = s.getTracks();
    audio_context = new AudioContext();
    let mediaStreamSource = audio_context.createMediaStreamSource(s);
    audio_recorder = new Recorder(mediaStreamSource);
    audio_recorder.record();
}

function AudioRecordStart(audio_status) {
    if (audio_status === 1) {
        if (navigator.getUserMedia) {
            //Desabilita textarea enquanto o áudio está sendo gravado
            $(".message-text-area").attr("disabled", true);

            //retira o icone de microfone
            $(".start-recording").css("display", "none");

            //insere os botões de opções e o contador do áudio
            $(".recording-audio").css("display", "flex");
            navigator.getUserMedia({ audio: true, video: false }, onAudioSuccess, (e) => alert('Error: ' + e));
        }
    } else if (audio_status === 0) {
        audio_recorder.stop();
        audio_stream.getAudioTracks()[0].stop();
        audio_tracks.forEach(track => track.stop());
        audio_recorder.exportWAV(sendRecordedAudioAction);
        audio_recorder.clear();
    } else {
        audio_recorder.stop();
        audio_stream.getAudioTracks()[0].stop();
        audio_tracks.forEach(track => track.stop());
        audio_recorder.clear();
    }
}

//Evento de start audio
$(".start-recording").on("click", function () { AudioRecordStart(1) });

//Evento de envio do audio
$("#send-audio").on("click", function () {
    AudioRecordStart(0);
    audio_recorder.clear();
    audio_stream.getAudioTracks()[0].stop();
    $(".start-recording").css("display", "block");
    $(".recording-audio").css("display", "none");
    $(".message-text-area").attr("disabled", false);
});

//Evento de cancelamento do audio
$("#cancel-audio").on("click", function () {
    AudioRecordStart();
    $(".start-recording").css("display", "block");
    $(".recording-audio").css("display", "none");
    $(".message-text-area").attr("disabled", false);
});

/*
 * Ajustes que dependem da resolucao
 */
function resizeAction() {
    if ($(window).width() < 1250) { $(".app-container").addClass("closed-sidebar-mobile closed-sidebar") }
    else { $(".app-container").removeClass("closed-sidebar-mobile closed-sidebar") }
    $(".mobile-toggle-nav").toggleClass("is-active");
    $(".app-container").toggleClass("sidebar-mobile-open");

    if ($(".app-container").prop('class').indexOf('sidebar-mobile-open') > -1) {
        $(".sidebar-mobile-open").removeClass("closed-sidebar-mobile closed-sidebar");
    }
}

function readURL(input, f) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) { $('#' + f).attr('src', e.target.result) }
        reader.readAsDataURL(input.files[0]);
    }
}
$("#edit_avatar").change(function () { readURL(this, "edit_avatarPreview") });
$("#edit-avatar-change-btn").click(function () {
    $("#edit_avatar").trigger('click');
    return false;
});
$("#add_avatar").change(function () {
    readURL(this, "add_avatarPreview");
});
$("#add-avatar-change-btn").click(function () {
    $("#add_avatar").trigger('click');
    return false;
});

function attachedFileClickAction() { $('#attached_file').trigger('click') }

//elemento DOM capturado para a execução do evento de paste
$(".pastediv").on('paste', handlePaste);

//Função que manipula o evento de clipboardData e exibe na pré visualização
function handlePaste(event) {
    var items = (event.clipboardData || event.originalEvent.clipboardData).items;
    for (index in items) {
        var item = items[index];
        if (item.kind === 'file') {
            var blob = item.getAsFile();
            var reader = new File([blob], "Captura.png", { type: "image/png" });
            preview(reader);
            file_data = reader; //Armazena o objeto File da imagem colada no atributo file_data
        }
    }
}


//Função de preview de arquivos no chat
function preview(files) {
    $(".container-pre-visualizacao").css("display", "block");

    //Previne possíveis conflitos com a função createObjectURL
    var getBlobUrl = (window.URL && URL.createObjectURL.bind(URL)) ||
        (window.webkitURL && webkitURL.createObjectURL.bind(webkitURL)) ||
        window.createObjectURL;

    var out = $(".visualizacao");
    var img = document.createElement("img");
    img.src = getBlobUrl(files); //getBlobUrl transforma o files, em formato blob, para uma url válida a ser usada no src de preview
    out.html(img);
}

$('#attached_file').on('change', function () {
    file_data = $('#attached_file').prop('files')[0]; //Configura file_data se for utilizado o botão btn-submit-file para a seleção da imagem
    preview(this.files[0]);
});

function closeAttachedFileAction() { $("#toast-container").css('display', "none") }

//clique para esconder a pré visualização da imagem
$(".close-pre-visualizacao").on("click", function () { $(".container-pre-visualizacao").css("display", "none") });

//Envio da imagem
$(".btn-send-img").on("click", function () {
    $("#toast-container .toast-message").html($(this).val());
    $(".container-pre-visualizacao").css("display", "none");
    sendMessageAction(file_data);
});

/**
 * request getting Msg History ( right chattingroom ) from server.
 * param selectedType   - 0 = selected is agent; 1 = selected is Contact
 * param selectedId     - current selected agent or contact
 * param page           - current page number
 * param firstTime      - last min Time that was already shown
 * param messageId      - used when searched message was clicked - to scroll to messageId
 */
function getMsgHistory(selectedType, selectedId, page, firstTime, messageId) {
    msgHistoryLock = 1;
    var form_data = new FormData();
    var pagesize = 50;
    if (page > 0) { pagesize = 20 }
    form_data.append('selectedId', selectedId);
    form_data.append('selectedType', selectedType);
    form_data.append('pageSize', pagesize);
    form_data.append('firstTime', firstTime);
    form_data.append('messageId', messageId);

    // first time
    if (page == 0) {
         // clear current html
        $("#msg_history").html("");
    }

    postAjax('/Chat/GetMsgHistory', APIHeader, form_data, true, false, false, false, false).then((response) => {
        if (response.count > 0) {
            var html = "";
            var chattingLogViewList = response.chattingLogViewList;
            msgHistoryFirstTime = chattingLogViewList[0].chattingLog.time; // update global history references

            // first time
            if (page == 0) {
                 // update global reference for last message
                msgHistoryLastTime = chattingLogViewList[chattingLogViewList.length - 1].chattingLog.time;
            }

            for (var i = 0; i < chattingLogViewList.length; i++) {
                // messagem from bot
                if (chattingLogViewList[i].chattingLog.source == 0) {
                    html += htmlFormatMsgHistory(chattingLogViewList[i].chattingLog.id, 0, chattingLogViewList[i].chattingLog.text, chattingLogViewList[i].quotedLog, chattingLogViewList[i].chattingLog.filename, chattingLogViewList[i].chattingLog.type, chattingLogViewList[i].chattingLog.status, getNowTime(getDateObj(chattingLogViewList[i].chattingLog.time)) + " | Bot")
                }

                // message from contact
                else if (chattingLogViewList[i].chattingLog.source == 1) {
                    html += htmlFormatMsgHistory(chattingLogViewList[i].chattingLog.id, 1, chattingLogViewList[i].chattingLog.text, chattingLogViewList[i].quotedLog, chattingLogViewList[i].chattingLog.filename, chattingLogViewList[i].chattingLog.type, chattingLogViewList[i].chattingLog.status, getNowTime(getDateObj(chattingLogViewList[i].chattingLog.time)) + " | " + chattingLogViewList[i].contactName)
                }

                // messagem from agent
                else if (chattingLogViewList[i].chattingLog.source == 2) {
                    html += htmlFormatMsgHistory(chattingLogViewList[i].chattingLog.id, 0, chattingLogViewList[i].chattingLog.text, chattingLogViewList[i].quotedLog, chattingLogViewList[i].chattingLog.filename, chattingLogViewList[i].chattingLog.type, chattingLogViewList[i].chattingLog.status, getNowTime(getDateObj(chattingLogViewList[i].chattingLog.time)) + " | " + (chattingLogViewList[i].agentName ?? ""))
                }

                // messagem from agent to agent
                else if (chattingLogViewList[i].chattingLog.source == 3) {
                    var dir = chattingLogViewList[i].chattingLog.applicationUserId == selectedId ? 0 : 1;
                    html += htmlFormatMsgHistory(chattingLogViewList[i].chattingLog.id, 1 - dir, chattingLogViewList[i].chattingLog.text, chattingLogViewList[i].quotedLog, chattingLogViewList[i].chattingLog.filename, chattingLogViewList[i].chattingLog.type, chattingLogViewList[i].chattingLog.status, getNowTime(getDateObj(chattingLogViewList[i].chattingLog.time)) + " | " + chattingLogViewList[i].agentName);
                }
            }

            // save current history size
            var lastScrollHeight = $("#msg_history")[0].scrollHeight;

            // update msg_history html
            html = html + $("#msg_history").html();
            $("#msg_history").html(html);

            // on the first page, scroll bottom, but when going back in history, we need to calculate
            if (page == 0) {
                $("#msg_history").scrollTop($("#msg_history")[0].scrollHeight);

                // When searching, don't scroll botton, as search will point to the searched message
                if (!is_searching) {
                    // we need to give some time so the browser can render new HTML, and then we may check its height
                    setTimeout(() => { // After some seconds checks scrollHeight again and adjust scrool position
                        $("#msg_history").scrollTop($("#msg_history")[0].scrollHeight);
                    }, 1000);
                }
            }
            else {
                $("#msg_history").scrollTop($("#msg_history")[0].scrollHeight - lastScrollHeight);

                // we need to give some time so the browser can render new HTML, and then we may check its height
                setTimeout(() => { // After 1 second checks scrollHeight again and adjust scrool position
                    $("#msg_history").scrollTop($("#msg_history")[0].scrollHeight - lastScrollHeight);
                }, 3000);
            }

            msgHistoryPage++; // increase page size - so we can know when is the first load, and when we are retreaving older data

            // Se recebeu messageId - searched message
            if (messageId) {
                // Scroll to messageId
                var elmnt = document.getElementById(messageId);
                if (elmnt != null) {
                    elmnt.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'start' })
                }
                $("#" + messageId + " div p").css('background-color', '#794c8a'); // Change background
            }
        }
        else {
            if (firstTime == 0) $("#msg_history").html(""); // Clean Msg History
        }

        msgHistoryLock = 0 // unlock chatroom
        //update globals
        currentSelectedType = selectedType;
        currentSelectedId = selectedId;
    });

}

/*
 * Ao selecionar a div com os dados do agente logado
 */
function currentAgentSelectAction() {
    // preenche AgentView com os dados do Agent selecionado
    $('#edit_avatar').prop('src', $("#current_avatar").val());
    $("#edit_avatarPreview").prop('src', $("#current_avatar").val() == "NULL" ? "assets-chatroom/images/avatars/default.png" : $("#current_avatar").val());
    $("#edit_fullname").val($("#current_fullname").val());
    $("#edit_nickname").val($("#current_nickname").val());
    $("#edit_username").val($("#current_username").val());
    $("#edit_role").val($("#current_role").val());
    $("#edit_userid").val($("#current_userid").val());
}

/*
 * Atualiza os status das mensagens
 */
function updateMsgStatus(statusList) {
    for (var i = 0; i < statusList.length; i++) {
        var spanid = "#chk" + statusList[i].id;
        var html = getMessageCheckMark(statusList[i].status);
        $(spanid).html(html);
    }
}

/*
 *  Devolve um bloco de html contendo a mensagem que deve ser adicionada ao MsgHistory
 */
function htmlFormatMsgHistory(id, bool_receaved, message, quotedmsg, attach, type, status, nowdate) {
    /*
     * quotedmsg is chattinglog model object
     * Text
     * Filename
     * Time
     * Type
    */
    var h = "";

    // Mensagem recebida do cliente
    if (bool_receaved) {
        h += "<div id=\"" + id + "\" class=\"incoming_msg\">" +
            "<div class=\"received_msg\">" +
            "<div class=\"inner_wrapper\">";
        h += "<div class=\"inner\">";


        // If there is an attachment
        if (!(attach == "" || attach == null || attach == "undefined" || attach == "null")) {
            // Image
            if (type == "2")
                h += "<img src=\"" + attach + "\" onclick=\"ImageZoom(this)\" alt=\"" + message + "\" /><br>";
            // Audio
            else if (type == "1")
                if (attach.endsWith("mp3"))
                    h += "<audio controls><source src = '" + attach + "' type = \"audio/mpeg\" >Your browser does not support the audio element.</audio>";
                else
                    h += "<audio controls><source src = '" + attach + "' type = \"audio/ogg\" >Your browser does not support the audio element.</audio>";
            // Files
            else if (type == "3" || type == "4" || type == "5")
                h += "<a href=\"#\" onclick=\"window.open('" + attach + "')\"><img src=\"/images/" + type + ".png\" title=\"clique para fazer download do File\"></a><br>";
            // Video
            else if (type == "11") {
                h += '<video width="320" height="240" controls>';
                h += '<source src="' + attach + '" type="video/mp4">';
                h += 'Your browser does not support the video tag.';
                h += '</video>';
            }
            // Other
            else
                h += "<a href=\"#\" onclick=\"window.open('" + attach + "')\"><img src=\"/images/6.png\" title=\"clique para fazer download do arquivo\"></a><br>";
        }

        // Quoted Msg
        if (quotedmsg != null && quotedmsg.text != null ) {
            h += "<div class=\"quoted_msg\">";
            if (quotedmsg.Type == "2")
                h += "<img src=\"" + quotedmsg.filename + "\" />";
            else
                h += quotedmsg.text.replace(/\n/g, "<br>");
            h += "</div>";
        }

        // Map
        if (type == "7") {
            var location = JSON.parse(message);
            h += "<iframe style=\"border: 0; text-align: center; margin: auto;\" src=\"https://www.google.com/maps/embed/v1/place?key=AIzaSyD1sWrVxwcK9ZuVdFV0x0VMgfDwU37zh8A&q=" + location.latitude + "," + location.longitude + "\" width=\"100%\" height=\"300\" frameborder=\"0\" allowfullscreen=\"allowfullscreen\"></iframe>";
        }
        // Contacts
        else if (type == "8") {
            var contacts = JSON.parse(message);
            h += "<div class=\"contacts\">";
            var index;
            for (index = 0; index < contacts.length; ++index) {
                h += "<div class=\"contact\">";
                h += "<div class=\"name\">";
                var name = contacts[index].name;
                h += name.formatted_name;
                h += "</div>";
                var emails = contacts[index].emails;
                for (eindex = 0; eindex < emails.length; ++eindex) {
                    h += "<div class=\"email\">";
                    var email = emails[eindex];
                    h += email.email;
                    h += "</div>"
                }
                var phones = contacts[index].phones;
                for (pindex = 0; pindex < phones.length; ++pindex) {
                    h += "<div class=\"phone\">";
                    var phone = phones[pindex];
                    h += phone.phone;
                    h += "</div>"
                }
                h += "</div>"
            }
            h += "</div>"
        }
        // VCards
        else if (type == "9" && message) {
            if (message.includes("<br>"))
                message = message.split("<br>").slice(-1)[0]

            var vcardList = JSON.parse(message);
            h += "<div class=\"contacts\">";
            var index;
            for (index = 0; index < vcardList.length; ++index) {
                var vcard = vcardList[index].vcard;
                var card = vcardParse(vcard);
                h += "<div class=\"contact\">";
                for (index2 = 0; index2 < card.fn.length; ++index2) {
                    h += "  <div class=\"name\">";
                    h += card.fn[index2].value;
                    h += "  </div>";
                }
                for (index3 = 0; index3 < card.tel.length; ++index3) {
                    h += "  <div class=\"phone\">";
                    h += card.tel[index3].value;
                    h += "</div>"
                }

                h += "</div>"
            }
            h += "</div>"
        }
        else
            h += message.replace(/\n/g, "<br>")

        if (type == "10") {
            h += "<img src='/assets-chatroom/images/channels/sms.png' title='mensagem de SMS' class='msgIconType'>";
        }
        else if (type == "12") {
            h += "<img src='/assets-chatroom/images/channels/email.png' title='mensagem de Email' class='msgIconType'>";
        }

        h += "</div>";

        h += "<span class=\"time_date\">" + nowdate + "</span></div>" +
             "</div>" +
             "</div>";
    }
    // Mensagem enviada
    else {
        h += "<div id=\"" + id + "\" class=\"outgoing_msg\">" +
            "<div class=\"sent_msg\">";

        if (quotedmsg != null) {
            h += "<div>";
            if (quotedmsg.Type == "2")
                h += "<img src=\"" + quotedmsg.filename + "\" />";
            else
                h += quotedmsg.text.replace(/\n/g, "<br>");
            h += "</div>";
        }

        h += "<p>";

        // Se tem arquivo
        if (!(attach == "" || attach == null || attach == "undefined" || attach == "null")) {
            if (type == "2")
                h += "<img src=\"" + attach + "\" onclick=\"ImageZoom(this)\" alt=\"" + message + "\" />";
            else if (type == "1")
                if (attach.endsWith("mp3"))
                    h += "<audio controls><source src = '" + attach + "' type = \"audio/mpeg\" >Your browser does not support the audio element.</audio>";
                else
                    h += "<audio controls><source src = '" + attach + "' type = \"audio/ogg\" >Your browser does not support the audio element.</audio>";
            else if (type == "3" || type == "4" || type == "5")
                h += "<a href=\"#\" onclick=\"window.open('" + attach + "')\"><img src=\"/images/" + type + ".png\" title=\"clique para fazer download do File\"></a>";
            else
                h += "<a href=\"#\" onclick=\"window.open('" + attach + "')\"><img src=\"/images/6.png\" title=\"clique para fazer download do File\"></a>";
        }

        if (message != null)
            h += message.replace(/\n/g, "<br>");

        if (type == "10") {
            h += "<img src='/assets-chatroom/images/channels/sms.png' title='mensagem de SMS' class='msgIconType'>";
        }
        else if (type == "12") {
            h += "<img src='/assets-chatroom/images/channels/email.png' title='mensagem de Email' class='msgIconType'>";
        }

        h += "<span id=\"chk" + id + "\" >" + getMessageCheckMark(status) + "</span>"
        h += "</p>";
        h += "<span class=\"time_date\">" + nowdate + "</span>";
        h += "</div></div>";
    }
    return h;
}

/*
 * Return html to produce checkmark or symbol acording to message status
 * 2=failed
 * 3=sent
 * 4=delivered
 * 5=read
 */
function getMessageCheckMark(status) {
    var html = "";
    if (status == "2")
        html = "<span data-icon=\"recalled\" class=\"msgStatusCheck\"><svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 17 19\" width=\"17\" height=\"19\"><path fill=\"currentColor\" d=\"M12.629 12.463a5.17 5.17 0 0 0-7.208-7.209l7.208 7.209zm-1.23 1.229L4.191 6.484a5.17 5.17 0 0 0 7.208 7.208zM8.41 2.564a6.91 6.91 0 1 1 0 13.82 6.91 6.91 0 0 1 0-13.82z\"></path></svg></span>";
    else if (status == "3")
        html = "<span aria-label=\"Sent.\" data-icon=\"msg-check\" class=\"msgStatusCheck\"><svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 16 15\" width=\"16\" height=\"15\"><path fill=\"currentColor\" d=\"M10.91 3.316l-.478-.372a.365.365 0 0 0-.51.063L4.566 9.879a.32.32 0 0 1-.484.033L1.891 7.769a.366.366 0 0 0-.515.006l-.423.433a.364.364 0 0 0 .006.514l3.258 3.185c.143.14.361.125.484-.033l6.272-8.048a.365.365 0 0 0-.063-.51z\"></path></svg></span>";
    else if (status == "4")
        html = "<span aria-label=\"Delivered.\" data-icon=\"msg-dblcheck\" class=\"msgStatusCheck\"><svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 16 15\" width=\"16\" height=\"15\"><path fill=\"currentColor\" d=\"M15.01 3.316l-.478-.372a.365.365 0 0 0-.51.063L8.666 9.879a.32.32 0 0 1-.484.033l-.358-.325a.319.319 0 0 0-.484.032l-.378.483a.418.418 0 0 0 .036.541l1.32 1.266c.143.14.361.125.484-.033l6.272-8.048a.366.366 0 0 0-.064-.512zm-4.1 0l-.478-.372a.365.365 0 0 0-.51.063L4.566 9.879a.32.32 0 0 1-.484.033L1.891 7.769a.366.366 0 0 0-.515.006l-.423.433a.364.364 0 0 0 .006.514l3.258 3.185c.143.14.361.125.484-.033l6.272-8.048a.365.365 0 0 0-.063-.51z\"></path></svg></span>";
    else if (status == "5")
        html = "<span aria-label=\"Read.\" data-icon=\"msg-dblcheck\" class=\"msgStatusCheck CheckRead\"><svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 16 15\" width=\"16\" height=\"15\"><path fill=\"currentColor\" d=\"M15.01 3.316l-.478-.372a.365.365 0 0 0-.51.063L8.666 9.879a.32.32 0 0 1-.484.033l-.358-.325a.319.319 0 0 0-.484.032l-.378.483a.418.418 0 0 0 .036.541l1.32 1.266c.143.14.361.125.484-.033l6.272-8.048a.366.366 0 0 0-.064-.512zm-4.1 0l-.478-.372a.365.365 0 0 0-.51.063L4.566 9.879a.32.32 0 0 1-.484.033L1.891 7.769a.366.366 0 0 0-.515.006l-.423.433a.364.364 0 0 0 .006.514l3.258 3.185c.143.14.361.125.484-.033l6.272-8.048a.365.365 0 0 0-.063-.51z\"></path></svg></span>";

    return html;
}

/**
 * generate icon bt user name
 * */
function getIconByName(name, status, unAnsweredCount) {

    if (name == "" | name == "?" | name == null) {
        icon_name = "?";
        icon_color = "dark";
    }
    else {
        if (name == null)
            name = "";
        else
            name = name.trim();

        var arr = name.split(" ");
        var color_arr = (new String("PRIMARY SECONDARY SUCCESS INFO WARNING DANGER FOCUS DARK")).toLowerCase().split(" ");
        var first_name = arr[0];
        var firstchars = [...first_name];
        if (firstchars.length > 0) {
            var first_letter = firstchars[0];
            var icon_name = first_letter
            if (!icon_name.match(/[a-z]/i)) {
                icon_name = `<i class="fas fa-user"></i>`;
                icon_color = "focus";

            }
            else {
                if (arr.length > 1) {
                    var second_name = arr[1];
                    var nexchars = [...second_name];
                    icon_name += nexchars[0];
                }
                icon_name = icon_name.toUpperCase();
                var icon_color = color_arr[(first_letter.charCodeAt(0) - 65) % color_arr.length];
            }
        }
        else {
            icon_name = "?";
            icon_color = "dark";
        }
    }

    var html = "<div class=\"badge badge-pill badge-" + icon_color + " left-panel-generated-icon\">" + icon_name;

    // Se o status == 2 ( Aguardando ) insere div com css para circulo piscante
    if (status == "2")
        html += "<div role=\"presentation\" tabindex=\"-1\" title=\"" + name + "\" aria-hidden=\"true\" class=\"waiting-flashing-icon\"></div>";
    // Se tem mensagens nao respondidas, insere div com numero
    else if (unAnsweredCount > 0)
        html += "<div role=\"presentation\" tabindex=\"-1\" title=\"" + name + "\" aria-hidden=\"true\" class=\"unAnsweredCount-icon\">" + unAnsweredCount + "</div>";

    html += "</div>";
    return html;
}

/*
 * Chama o Controller para enviar uma mensagem para um contato
 * Adiciona a mensagem enviada ao ChattingLog na tela
 */
function sendMessageAction(file_data) {

    var form_data = new FormData();
    form_data.append('contactId', currentSelectedId);					    	//current selected contact
    var msgtext = $("#send-message-obj").val();
    msgtext = msgtext.replaceAll("<div></div>", "\n").replaceAll("<div><br></div>", "\n").replaceAll("<div>", "").replaceAll("</div>", "");

    if (msgtext.replace("\n", "").trim() != "" | file_data != "") {
        form_data.append('msgText', msgtext);				                    //message text
        form_data.append('attachedFile', file_data);	    				    //attached file object
        form_data.append('isToAgent', 1 - currentSelectedType);

        $('#attached_file').val('');
        closeAttachedFileAction();
        postAjax('/SendMessage', APIHeader, form_data, true, false, false, false, false).then((response) => {
            if (response.chattingLogId == "0")
                alert("Falha ao enviar a mensagem");
            else if (response.chattingLogId.startsWith("Erro"))
                alert(response.chattingLogId);
            else
                addToMsgHistory(msgtext, response.chattingLogId, response.filename, response.chatMsgType, Date.now());

            // Desativa modo de serch - para voltar a atualizar left side bar, caso tivesse procurando
            if (is_searching) {
                $('.app-sidebar__inner .sidebar-search .x svg').css('display', 'none');
                $('.app-sidebar__inner .sidebar-search .lupa svg').css('display', 'inline');
                $('.app-sidebar__inner .sidebar-search input').val('');

                // Hides search bar
                $("#vertical-nav-menu-search").addClass("hide");
                // Show contact list
                $("#vertical-nav-menu").removeClass("hide");

                is_searching = false;
                searchName = "";
                getSideBar();
            }

        });
    }

    // Reset textarea
    $("#send-message-obj").val("");

    // Reset Emojionearea
    $("#send-message-obj").data("emojioneArea").hidePicker();
    $('.app-footer .emojionearea-editor').html('');

    // Update sent message at left side bar
    if (msgtext) {
        let selector = "#contact" + currentSelectedId + " > a > div > div > div.widget-content-left.col-desc > div.widget-subheading.lastest_message";
        $(selector).html(msgtext);
    }
}
/*
 * 
 */
function sendRecordedAudioAction(blob) {
    var filename = getFormattedTime() + '.wav';
    var url = window.URL.createObjectURL(blob);
    audio.controls = true;
    audio.src = url;
    var hf = document.createElement('a');
    hf.href = url;
    hf.download = filename;
    hf.innerHTML = hf.download;

    var form_data = new FormData();
    form_data.append('contactId', currentSelectedId);							//current selected contact
    form_data.append('msgText', $("#send-message-obj").val());				    //message text
    form_data.append('attachedFile', blob, filename);						    //attached file object
    form_data.append('isToAgent', 1 - currentSelectedType);

    $("#send-message-obj").val("");
    $("#send-message-obj").focus();

    postAjax('/SendMessage', APIHeader, form_data, true, false, false, false, false).then((response) => {
        if (response.chattingLogId == "0")
            alert("Falha ao enviar a mensagem");
        else if (response.chattingLogId.startsWith("Erro"))
            alert(response.chattingLogId);
        else
            addToMsgHistory(response.msgTextRecognized, response.chattingLogId, response.filename, response.chatMsgType, response.time);

        // Desativa modo de serch - para voltar a atualizar left side bar, caso tivesse procurando
        if (is_searching) {
            $('.app-sidebar__inner .sidebar-search .lupa x').css('display', 'none');
            $('.app-sidebar__inner .sidebar-search .lupa svg').css('display', 'inline');
            $('.app-sidebar__inner .sidebar-search input').val('');
            is_searching = false;
            getSideBar();
        }

    });
}

/* 
 * Adiciona uma mensagem ao historico do chat
 */
function addToMsgHistory(messagetext, chattingLogId, filename, type, time) {
    var status = "0";

    var dateObj = new Date(time)
    var month = dateObj.getUTCMonth() + 1; //months from 1-12
    var day = dateObj.getUTCDate();
    var year = dateObj.getUTCFullYear();

    var nowdate = dateObj.getHours() + ":" + dateObj.getMinutes() + " " + getAmOrPm(dateObj.getHours()) + " | " + day + "/" + month + "/" + year;
    var lastdate = dateObj.getHours() + ":" + dateObj.getMinutes() + " " + getAmOrPm(dateObj.getHours());

    var h = $("#msg_history").html();

    $("#msg_history").html(h + htmlFormatMsgHistory(chattingLogId, 0, messagetext, null, filename, type, status, nowdate + " | " + $('#current_fullname').val()));

    $("#msg_history").animate({ scrollTop: $("#msg_history").prop("scrollHeight") }, "slow");
    var len = 35;
    if (messagetext.length > len) messagetext = messagetext.substring(0, len - 3) + "...";
    $("#agent" + currentSelectedId + " a div div div .lastest_message").html(messagetext);
    $("#agent" + currentSelectedId + " a .lastest_datetime").html(lastdate);
}

/*
 * Ao fazer Scroll em MsgHistory
 */
function msgHistoryScroll() {
    var s = $("#msg_history")[0].scrollTop;
    if (s == 0 && msgHistoryPage > 0) {
        getMsgHistory(currentSelectedType, currentSelectedId, msgHistoryPage, msgHistoryFirstTime);
    }
}

$(document).on("click", ".navbar-toggler-icon", function () {
    $(".app-container").addClass("sidebar-mobile-open");
});


/*
 * Impede colar HTML na mensagem, traz somente texto sem formatação ao cloar
 * @author: Felipe Daiello
 * @date: 2021-02-06
 */
$("#send-msg-div").on('paste', function (e) {
    e.preventDefault();
});
