/**
 * Get Left Side Bar - contacts and agent list
 *     also gets new messages, and changed status
 * */
function getSideBar() {

    // Previne 2 chamadas quando está em busca - se estiver em busca, somente roda a chamada da busca, e não a do timeout
    if (is_searching)
        return;

    if (msgHistoryLock) {
        setTimeout(getSideBar, 3000);
        return;
    }
    
    var form_data = new FormData();
    form_data.append('SelectedId', currentSelectedId);
    form_data.append('lastTime', msgHistoryLastTime);
    form_data.append('SelectedType', currentSelectedType);
    form_data.append('lastActivity', lastActivity);

    $.ajax({
        url: '/Chat/GetSideBar',
        data: form_data,
        cache: false,
        contentType: false,
        processData: false,
        type: 'POST',
        dataType: "json",
        success: function (response) {

            // Update global lastActivity
            if (response.contactList.length > 0) {
                lastActivity = response.contactList[0].lastActivity;
                // First time update first activity ( oldest )
                if (firstActivity == null)
                    firstActivity = response.contactList[response.contactList.length - 1].lastActivity;
            }
            if (response.agentList.length > 0) {
                lastAgentActivity = response.agentList[0].lastActivity;
            }

            // Save AgentList to global
            agentList = response.agentList;

            // Update Agents list at left side bar
            renderAgents(response.agentList);

            // Update Contacts List at left side bar
            renderContacts(response.contactList);

            // Update MsgHistory
            updateMsgHistory(response.chattinglogViewList);

            // confere se esta selecionado um contato ou um atendente
            if (currentSelectedType == 1)
                // marca o contato selecionado
                $("#contact" + currentSelectedId + " a").addClass("mm-active");
            else if (currentSelectedType == 0)
                // marca o agente selecionado
                $("#agent" + currentSelectedId + " a").addClass("mm-active");

            // update any message Status recently updated
            updateMsgStatus(response.changedStatusList);

            // if its not within a search
            if (!is_searching)
                // Loop this function on the next 5 seconds
                setTimeout(getSideBar, 5000);

        },
        error: (xhr, textStatus, errorThrown) => {
            console.log(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
            // Loop this function on the next 10 seconds
            setTimeout(getSideBar, 10000);
        }

    });
}

/*
 * Render Contact List at the the left side bar
 *     - build <li> element for each contact in list
 *     - check if <li> element exists a list ( by Id ) and delete old version
 *     - insert each element at the beggining of list ( newer contacts)
 */

function renderContacts(contactList) {

    if (contactList.length > 0) {

        // gets parent
        let ul = document.getElementById("vertical-nav-menu");

        // Get the header
        let header = document.getElementById("left-contacts-header");

        // Loop contact list from end to top, older to newer
        for (var i = contactList.length - 1; i >= 0; i--) {

            // builds <li> element for contact
            var newLi = document.createElement("li");
            newLi.id = "contact" + contactList[i].id;
            newLi.innerHTML = renderLiInnerHtml(contactList[i], '1');

            // lets search if is there any <li> with current id
            var oldLi = document.getElementById(newLi.id);

            //If it isn't "undefined" and it isn't "null", then it exists.
            if (typeof (oldLi) != 'undefined' && oldLi != null) {
                // element exist, lets delete it
                ul.removeChild(oldLi);
            }

            // Insert the new element after the header
            ul.insertBefore(newLi, header.nextSibling);
        }
    }
}

/*
 *  Render Agents at left side bar
 */
function renderAgents(agentList) {

    if (agentList.length > 0) {
        // gets parent
        let ul = document.getElementById("vertical-nav-menu");

        // Get the header
        let header = document.getElementById("left-agents-header");
        header.classList.remove("hide")

        // Loop contact list from end to top, older to newer
        for (var i = agentList.length - 1; i >= 0; i--) {

            // builds <li> element for contact
            var newLi = document.createElement("li");
            newLi.id = "agent" + agentList[i].id;
            newLi.innerHTML = renderLiInnerHtml(agentList[i], '0');

            // lets search if is there any <li> with current id
            var oldLi = document.getElementById(newLi.id);

            //If it isn't "undefined" and it isn't "null", then it exists.
            if (typeof (oldLi) != 'undefined' && oldLi != null) {
                // element exist, lets delete it
                ul.removeChild(oldLi);
            }

            // Insert the new element after the header
            ul.insertBefore(newLi, header.nextSibling);
        }
    }
}

/*
 * Render Searched Contacts at search list
 */
function renderSearchedContacts(contactList) {

    if (contactList.length > 0) {
        // builds <li> element for header
        let newLi = document.createElement("li");
        newLi.className = "app-sidebar__heading";
        newLi.innerHTML = "Contatos - pesquisa";

        // gets parent
        let ul = document.getElementById("vertical-nav-menu-search")

        // append header
        ul.appendChild(newLi)

        // Loop contactList from end to top, older to newer
        for (var i = contactList.length - 1; i >= 0; i--) {

            // builds <li> element for contact
            let newLi = document.createElement("li");
            newLi.id = "contact-search-" + contactList[i].id;
            newLi.innerHTML = renderLiInnerHtml(contactList[i], '1');

            // Insert the element at the end of the list
            ul.appendChild(newLi)
        }
    }
}
/*
 * Render Searched Agents at search list
 */
function renderSearchedAgents(agentList) {

    if (agentList.length > 0) {
        // builds <li> element for header
        let newLi = document.createElement("li");
        newLi.className = "app-sidebar__heading";
        newLi.innerHTML = "Atendentes - pesquisa";

        // gets parent
        let ul = document.getElementById("vertical-nav-menu-search")

        // append header
        ul.appendChild(newLi)

        // Loop contactList from end to top, older to newer
        for (var i = agentList.length - 1; i >= 0; i--) {

            // builds <li> element for Agent
            let newLi = document.createElement("li");
            newLi.id = "agent-search-" + agentList[i].id;
            newLi.innerHTML = renderLiInnerHtml(agentList[i], '0');

            // Insert the element at the end of the list
            ul.appendChild(newLi)
        }
    }
}

/*
 * Returns innerHTML for a single contact or agent <li>
 */
function renderLiInnerHtml(element, selectedType) {

    var innerHtml = "<a href=\"#\" onclick=\"leftBarSelectAction('" + element.id + "'," + selectedType + ");\">" +
        "<div class=\"widget-content p-0\">" +
        "<div class=\"widget-content-wrapper\">" +
        "<div class=\"widget-content-left mr-2\">" +
        (
        !element.avatar ?
            getIconByName(element.fullName, element.status, element.unAnsweredCount)
            : "<img class=\"rounded-circle\" src=\"" + element.avatar + "\" alt=\"\">" +
            (element.unAnsweredCount > 0 ? "<div class=\"unAnsweredCount-for-avatar\">" + element.unAnsweredCount + "</div>" : "") +
            (element.unAnsweredCount == 0 && element.status == "2" ? "<div role=\"presentation\" tabindex=\"-1\" title=\"" + element.fullName + "\" aria-hidden=\"true\" class=\"waiting-flashing-icon waiting-flashing-icon2\"></div>" : "")

        ) +
        "</div>" +
        "<div class=\"widget-content-left col-desc\">" +
        "<div class=\"widget-heading\">" +
        (
        element.channelType == "0" ? ""//channel is not assigned
            : element.channelType == "1" ?//channel is whatsapp
                    "<img src=\"/assets-chatroom/images/channels/whatsapp.png\" alt=\"\">"
                : element.channelType == "2" ?//channel is webchat
                        "<img src=\"/assets-chatroom/images/channels/webchat.png\" alt=\"\">"
                    : element.channelType == "3" ?// channel is messenger
                            "<img src=\"/assets-chatroom/images/channels/messenger.png\" alt=\"\">"
                        : element.channelType == "5" ?// channel is instagram direct message
                                "<img src=\"/assets-chatroom/images/channels/instagram.png\" alt=\"\">"
                            : element.channelType == "7" ?// email
                                    "<img src=\"/assets-chatroom/images/channels/email.png\" alt=\"\">"
                                : element.channelType == "8" ?// SMS
                                        "<img src=\"/assets-chatroom/images/channels/sms.png\" alt=\"\">"
                                        ://channel is others
                                        "<img src=\"/assets-chatroom/images/channels/other.png\" alt=\"\">"
        ) +
        element.name +
        "</div>" +
        "<div class=\"widget-subheading lastest_message\">" + element.lastMessage + "</div>" +
        "</div>" +
        "<div class=\"widget-content-right col-desc\">" +
        "<div class=\"text-muted\">" +
        "<small class=\"pl-2 lastest_datetime\">" + abbreviateDate(element.lastTime) + "</small>" +
        "</div>" +
        "</div>" +
        "</div>" +
        "</div>" +
        "</a>";

    return innerHtml;

}
/*
 * Ao selecionar alguem na barra lateral esquerda
 */
function leftBarSelectAction(selectedId, selectedType, messageId) {

    // Limpa a caixa de enviar mensagens
    $("#send-message-obj").val("");
    $('.app-footer .emojionearea-editor').html('');

    // exibe o rodapé
    $(".app-footer").css('display', 'block');

    // Exibe o botão de toggle do right side bar
    $('.btn-toggle-right-sidebar').addClass('show');

    // Se o right sidebar ainda não está aberto, e se não estamos em Mobile
    if (!$('.right-sidebar').hasClass('show') & $(".app-sidebar").css('width') != $(".app-main").css('width')) {
        // exibe o right side bar
        $('.right-sidebar').addClass('show');
    }

    // desmarca selecao de todos
    $(".app-sidebar__inner ul li").each(function () { $(this).find("a").removeClass("mm-active"); });

    // confere se foi selecionado um contato ou um atendente
    if (selectedType == 1)
        $("#contact" + selectedId + " a").addClass("mm-active"); // marca o contato selecionado
    else
        $("#agent" + selectedId + " a").addClass("mm-active"); // marca o agente selecionado

    // Mobile adjustment
    if ($(".app-sidebar").css('width') == $(".app-main").css('width')) {
        $(".mobile-toggle-nav").toggleClass("is-active");
        $(".app-container").toggleClass("sidebar-mobile-open");
    }

    // se selecionou agente
    if (selectedType == 0) {
        // esconde perfil do contato se estiver aberto
        $("#contact_profile_view").css("display", "none");
        // esconde o botão de inclur Contato
        $("#novo-contato").css("display", "none");

        // localiza os dados do Agent - salvos em variavel global
        var agents = agentList;
        for (var i = 0; i < agents.length; i++) {
            if (agents[i].id == selectedId) {
                updateRightDetailPanel(selectedType, selectedId, agents[i]);
                break;
            }
        }
    }
    // se selecionou Contact
    else if (selectedType == 1) {
        // mostra o botão de inclur Contato
        $("#novo-contato").css("display", "flex");

        // Atualiza o painel 
        updateRightDetailPanel(selectedType, selectedId);
        $("#contact_profile_view").css("display", "block");

        // Configura o botão Incluir do Right SideBar
        $(".btn-novo-contato").addClass("btn-incluir");
        $(".btn-novo-contato").removeClass("btn-cancelar");
        $(".btn-novo-contato").text("Incluir");
        $(".btn-save-contact").attr("data-type", "1"); //configura o botão de submit do formulário para type=1
        $(".btn-collapse-right").prop("disabled", false);
        $(".btn-save-contact").attr("data-type", "1"); //configura o botão de submit do formulário para type=0
    }

    // Salva em globais quem foi clicado
    currentSelectedId = selectedId;
    currentSelectedType = selectedType;

    // Zera referencias globais do historico de mensagens
    msgHistoryFirstTime = null;
    msgHistoryLastTime = null;
    msgHistoryPage = 0;

    // Atualiza o historico de mensagens
    getMsgHistory(selectedType, selectedId, msgHistoryPage, msgHistoryFirstTime, messageId);
}

/*
 * Update Message History
 */
function updateMsgHistory(chattingLogViewList) {

    var html = '';
    for (var i = 0; i < chattingLogViewList.length; i++) {
        if (!currentSelectedType) {
            //agent
            if (chattingLogViewList[i].chattingLog.source == 2) {
                html += htmlFormatMsgHistory(chattingLogViewList[i].chattingLog.id + ":1", 1, chattingLogViewList[i].chattingLog.text, chattingLogViewList[i].quotedLog, chattingLogViewList[i].chattingLog.filename, chattingLogViewList[i].chattingLog.type, chattingLogViewList[i].chattingLog.status, getNowTime(getDateObj(chattingLogViewList[i].chattingLog.time)) + " | " + chattingLogViewList[i].contactName);
            } else if (chattingLogViewList[i].chattingLog.source == 1) {
                html += htmlFormatMsgHistory(chattingLogViewList[i].chattingLog.id + ":0", 0, chattingLogViewList[i].chattingLog.text, chattingLogViewList[i].quotedLog, chattingLogViewList[i].chattingLog.filename, chattingLogViewList[i].chattingLog.type, chattingLogViewList[i].chattingLog.status, getNowTime(getDateObj(chattingLogViewList[i].chattingLog.time)) + " | " + chattingLogViewList[i].contactName);
            } else if (chattingLogViewList[i].chattingLog.source == 3) {
                //from agent
                var dir = 1;
                if (chattingLogViewList[i].chattingLog.ToAgentId == currentSelectedId) dir = 0;
                html += htmlFormatMsgHistory(chattingLogViewList[i].chattingLog.id + ":" + dir, dir, chattingLogViewList[i].chattingLog.text, chattingLogViewList[i].quotedLog, chattingLogViewList[i].chattingLog.filename, chattingLogViewList[i].chattingLog.type, chattingLogViewList[i].chattingLog.status, getNowTime(getDateObj(chattingLogViewList[i].chattingLog.time)) + " | " + chattingLogViewList[i].agentName);
            }
        } else {
            // Contact
            if (chattingLogViewList[i].chattingLog.source == 0)
                //from bot
                html += htmlFormatMsgHistory(chattingLogViewList[i].chattingLog.id + ":0", 0, chattingLogViewList[i].chattingLog.text, chattingLogViewList[i].quotedLog, chattingLogViewList[i].chattingLog.filename, chattingLogViewList[i].chattingLog.type, chattingLogViewList[i].chattingLog.status, getNowTime(getDateObj(chattingLogViewList[i].chattingLog.time)) + " | Bot");
            else if (chattingLogViewList[i].chattingLog.source == 1) {
                //from contact
                html += htmlFormatMsgHistory(chattingLogViewList[i].chattingLog.id + ":1", 1, chattingLogViewList[i].chattingLog.text, chattingLogViewList[i].quotedLog, chattingLogViewList[i].chattingLog.filename, chattingLogViewList[i].chattingLog.type, chattingLogViewList[i].chattingLog.status, getNowTime(getDateObj(chattingLogViewList[i].chattingLog.time)) + " | " + chattingLogViewList[i].contactName);
            } else if (chattingLogViewList[i].chattingLog.source == 2) {
                //from agent
                html += htmlFormatMsgHistory(chattingLogViewList[i].chattingLog.id + ":0", 0, chattingLogViewList[i].chattingLog.text, chattingLogViewList[i].quotedLog, chattingLogViewList[i].chattingLog.filename, chattingLogViewList[i].chattingLog.type, chattingLogViewList[i].chattingLog.status, getNowTime(getDateObj(chattingLogViewList[i].chattingLog.time)) + " | " + chattingLogViewList[i].agentName);
            }
        }
    }

    if (html != '') {
        $("#msg_history").append(html);
        if (msgHistoryPage == 1)
            $("#msg_history").scrollTop($("#msg_history")[0].scrollHeight)
    }

    // update global msgHistoryLastTime
    if (chattingLogViewList.length > 0) {
        msgHistoryLastTime = chattingLogViewList[chattingLogViewList.length - 1].chattingLog.time
    }

}

/**
 * Get SearchedMessages
 * */
function getSearchedMessages(searchKey) {

    if (is_searching)
        startSpinner();

    var form_data = new FormData();
    form_data.append('SearchKey', searchKey);

    $.ajax({
        url: '/Chat/GetSearchedMessages',
        data: form_data,
        cache: false,
        contentType: false,
        processData: false,
        type: 'POST',
        dataType: "json",
        success: function (response) {

            // Update Searched Messages at left side bar
            var html = '';
            for (var i = 0; i < response.searchedMessages.length; i++) {
                html += (i ? "" : "<li class=\"app-sidebar__heading\">Mensagens - pesquisa</li>") +
                    "<li id=\"message" + response.searchedMessages[i].id + "\">" +
                    "<a href=\"#\" onclick=\"leftBarSelectAction('" + response.searchedMessages[i].contact.id + "', 1,'" + response.searchedMessages[i].id + "');\">" +
                    "<div class=\"widget-content p-0\">" +
                    "<div class=\"widget-content-wrapper\">" +
                    "<div class=\"widget-content-left col-desc\">" +
                    "<div class=\"widget-heading\">" +
                    (
                        response.searchedMessages[i].contact.channelType == "0" ? ""//channel is not assigned
                            : response.searchedMessages[i].contact.channelType == "1" ?//channel is whatsapp
                                "<img src=\"/assets-chatroom/images/channels/whatsapp.png\" alt=\"\">"
                                : response.searchedMessages[i].contact.channelType == "2" ?//channel is webchat
                                    "<img src=\"/assets-chatroom/images/channels/webchat.png\" alt=\"\">"
                                    : response.searchedMessages[i].contact.channelType == "3" ?// channel is messenger
                                        "<img src=\"/assets-chatroom/images/channels/messenger.png\" alt=\"\">"
                                        : response.searchedMessages[i].contact.channelType == "5" ?// channel is instagram direct message
                                            "<img src=\"/assets-chatroom/images/channels/instagram.png\" alt=\"\">"
                                            ://channel is others
                                            "<img src=\"/assets-chatroom/images/channels/other.png\" alt=\"\">"
                    ) +
                    response.searchedMessages[i].contact.name +
                    "</div>" +
                    "<div class=\"widget-subheading lastest_message\">" + response.searchedMessages[i].text + "</div>" +
                    "</div>" +
                    "<div class=\"widget-content-right col-desc\">" +
                    "<div class=\"text-muted\">" +
                    "<small class=\"pl-2 lastest_datetime\">" + abbreviateDate(getBrasilianDateTime(response.searchedMessages[i].time)) + "</small>" +
                    "</div>" +
                    "</div>" +
                    "</div>" +
                    "</div>" +
                    "</a>" +
                    "</li>";
            }

            // Fill search bar
            $("#vertical-nav-menu-search").html(html);

            stopSpinner();

        },
        error: (xhr, textStatus, errorThrown) => {
            console.log(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }

    });
}

/*
 * Get Searched Contacts
 */
function getSearchedContacts(searchKey) {

    var form_data = new FormData();
    form_data.append('SearchKey', searchKey);

    $.ajax({
        url: '/Chat/GetSearchedContacts',
        data: form_data,
        cache: false,
        contentType: false,
        processData: false,
        type: 'POST',
        dataType: "json",
        success: function (response) {

            // Renders searched contact list
            renderSearchedContacts(response.searchedContactList);

            // Stop spinner icon
            stopSpinner();

        },
        error: (xhr, textStatus, errorThrown) => {
            // Stop spinner icon
            stopSpinner();

            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }

    });
}
/*
 * Get Searched Agents
 */
function getSearchedAgents(searchKey) {

    startSpinner();

    var form_data = new FormData();
    form_data.append('SearchKey', searchKey);

    $.ajax({
        url: '/Chat/GetSearchedAgents',
        data: form_data,
        cache: false,
        contentType: false,
        processData: false,
        type: 'POST',
        dataType: "json",
        success: function (response) {

            // Update global AgentList
            agentList = response.searchedAgentList;

            // Renders searched AgentList
            renderSearchedAgents(response.searchedAgentList);

            getSearchedContacts(searchKey);

        },
        error: (xhr, textStatus, errorThrown) => {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
            getSearchedContacts(searchKey);
        }

    });
}

/*
 * Get Single Contact
 */
function getSingleContact(searchId) {

    startSpinner();

    var form_data = new FormData();
    form_data.append('searchId', searchId);
    currentSelectedId = searchId;

    $.ajax({
        url: '/Chat/GetSingleContact',
        data: form_data,
        cache: false,
        contentType: false,
        processData: false,
        type: 'POST',
        dataType: "json",
        success: function (response) {

            // Renders searched contact list
            renderSearchedContacts(response.singleContactList);

            // Seleciona o contato - e abre right side bar
            leftBarSelectAction(currentSelectedId, currentSelectedType);

            // marca o contato selecionado
            $("#contact-search-" + currentSelectedId + " a").addClass("mm-active");

            $('#msg_history').attr({ scrollTop: $('#msg_history').attr('scrollHeight') });
 
            stopSpinner();

        },
        error: (xhr, textStatus, errorThrown) => {
            console.log(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
            stopSpinner();
        }

    });
}

/*
 * Search box click
 */
$("#sidebar_search_input").keypress(function (e) {
    if (e.keyCode == 13) {
        is_searching = true;
        $('.app-sidebar__inner .sidebar-search .x svg').css('display', 'inline');
        $('.app-sidebar__inner .sidebar-search .lupa svg').css('display', 'none');
        currentSelectedId = "";
        var searchKey = $("#sidebar_search_input").val()

        // If searchKey starts and ends with double quotte, consider as message search, else consider as contact search
        if (searchKey.startsWith('"') && searchKey.endsWith('"')) {
            getSearchedMessages(searchKey);
        }
        else {
            // Clean search bar
            $("#vertical-nav-menu-search").html("");
            getSearchedAgents(searchKey);
        }

        // Show search bar
        $("#vertical-nav-menu-search").removeClass("hide");
        // Hide contact list
        $("#vertical-nav-menu").addClass("hide");

    }
});

/*
 * Search box close
 */
$('.app-sidebar__inner .sidebar-search .x svg').on('click', function () {
    //Se tiver a váriavel search, é feito o redirecionamento sem as váriaveis de filtro
    if (searchName) {
        location.assign(location.origin)
    }

    $(this).css('display', 'none');
    $('.app-sidebar__inner .sidebar-search .lupa svg').css('display', 'inline');
    $('.app-sidebar__inner .sidebar-search input').val('');
    is_searching = false;

    // Hides search bar
    $("#vertical-nav-menu-search").addClass("hide");
    // Show contact list
    $("#vertical-nav-menu").removeClass("hide");


    getSideBar();
});

/*
 * Event contacts list is scrolled
 */
function contactsScroll() {

    // Check if position of scrooll is at de bottom and its not within a search
    var element = document.getElementById("vertical-nav-menu");
    if (element.offsetHeight + element.scrollTop + 1 >= element.scrollHeight) {
        appendContactList();
    };
}
/*
 * Append Contact List
 *   Gets next older 50 Contacts by LastActivity
 */
function appendContactList() {

    // Start Spinner
    startSpinner();

    // Insert global firstActivity ( older activity that appears at left side bar ) as form data parameter
    var form_data = new FormData();
    form_data.append('firstActivity', firstActivity);

    $.ajax({
        url: '/Chat/AppendContactList',
        data: form_data,
        cache: false,
        contentType: false,
        processData: false,
        type: 'POST',
        dataType: "json",
        success: function (contactList) {

            // Check if is there any elements
            if (contactList.length > 0) {
                // Update firstActivity - older activity
                firstActivity = contactList[contactList.length - 1].lastActivity;

                // Append Contact Info at left side bar
                renderAppendendContacts(contactList);
            }

            // Stop Spinner
            stopSpinner();

        },
        error: (xhr, textStatus, errorThrown) => {
            // Stop Spinner
            stopSpinner();
            // Alert Error
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText )
        }

    });
}
/*
 * APPEND Contact List at the the left side bar
 *     - build <li> element for each contact in list
 *     - insert each element at the end of list ( older contacts by lastActivity )
 */

function renderAppendendContacts(contactList) {

    // Loop contact list
    for (var i = 0; i < contactList.length; i++) {

        // builds <li> element for contact
        var newLi = document.createElement("li");
        newLi.id = "contact" + contactList[i].id;
        newLi.innerHTML = renderLiInnerHtml(contactList[i],'1');

        // gets parent
        let ul = document.getElementById("vertical-nav-menu");

        // lets search if is there any <li> with current id
        var oldLi = document.getElementById(newLi.id);

        // Ensure element doesn't exist yet
        if (typeof (oldLi) == 'undefined' || oldLi == null) {
            // Append child element
            ul.appendChild(newLi);
        }

    }
}

