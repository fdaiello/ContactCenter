/**
 * Delete an Agent by administrator
 * */
function deleteAgentAction() {
    form_data = new FormData();
    form_data.append('id', $("#edit_userid").val());
    $.ajax({
        url: '/Chat/DeleteAgent',
        data: form_data,
        cache: false,
        contentType: false,
        processData: false,
        type: 'POST',
        dataType: "json",
        success: function (response) {
            //alert(response);
            $('.btn-modal-cloase').trigger('click');
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}
/**
 * update an Agent by administrator
 * */
function editAgentAction() {
    if ($("#edit_fullname").val() == '') {
        $("#edit_fullname").focus();
        return;
    }
    if ($("#edit_nickname").val() == '') {
        $("#edit_nickname").focus();
        return;
    }
    if ($("#edit_username").val() == '') {
        $("#edit_username").focus();
        return;
    }
    var file_data = $('#edit_avatar').prop('files')[0];
    form_data = new FormData();
    form_data.append('id', $("#edit_userid").val());
    form_data.append('avatar', file_data);
    form_data.append('fullname', $("#edit_fullname").val());
    form_data.append('nickname', $("#edit_nickname").val());
    form_data.append('username', $("#edit_username").val());
    form_data.append('role', $("#edit_role").val());
    form_data.append('newpassword', $("#new_password").val());
    $.ajax({
        url: '/Chat/EditAgent',
        data: form_data,
        cache: false,
        contentType: false,
        processData: false,
        type: 'POST',
        dataType: "json",
        success: function (response) {
            $('.btn-modal-cloase').trigger('click');
            location.reload(true); 
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}
/**
 * update an Customer by administrator
 * */
function editCustomerAction() {
    if ($("#edit_fullname").val() == '') {
        $("#edit_fullname").focus();
        return;
    }
    if ($("#edit_nickname").val() == '') {
        $("#edit_nickname").focus();
        return;
    }
    if ($("#edit_username").val() == '') {
        $("#edit_username").focus();
        return;
    }
    var file_data = $('#edit_avatar').prop('files')[0];
    form_data = new FormData();
    form_data.append('id', $("#edit_userid").val());
    form_data.append('avatar', file_data);
    form_data.append('fullname', $("#edit_fullname").val());
    form_data.append('nickname', $("#edit_nickname").val());
    form_data.append('username', $("#edit_username").val());
    form_data.append('reset_password', $("#reset_password").val());
    $.ajax({
        url: '/Chat/EditCustomer',
        data: form_data,
        cache: false,
        contentType: false,
        processData: false,
        type: 'POST',
        dataType: "json",
        success: function (response) {
            $('.btn-modal-cloase').trigger('click');
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}
/**
 * Validate Password
 * */
function validatePass() {
    $(".validate-label").css('display', 'none');
    var pass = $("#add_password").val();
    var res = true;
    if (pass.length < 6) {
        $("#validate_length").css('display', 'block');
        res = false;
    }
    var regx = /^.*?[^A-Za-z0-9, -]+[A-Za-z0-9, -]*$/;
    if (!regx.test(pass)) {
        $("#validate_alpha").css('display', 'block');
        res = false;
    }
    var regx = /(?=.*\d)/;
    if (!regx.test(pass)) {
        $("#validate_digit").css('display', 'block');
        res = false;
    }
    var regx = /(?=.*[A-Z])/;        // use positive look ahead to see if at least one upper case letter exists
    if (!regx.test(pass)) {
        $("#validate_upper").css('display', 'block');
        res = false;
    }
    var regx = /(?=.*[a-z])/;        // use positive look ahead to see if at least one lower case letter exists
    if (!regx.test(pass)) {
        $("#validate_lower").css('display', 'block');
        res = false;
    }
    $("#add_password").focus();
    return res;
}

/**
 * logout action
 **/
function logoutAction() {
    $.ajax({
        url: 'Account/Logout',
        type: 'POST',
        dataType: "json"
    });
}

/*
 * View Customer button Action - 
 */
function viewCustomerAction() {

    form_data = new FormData();
    form_data.append('customerId', currentSelectedId);
    $.ajax({
        url: '/Chat/GetCustomerExternalAccounts',
        cache: false,
        contentType: false,
        processData: false,
        data: form_data,
        type: 'POST',
        dataType: "text",
        success: function (response) {
            var html = "";
            var data = JSON.parse(response)
            for (var i = 0; i < data.externalAccounts.length; i++) {

                html += "<div class=\"form-row\" style=\"margin-bottom: 10px\"><div class=\"col-md-3\" style=\"padding-top: 7px;text-align: center;\"><label for=\"external_email_" + i + "\">Email</label></div><div class=\"col-md-8\"><input name=\"external_email_" +i + "\" id=\"external_email_" + i + "\" type=\"text\" class=\"form-control\" value=\"" + data.externalAccounts[i].email + "\"></div></div>";
                html += "<div class=\"form-row\" style=\"margin-bottom: 10px\"><div class=\"col-md-3\" style=\"padding-top: 7px;text-align: center;\"><label for=\"external_email_" + i + "\">Celular</label></div><div class=\"col-md-8\"><input name=\"external_phone" + i + "\" id=\"external_phone_" + i + "\" type=\"text\" class=\"form-control\" value=\"" + data.externalAccounts[i].phone + "\"></div></div>";
            }
            $("#externalaccounts").html(html);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}