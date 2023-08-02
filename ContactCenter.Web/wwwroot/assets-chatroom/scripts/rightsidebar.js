/**
 * Reset datalist value when click
 * 
 * @author Daniel
 */
$(document).on('focusin', 'input[list]', function () { $(this).val('') });

/**
 * Add custom field to custom fields accordion
 * 
 * @param {any} customField
 * @author Daniel
 */
function addCustomFields(customField, classname) {
    let strHTML = '';
    let field = customField.field;
    let value = customField.value;
    strHTML = '<div class="form-group"><label>' + field.label + '</label>';
    switch (field.fieldType) {
        case 0:
            strHTML += '<input type="number" fieldType="' + customField.field.fieldType + '" id="input-' + customField.fieldId + '-' + customField.id + '" class="form-control-text ' + classname + ' form-control form-control-sm" value="' + (value ? value : '') + '" />';
            break;
        case 1:
            strHTML += '<input type="number" fieldType="' + customField.field.fieldType + '" id="input-' + customField.fieldId + '-' + customField.id + '" class="form-control-text ' + classname + ' form-control form-control-sm" value="' + (value ? value : '') + '" />';
            break;
        case 2:
            strHTML += '<input type="number" fieldType="' + customField.field.fieldType + '" id="input-' + customField.fieldId + '-' + customField.id + '" class="form-control-text ' + classname + ' form-control form-control-sm" value="' + (value ? value : '') + '" />';
            break;
        case 3:
            strHTML += '<input type="date" fieldType="' + customField.field.fieldType + '" id="input-' + customField.fieldId + '-' + customField.id + '" class="form-control-text ' + classname + ' form-control form-control-sm" value="' + (value ? value : '') + '" />';
            break;
        case 4:
            strHTML += '<input type="time" fieldType="' + customField.field.fieldType + '" id="input-' + customField.fieldId + '-' + customField.id + '" class="form-control-text ' + classname + ' form-control form-control-sm" value="' + (value ? value : '') + '" />';
            break;
        case 5:
            strHTML += '<input type="datetime-local" fieldType="' + customField.field.fieldType + '" id="input-' + customField.fieldId + '-' + customField.id + '" class="form-control-text ' + classname + ' form-control form-control-sm" value="' + (value ? value : '') + '" />';
            break;
        case 6:
            strHTML += '<input type="text" fieldType="' + customField.field.fieldType + '" id="input-' + customField.fieldId + '-' + customField.id + '" class="form-control-text ' + classname + ' form-control form-control-sm" value="' + (value ? value : '') + '" />';
            break;
        case 7:
            strHTML += '<textarea fieldType="' + customField.field.fieldType + '" id="input-' + customField.fieldId + '-' + customField.id + '" class="form-control-text ' + classname + ' form-control form-control-sm">' + (value ? value : '') + '</textarea>';
            break;
        case 8:
            strHTML += '<input fieldType="' + customField.field.fieldType + '" type="text" id="input-' + customField.fieldId + '-' + customField.id + '" list="datalists_' + field.id + '" class="form-control-text ' + classname + ' form-control form-control-sm" value="' + (value ? value : '') + '" autocomplete="off" />';
            strHTML += '<datalist id="datalists_' + field.id + '">';
            if (field.dataListValues) {
                field.dataListValues.forEach((elmnt) => strHTML += '<option value="' + elmnt.value + '">' + elmnt.value + '</option>');
            }
            strHTML += '</datalist>'
            break;
        case 9:
        case 10:
            var filepath = customField.value;
            strHTML += '<span>&nbsp;</span>'
            strHTML += '<div class="container-' + customField.id + '">';
            if (filepath) {
                strHTML += '<input type="hidden" id="input-' + customField.fieldId + '-' + customField.id + '" class="' + classname + ' file-path" value="' + filepath + '" />';
                strHTML += '<div class="btn-group ' + customField.fieldId + '-' + customField.id + '" role="group" aria-label="Basic outlined example">';
                strHTML += '<button type="button" data-filepath="' + filepath + '" data-fieldId="' + customField.fieldId + '" class="btn btn-purple upload-options upload-download">Download</button>';
                strHTML += '<button type="button" data-filepath="' + filepath + '" data-fieldId="' + customField.fieldId + '" class="btn btn-purple upload-options upload-delete-' + classname +'">Excluir</button>';
                strHTML += '</div>';
            } else {
                strHTML += '<input type="file" id="file-' + customField.fieldId + '-' + customField.id + '" class="' + classname + ' file-input" />';
            }
            strHTML += '</div>';
            break;

        default:
            strHTML += '<input fieldType="' + customField.field.fieldType + '" type="text" class="' + classname + ' form-control form-control-sm form-control-text" value="' + (value ? value : '') + '" />';
            break;
    }
    strHTML += '</div>';
    return strHTML;
}

/**
 * Init Custom Fields Accordion
 * 
 * @param {id} contactId
 * @author Daniel
 */
function initCustomFields(contactFieldValues) {
    $('.right-sidebar .custom-fields').html('');
    let strHTML = '';
    contactFieldValues.forEach((elmnt) => {
        strHTML += addCustomFields(elmnt,'form-custom-field');
    });
    $('.right-sidebar .custom-fields').html(strHTML);
}

/**
 * Init Cards Accordion
 * 
 * @param {any} id
 * @author Daniel
 */
function initCards(id) {
    let cardsAccordion = $('#contact_cards .card-body');
    cardsAccordion.html('');
    getAjax('/api/Cards/?contactId=' + id, APIHeader, false, false).then((res) => {
        let strHTML = '';
        if (res.length > 0) {
            res.forEach((elmnt) => {
                strHTML += `
                <a data-id="${elmnt.id}" data-target="#${elmnt.id}" class="board-card">
                <div class="card inner-card">
                    <div class="card-body contact-card ${elmnt.color}">
                    <div class="name-fields">
                        <div class="name card-title">
                            <h1>${elmnt.stage.board.name}</h1>
                            <h2>${elmnt.stage.name}</h2>
                        </div>
                        <div class="fields">`;
                var obj = elmnt.cardFieldValues;
                for (var prop in obj) {
                    if (obj.hasOwnProperty(prop)) {
                        // Se for imagem ou arquivo, mostra apenas um check mark se houver valor
                        var value;
                        if (obj[prop].field.fieldType == 10 | obj[prop].field.fieldType == 9)
                            value = obj[prop].value ? 'arquivo' : '';
                        else if (obj[prop].field.fieldType == 3)
                            value = getBrasilianDate(obj[prop].value);
                        else if (obj[prop].field.fieldType == 4 )
                            value = getTime(obj[prop].value);
                        else if (obj[prop].field.fieldType == 5)
                            value = getBrasilianDateTime(obj[prop].value);
                        else
                            value = obj[prop].value ? obj[prop].value : '';
                        strHTML += `<div class="field">
                                    <p><b>${obj[prop].field.label}:</b> <span *ngIf="field.value">${value}</span></p>
                                </div>`;
                    }
                }
                strHTML += `</div>
                    </div>
                    <div class="dates">
                        <div class="createdAt">Criado: ${dateConverter(elmnt.createdDate)}</div>
                        <div class="updatedAt">Alterado: ${dateConverter(elmnt.updatedDate)}</div>
                    </div>
                    </div>
                </div></a>`;
            });
        } else strHTML = 'Nenhum cartão está disponível.';
        cardsAccordion.html(strHTML);
    });
}
/**
 * Update Card Modal 
 * 
 * @param {any} res
 * @author Daniel
 */
function updateCardModal(res) {
    currentCard = res;
    $('#cardModalLabel').html(currentContact.fullName);
    $('#card_modal_phone').html(formatMobilePhone(currentContact.mobilePhone));
    $('#card_modal_email').html(currentContact.email);
    let strHTML = '';
    if (res.cardFieldValues.length > 0) {
        res.cardFieldValues.forEach((elmnt) => {
            strHTML += addCustomFields(elmnt,'card-custom-field');
        })
    } else {
        strHTML = 'Nenhum campo personalizado está disponível.'
    }
    $('#card_modal_custom_fields').html(strHTML);
    $('#cardModal .btn-card-color').each(function () {
        if ($(this).hasClass('active')) $(this).removeClass('active');
        if ($(this).attr('data-color') == res.color) {
            $(this).addClass('active');
        }
    });
}

/**
 * Init modal and show when click card button
 *
 * @author Daniel
 */
$(document).on('click', '.board-card', function () {
    let id = $(this).attr('data-id');
    if (id) {
        getAjax('/api/Cards/' + id, APIHeader, false, false).then((res) => {
            updateCardModal(res);
            $('.btn-update-card').attr('data-id', id);
            $('.btn-update-card').attr('data-type', 1);
            $('.btn-delete-card').show();
            $('.btn-delete-card-confirm').attr('data-id', id);
            $('#cardModal').modal('show');
            initStageSelector(res.stage.boardId, res.stageId);
        });
    }
});

/**
 * Init Chat Channel selector
 * 
 * @author Felipe Daiello
 */
function initChatChannelSelector() {
    let chatchannelSelector = $('#contact_channel_selector');
    let strHTML = '';
    let count = 0;
    getAjax('/api/ChatChannels', APIHeader, false, false).then((res) => {
        res.forEach((channel) => {
            // Não permite trocar para WhatsApp GupShup, WebChat, Emulator
            if ((channel.channelType == currentContact.channelType && (channel.channelSubType == currentContact.channelSubType || !currentContact.channelSubType ))
                || (channel.channelType != 2 && channel.channelType != 4 && channel.channelType != 5 && channel.channelSubType != 1)
                || currentContact.channelType == 0) {
                if (currentContact.chatChannelId == channel.id) strHTML += '<option value="' + channel.id + '" selected>' + channel.name + '</option>';
                else strHTML += '<option value="' + channel.id + '">' + channel.name + '</option>';
            };
            count++;
        });
        if (count > 1)
            strHTML = '<option value="">--</option>' + strHTML;
        chatchannelSelector.html(strHTML); 
    });
}

/**
 * Init Stage selector at card Modal
 * 
 * @author Felipe Daiello
 */
function initStageSelector(boardId, stageId) {
    let stageSelector = $('#stage_selector');
    let strHTML = '';
    getAjax('/api/Stages?boardId=' + boardId, APIHeader, false, false).then((res) => {
        // Se não tem estágios
        if (res.length == 0) {
            let newStage = {
                id: 0,
                boardId: boardId,
                label: '',
                name: '',
                order: 0
            };
            postAjax('/api/Stages', APIHeader, JSON.stringify(newStage), false).then((stage) => {
                strHTML += '<option value="' + stage.id + '" selected>' + stage.name + '</option>';
                $("#div-stage-selector").addClass("hide");
                stageSelector.html(strHTML);
            })
        }
        else {
            res.forEach((stage) => {
                if (stage.id == stageId)
                    strHTML += '<option value="' + stage.id + '" selected>' + stage.name + '</option>';
                else
                    strHTML += '<option value="' + stage.id + '">' + stage.name + '</option>';
            });
            stageSelector.html(strHTML);
            if (res.length > 1)
                $("#div-stage-selector").removeClass("hide");
            else
                $("#div-stage-selector").addClass("hide");
        }
    });
}

/**
 * Init application user selector
 * 
 * @author Daniel
 */
function initApplicationUserSelector() {
    let agentSelector = $('#contact_agent_selector');
    let strHTML = '';
    getAjax('/api/ApplicationUsers', APIHeader, false, false).then((res) => {
        strHTML += '<option value="">--</option>';
        res.forEach((elmnt) => {
            if (currentContact.applicationUserId == elmnt.id) strHTML += '<option value="' + elmnt.id + '" selected>' + elmnt.fullName + '</option>';
            else strHTML += '<option value="' + elmnt.id + '">' + elmnt.fullName + '</option>';
        });
        agentSelector.html(strHTML);
    });
}
/**
 * Init board selector 
 * 
 * @author Daniel
 */
function initBoardSelector() {
    let boardSelector = $('#contact_board_selector');
    let strHTML = '';
    getAjax('/api/Boards/CurrentUser', APIHeader, false, false).then((res) => {
        //Se não houver nenhum quadro, é escondido o collapse de criação de cartões
        if(res.length == 0){ $("#all_cards").css("display", "none") }
        strHTML += '<option>Selecione a lista</option>';
        res.forEach((elmnt) => strHTML += '<option value="' + elmnt.id + '">' + elmnt.name + '</option>');
        boardSelector.html(strHTML);
    });
}

/**
 * Init Department selector
 * 
 * @author FelipeDaiello
 */
function initDepartmentSelector(departmentId) {
    let departmentSelector = $('#contact_department_selector');
    let strHTML = '';
    getAjax('/api/Departments', APIHeader, false, false).then((res) => {
        strHTML += '<option value="">--</option>';
        res.forEach((elmnt) => {
            if (departmentId == elmnt.id) strHTML += '<option value="' + elmnt.id + '" selected>' + elmnt.name + '</option>';
            else strHTML += '<option value="' + elmnt.id + '">' + elmnt.name + '</option>';
        });
        departmentSelector.html(strHTML);
    });
}

/*
 * Formata um celular em padrão User Friendly
 */
function formatMobilePhone(phone) {
    if (phone) {
        phone = '(' + phone.substring(0,2) + ') ' + phone.substring(2,7) + '-' + phone.substring(7)
    }
    return phone;
}
/**
 * Show Right Sidebar
 * @param {any} type // 0 - application user, 1 - contact
 * @param {any} id  
 * @param {any} user
 * @author Daniel
 */
function updateRightDetailPanel(type, id, user) {

    // Show right sidebar button on header
    $('.btn-toggle-right-sidebar').show();

    // Se recebeu os dados do contato
    if (user) {
        // Preenche os campos
        $('.right-sidebar #contact_name').val(user.fullName);
        $('.right-sidebar #contact_email').val(user.email);
        $('.right-sidebar #contact_mobilephone').val(formatMobilePhone(user.phone));
        if (user.avatar) {
            $('.right-sidebar .contact-image').html("<img src=\"" + user.avatar + "\" onclick=\"ImageZoom(this)\" alt=\"" + user.fullName + "\">");
        }
        else {
            $('.right-sidebar .contact-image').html(getIconByName(user.fullName, ''));
        }
        initDepartmentSelector(user.departmentId);
    }

    /* If contact selected */
    if (type == 1) {
        getAjax('/api/Contacts/' + id, APIHeader, false, false).then((res) => {
            currentContact = res;
            //Se nao tinha recebido os dados do contato
            if (!user) {
                // Preenche os campos
                $('.right-sidebar #contact_name').val(res.fullName);
                $('.right-sidebar #contact_email').val(res.email);
                $('.right-sidebar #contact_mobilephone').val(formatMobilePhone(res.mobilePhone));
                if (res.avatar) {
                    $('.right-sidebar .contact-image').html("<img src=\"" + res.avatar + "\" onclick=\"ImageZoom(this)\" alt=\"" + res.fullName + "\">");
                }
                else {
                    $('.right-sidebar .contact-image').html(getIconByName(res.fullName, ''));
                }
            }
            //Se não houver nenhum campo personalizado esconde o collapse correspondente
            if (res.contactFieldValues.length == 0) {
                $(".btn_custom_fields").css("display", "none");
            }
            initCustomFields(res.contactFieldValues);
            initChatChannelSelector();
            initApplicationUserSelector();
            initDepartmentSelector(res.departmentId);
        });
        $('button[data-target="#contact_info"]').html('<span>Contato</span>');
        $('#custom_field_card').show();
        $('#all_cards').show();
        $('#add_new_card').show();
        // Show static contact fields
        $('#contact_channel').show();
        $('.btn-save-contact').show();
        $('#contact_board_selector').attr('data-id', id);
        $('.btn-save-contact').attr('data-type', 1);
        $('#contact_agent').show();
        initCards(id);
        initBoardSelector();

    } else {
        $('button[data-target="#contact_info"]').html('<span>Atendente</span>');
        $('#custom_field_card').hide();
        $('#all_cards').hide();
        $('#add_new_card').hide();

        // hide static contact fields
        $('#contact_channel').hide();
        $('#contact_agent').hide();
        $('.btn-save-contact').hide();
        $('.btn-save-contact').attr('data-type', 0);
    }

}

$(document).on("click", ".btn-incluir", function () {

    currentSelectedId = "";

    $("#contact_name").val(""); //configura os campos de contato com o valor em branco
    $("#contact_email").val(""); //configura os campos de contato com o valor em branco
    $("#contact_mobilephone").val(""); //configura os campos de contato com o valor em branco
   // $("#contact_channel_selector").val(""); //configura os campos de contato na primeira posição do select
    $("#contact_channel_selector").prop("disabled", false); //ativa o campo - caso estivesse desativado
    $("#contact_agent_selector").val(""); //configura os campos de contato na primeira posição do select
    $("#contact_department_selector").val(""); //configura os campos de contato na primeira posição do select
    $(".contact-image").css("background-image", "none"); //Apaga a contact-image do contato atual
    $(".left-panel-generated-icon").html(""); //caso não haja uma imagem apaga o texto das iniciais do nome
    $(".btn-save-contact").attr("data-type", "0"); //configura o botão de submit do formulário para type=0
    $('.right-sidebar .contact-image').html(''); // Apaga o Avatar

    //desabilita campos
    $(".btn-collapse-right").prop("disabled", true);
    $(".btn-novo-contato").removeClass("btn-incluir");      //remove btn-incluir
    $(".btn-novo-contato").addClass("btn-cancelar");        //adiciona o btn-cancelar de incluir contato
    $(".btn-novo-contato").text("Cancelar");

    // esconde o rodapé
    $(".app-footer").css('display', 'none');

    // desmarca selecao de todos os contatos no left side bar
    $(".app-sidebar__inner ul li").each(function () { $(this).find("a").removeClass("mm-active"); });

    // limpa msg history
    $("#msg_history").html("");

});

$(document).on("click", ".btn-cancelar", function () {
    if (currentContact.id)
        // Chama ação de seleção no left side bar para mostrar novamente o chatroom
        leftBarSelectAction(currentContact.id, "1");
});

/**
 * Save updated contact info in right sidebar 
 * 
 * @author Daniel
 */
$(document).on('click', '.btn-save-contact', function () {
    let type = $(this).attr('data-type');
    //Limpa todos os campos de contato
    currentContact.contactFieldValues = new Array();
    currentContact.name = $('#contact_name').val();
    currentContact.fullName = $('#contact_name').val();
    currentContact.email = $('#contact_email').val();
    currentContact.mobilePhone = $('#contact_mobilephone').val();
    currentContact.chatChannelId = $('#contact_channel_selector').val();
    currentContact.applicationUserId = $('#contact_agent_selector').val();
    currentContact.departmentId = $('#contact_department_selector').val();

    // Nick name comes from whatsapp. Name is showed at right side bar
    if (currentContact.name)
        currentContact.nickName = currentContact.name;
    else {
        currentContact.name = currentContact.nickName;
        currentContact.fullName = currentContact.nickName;
    }

    // Add new contact
    if (type == 0) {
        currentContact.id = null;
        currentContact.source = 1                   // Source = Agent;
        currentContact.pictureFileName = null;
        currentContact.contactFieldValues = null;
        currentContact.lastText = "";
        postAjax('/api/Contacts/', APIHeader, JSON.stringify(currentContact)).then((res) => {
            $(".btn-novo-contato").removeClass("btn-cancelar");//remove o cancelar de incluir contato
            $(".btn-novo-contato").addClass("btn-incluir");//adiciona o btn incluir novamente
            $(".btn-novo-contato").text("Incluir");
            $(".btn-save-contact").attr("data-type", "1"); //configura o botão de submit do formulário para type=1
            $(".btn-collapse-right").prop("disabled", false);//habilita novamente campos desabilitados

            currentContact.id = res.id;
            currentContact.applicationUserId = res.applicationUserId;
            currentContact.firstActivity = res.firstActivity;
            currentContact.lastActivity = res.lastActivity;
            currentContact.lastText = res.lastText;
            currentContact.contactFieldValues = res.contactFieldValues;
            currentContact.name = res.name;
            currentContact.fullName = res.fullName;
            currentContact.email = res.email;
            currentContact.mobilePhone = res.mobilePhone;
            currentContact.chatChannelId = res.chatChannelId;

            $('.right-sidebar .contact-image').html(getIconByName(currentContact.fullName, ''));

            leftBarSelectAction(res.id, "1");//carrega o chatroom do novo contato

            // Se recebeu flag dizendo que não tem WhatsApp, alerta
            if (!res.hasWhatsApp)
                alert("Este número não tem WhatsApp");

        }); // add database
    }
    // Update contact
    else if (type == 1) {
        if (currentContact.unAnsweredCount > 0) {
            // Ressets unanswered count when saving
            currentContact.unAnsweredCount = 0;
            var div = document.querySelectorAll("#contact" + currentContact.id + " .unAnsweredCount-icon");
            //If it isn't "undefined" and it isn't "null", then it exists.
            if (typeof (div) != 'undefined' && div != null && div.length > 0 ) {
                // element exist, hide it
                div[0].classList.add("hide");
            }
            div = document.querySelectorAll("#contact" + currentContact.id + " .unAnsweredCount-for-avatar");
            //If it isn't "undefined" and it isn't "null", then it exists.
            if (typeof (div) != 'undefined' && div != null && div.length > 0) {
                // element exist, hide it
                div[0].classList.add("hide");
            }
        }
        putAjax('/api/Contacts/' + currentContact.id, APIHeader, JSON.stringify(currentContact));

    }

    // Atualiza o left sidebar
    lastActivity = null;
    getSideBar();

});

/**
 * Save contact custom fields in right sidebar
 * 
 * @author Daniel
 */
$(document).on('click', '.btn-save-custom-fields', function (event) {
    event.preventDefault;
    // update current contact data
    $('.form-custom-field').each(function (index) {
        if (currentContact.contactFieldValues[index] && currentContact.contactFieldValues[index].field.fieldType != 9 && currentContact.contactFieldValues[index].field.fieldType != 10) {
            var fieldType = $(this).attr("fieldType");
            if (currentContact.contactFieldValues[index].field.fieldType == fieldType) {
                currentContact.contactFieldValues[index].value = $(this).val();
            }
        }

    });
    // update database
    putAjax('/api/Contacts/' + currentContact.id, APIHeader, JSON.stringify(currentContact));
});


/**
 * Update Card when click update button on card modal
 * 
 * @author Daniel
 */
$(document).on('click', '.btn-update-card', function () {
    let type = $(this).attr('data-type');        // 0 - create, 1 - update
    $('.card-custom-field').each(function (index) {
        if ($(this).val()) {
            // para os campos do tipo 9 e 10 ( arquivo ) nao pega o nome
            if (currentCard.cardFieldValues[index].field.fieldType != 10 & currentCard.cardFieldValues[index].field.fieldType != 9) {
                currentCard.cardFieldValues[index].value = $(this).val();
            }
        }
    });
    // Card Color
    let color = $('.btn-card-color.active').find('input');
    currentCard.color = $(color).val();

    // Card Stage
    let stageId = $("#stage_selector").val();
    currentCard.stageId = stageId;

    if (type == 0) {
        postAjax('/api/Cards', APIHeader, JSON.stringify(currentCard)).then((res) => {
            // update right side bar
            initCards(currentCard.contactId);
            // close modal
            $('#cardModal').modal('hide');
            //esconde o select de seleção de quadro
            $('.select-board').css({ 'display': 'none' });
            //esconde o botão 'Cancelar'
            $("#cancela-novo-cartao").css({ 'display': 'none' });
            //Mostra o botão 'Incluir'
            $("#novo-cartao").css({ 'display': 'block' });
        });
    } else if (type == 1) {
        let id = $(this).attr('data-id');
        putAjax('/api/Cards/' + id, APIHeader, JSON.stringify(currentCard)).then(() => {
            let boardCard = $('.board-card[data-id="' + id + '"]');
            boardCard.removeAttr('class');
            boardCard.addClass('board-card ' + currentCard.color);
            $('#cardModal').modal('hide');
            initCards(currentCard.contactId);
        });
    }
});
/** 
 *  Evento de fechar a modal
 *  
 *  @author: Felipe
 *  
 * 
 */
$(document).on('click', '.close', function () {
    $('#cardModal').modal('hide');
});

/** 
 * Show confirm delete when click delete button on card modal
 * 
 * @author Daniel
 */
$(document).on('click', '.btn-delete-card', function () {
    $(this).hide();
    $('#cardModal .delete-confirm').toggle();
});

/**
 * Delete card when click delete confirm button on card modal
 * 
 * @author Daniel
 */
$(document).on('click', '.btn-delete-card-confirm', function () {
    let id = $(this).attr('data-id');
    deleteAjax('/api/Cards/' + id, APIHeader).then(() => {
        let boardCard = $('.board-card[data-id="' + id + '"]');
        boardCard.remove();
        if ($('.board-card').length == 0) $('#contact_cards .card-body').html('Nenhum cartão está disponível.');
        $('#cardModal .delete-confirm').toggle();
        $('.btn-delete-card').show();
        $('#cardModal').modal('hide');
    });
});

/**
 * Show delete button when click cancel delete on card modal
 * 
 * @author Daniel
 */
$(document).on('click', '.btn-delete-card-cancel', function () {
    $('#cardModal .delete-confirm').toggle();
    $('.btn-delete-card').show();
});


/**
 * Botão que abre e fecha o right side bar
 *
 */
$(document).on('click', '.btn-toggle-right-sidebar', function () {
    $(this).toggleClass('rotate');
    $('.right-sidebar').toggleClass('show');
    // Se em Mobile
    if ($(".app-sidebar").css('width') == $(".app-main").css('width')) {
        // Esconde o menu Header
        $(".app-header__content").toggleClass("header-mobile-open");
    }

});

/*
 * Mostra a modal para editar um cartao
 */
function showCardModal(boardId, contactId) {
    getAjax('/api/Cards/NewTemplate?boardId=' + boardId, APIHeader, false, false).then((res) => {
        res.contactId = contactId;
        updateCardModal(res);
        $('.btn-update-card').attr('data-type', 0);
        $('.btn-delete-card').hide();
        initStageSelector(boardId);
        $('#cardModal').modal('show');
    });
}

/**
 * Add new card
 *
 * @author Daniel
 */
$(document).on('change', '#contact_board_selector', function () {
    let boardId = $(this).val();
    let contactId = $(this).attr('data-id');
    showCardModal(boardId, contactId);
});

//evento do botão 'Incluir' do collapse de cartões
$(document).on('click', '#novo-cartao', function(){
    $('#contact_cards').addClass('show'); //expande o collapse de cartões
    $('.select-board').css({ 'display': 'block' }); //mostra o select de seleção de quadro
    $(this).css({ 'display': 'none' }); //esconde o botão 'Incluir'
    $("#cancela-novo-cartao").css({ 'display': 'block' }); //Mostra um botão para cancelar o evento de incluir novo cartão
});

//Evento de cancelar a inclusão de novo cartão do botão 'Cancelar'
$(document).on('click', '#cancela-novo-cartao', function(){
    $('.select-board').css({ 'display': 'none' }); //esconde o select de seleção de quadro
    $(this).css({ 'display': 'none' }); //esconde o botão 'Cancelar'
    $("#novo-cartao").css({ 'display': 'block' }); //Mostra o botão 'Incluir'
    $("#contact_board_selector").val("Selecione a lista"); //Reseta o select de quadro para a primeira opção
});

//Se o modal for fechado reseta o select de quadro para a primeira opção
$(document).on('hidden.bs.modal', '#cardModal', function(){
    $("#contact_board_selector").val("Selecione a lista");
});

//upload de arquivo de campo personalizado
$(document).on("change", ".file-input", function () {
    // Pega o ID do campo INPUT deste arquivo
    var id_file = $(this).prop("id");

    //Separa o fieldId e o id dos campos configurado no input file
    var path_file = id_file.split("-");

    //id único para cada grupo de opções, com o fieldId e o id dos campos personalizados do contato
    var formData = new FormData();
    formData.append('file', $(this).prop('files')[0]);

    //Salva o arquivo
    postAjax('/api/Files', APIHeader, formData, true, false, false, false, true, "Arquivo recebido.")
        .then(
            (res) => {
                // Procura nos Custom Fields pelo campo com mesmo ID do sufixo do input
                $('.form-custom-field').each(function (index) {
                    //path_file[1] é igual ao fieldId pego no atributo id do input file clicado
                    if (currentContact.contactFieldValues[index].fieldId == path_file[1]) {
                        currentContact.contactFieldValues[index].value = res.fileName;
                    }
                });
                // Procura nos Card fields pelo campo com mesmo ID do sufixo do input
                $('.card-custom-field').each(function (index) {
                    if (currentCard.cardFieldValues[index].fieldId == path_file[1]) {
                        currentCard.cardFieldValues[index].value = res.fileName;
                    }
                });

            }
        );
});

//upload options
//excluir file dentro dos Custom Fields
$(document).on("click", ".upload-delete-form-custom-field", function () {

    var filename = $(this).attr('data-filepath'); //Nome do arquivo
    var fieldId = $(this).attr("data-fieldId"); //FieldId usada para determinar um atributo único para os botões de opções de upload
    var excluir = confirm("Excluir arquivo?");

    if (excluir == true) {
        deleteAjax('/api/Files?filename=' + filename, APIHeader);

        $('.form-custom-field').each(function (index) {
            if (currentContact.contactFieldValues[index].fieldId == fieldId) {
                currentContact.contactFieldValues[index].value = "";
                //Remove os botões de opções de upload
                $("." + currentContact.contactFieldValues[index].fieldId + "-" + currentContact.contactFieldValues[index].id).remove();
                //Template que monta novamente o input file e...
                var template = '<input type="file" id="file-${currentContact.contactFieldValues[index].fieldId}-${currentContact.contactFieldValues[index].id}" class="file-input" />';
                //...adiciona no seu respectivo lugar
                $(".container-" + currentContact.contactFieldValues[index].id).html(template);
            }
        });
        // update database
        putAjax('/api/Contacts/' + currentContact.id, APIHeader, JSON.stringify(currentContact), true, true, "application/json", false);
    }
});

//upload options
//excluir file dentro dos Card Fields
$(document).on("click", ".upload-delete-card-custom-field", function () {

    var filename = $(this).attr('data-filepath'); //Nome do arquivo
    var fieldId = $(this).attr("data-fieldId"); //FieldId usada para determinar um atributo único para os botões de opções de upload
    var excluir = confirm("Excluir arquivo?");

    if (excluir == true) {
        deleteAjax('/api/Files?filename=' + filename, APIHeader);

        $('.card-custom-field').each(function (index) {
            if (currentCard.cardFieldValues[index].fieldId == fieldId) {
                currentCard.cardFieldValues[index].value = "";
                //Remove os botões de opções de upload
                $("." + currentCard.cardFieldValues[index].fieldId + "-" + currentCard.cardFieldValues[index].id).remove();
                //Template que monta novamente o input file e...
                var template = '<input type="file" id="file-${currentCard.cardFieldValues[index].fieldId}-${currentCard.cardFieldValues[index].id}" class="file-input" />';
                //...adiciona no seu respectivo lugar
                $(".container-" + currentCard.cardFieldValues[index].id).html(template);
            }
        });
        // update database
        putAjax('/api/Cards/' + currentCard.id, APIHeader, JSON.stringify(currentCard), true, true, "application/json", false);
    }
});

//Download do arquivo
$(document).on("click", ".upload-download", function () {
    var filename = $(this).attr("data-filepath");
    if (filename.startsWith("http")) {
        window.open(filename);
    }
    else {
        getAjax('/api/Files?filename=' + filename, APIHeader).then((res) => window.open(res.url));
    }
});


