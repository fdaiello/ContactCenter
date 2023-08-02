/*
 * Script para Admin - CRM - Landing Pages
 * @author: Felipe Daiello
 */

var grid;
var currentRow;
var landing = {
    id:  0,
    title: '',
    boardId: 0,
    pageViews: 0,
    leads: 0,
    createdDate: null,
    html: '',
    jsonContent: '',
    thumbnailUrl: '',
    index: 0,
    code: '',
    emailAlert: '',
    redirUri: ''
}

var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};

/**
 * Save fields at modal to DB
 * */
$(document).on('click', '#modal-landing .btn-save-field', function () {

    var title = $("#modal-landing-title").val();
    var boardId = $("#modal-landing-board").val();
    var redirUri = $("#modal-landing-uri").val();
   
    // Validação dos campos
    if (title == '') {
        $("#modal-landing-title").attr("style", "border-color:red;");
        $("#modal-landing-title").focus();
        alert('Digite o titulo');
        return false;
    } else if (boardId == '0') {
        $("#modal-landing-board").attr("style", "border-color:red;");
        alert('Escolha uma lista');
        $("#modal-landing-board").focus();
        return false;
    } else if (redirUri && !validateUrl(redirUri)) {
        $("#modal-landing-uri").attr("style", "border-color:red;");
        alert('Informe uma URL válida, começando com http:// ou https://');
        $("#modal-landing-uri").focus();
        return false;
    }

    landing.title = title;
    landing.boardId = boardId;
    landing.redirUri = redirUri;

    // Salva a mensagem
    SaveLanding(landing)

    // Fecha a modal
    $(".modal-close-button")[0].click();
}); 

function SaveLanding(landing) {

    var method;
    var url = '/api/Landings';
    if (landing.id != 0) {
        method = 'PUT';
        url += '/' + landing.id;
    }
    else
        method = 'POST'

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        headers: APIHeader,
        data: JSON.stringify(landing),
        success: function (newLanding) {
            if (method == 'POST') {
                // Append new row to table
                grid.row.add(newLanding);
                grid.draw();
            }
            else {
                // Update grid row                
                grid.row(currentRow).data(landing).draw();

            }
            // Close modal
            $('#modal-landing').fadeOut(1000);
            $('.modal-close-button').trigger('click');
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

/*
 * Clear Landing Modal
 * @param type 1 - create, 2 - edit
 */
function clearLandingModal() {

    $("#modal-landing-title").attr("style", "");
    $("#modal-landing-board").attr("style", "");
    $("#modal-landing-uri").attr("style", "");

    $("#modal-landing-header").html("Criar landing page");

    $("#modal-landing-content").html("");
    $("#modal-landing-title").val("");
    $("#modal-landing-link").html(""); 
    $("#modal-landing-uri").val("");

    // Esconde o link desta mensagem
    $("#div-modal-landing-link").addClass('hide');

    // Configura o select dos Boards
    initBoardSelector(0);

    // Limpa objeto local com a Landing
    landing.id = 0;
    landing.title = '';
    landing.boardId = 0;
    landing.pageViews = 0;
    landing.leads = 0;
    landing.createdDate = null;
    landing.html = '';
    landing.jsonContent = '';
    landing.thumbnailUrl = '';
    landing.index = 0;
    landing.code = '';
}

function editLanding(data) {

    // Limpa estilos de borda - marcados quando da erro de validação
    $("#modal-landing-title").attr("style", "");
    $("#modal-landing-board").attr("style", "");
    $("#modal-landing-uri").attr("style", "");

    // Salva em local o valor da landing
    landing = data;

    // Modal header
    $("#modal-landing-header").html("Editar landing page");

    // Fill modal fields with data from the row
    $("#modal-landing-content").html(data.content);
    $("#modal-landing-title").val(data.title);
    $("#modal-landing-uri").val(data.redirUri);


    // Init board selector with selected option
    initBoardSelector(data.boardId);

    // URL da landing page,
    if (data.Code) {
        let link = "https://land.azurewebsites.net/" + data.Code;
        let anchor = "<a href='" + link + "' target='_new'>" + link + "</a>";
        $("#modal-landing-link").html(anchor);
    }
    else
        $("#modal-landing-link").html("");

    // Marca no atributo do botão que estamos editando - data-type=2
    $('#modal-landing .btn-save-field').attr('data-type', 2);

}

/*
 * Exclusão
 */
function deleteLanding(id,row) {

    $.ajax({
    url: '/api/Landings/' + id,
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
 * Copy
 */
function copyLanding(id, row) {

    $.ajax({
        url: '/api/Landings/Copy/' + id,
        type: 'GET',
        headers: APIHeader,
        contentType: 'application/json',
        success: function (newLanding) {
            // Append new row to table
            grid.row.add(newLanding);
            grid.draw()
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

/*
 * Resset
 */
function ressetLanding(id, row) {

    $.ajax({
        url: '/api/Landings/Resset/' + id,
        type: 'GET',
        headers: APIHeader,
        contentType: 'application/json',
        success: function (landing) {
            // Update grid row                
            grid.row(currentRow).data(landing).draw();
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
        url: '/api/Landings',
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

});

function populateGrid(data) {

    var renderButtons = `<button class="btn btn-primary btn-resset" title="Resetar as estatísticas" ><i class="fas fa-power-off"></i>
                     <button class="btn btn-info btn-copy" title="Fazer uma cópia Landing Page" ><i class="fa fa-copy"></i>
                     <button class="btn btn-danger btn-delete" title="Excluir landing" ><i class="fa fa-trash"></i>
                     <button class="btn btn-warning btn-edit" data-toggle="modal" data-target="#modal-landing" title="Editar dados"><i class="fa fa-edit"></i></button>
                     <button class="btn btn-success btn-draw" title="Modo de Design"><i class="far fa-file-image"></i></button>`;

    var cols = [
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
            "mData": 'code', "render": function (data, type, row) {
                let link = "land.azurewebsites.net/" + data
                return "<a href='https://" + link + "' target='_new'>" + link + "</a>"
            }
        },
        { "mData": 'pageViews' },
        { "mData": 'leads' },
        {
            data: null, "render": function (data, type, row) {
                if (row.pageViews > 0)
                    return parseFloat(row.leads / row.pageViews * 100).toFixed(2) + "%";
                else
                    return "-";
            }
        },
        {
            data: null,
            defaultContent: renderButtons,
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

    grid = $('#landings-grid').DataTable(tblDef);

    // Bind grid RESSET button event
    $('#landings-grid tbody').on('click', '.btn-resset', function () {
        currentRow = $(this).parents('tr');
        data = grid.row(currentRow).data();
        let id = data.id;
        ressetLanding(id, currentRow);
    });

    // Bind grid COPY button event
    $('#landings-grid tbody').on('click', '.btn-copy', function () {
        currentRow = $(this).parents('tr');
        data = grid.row(currentRow).data();
        let id = data.id;
        copyLanding(id, currentRow);
    });

    // Bind grid DELETE button event
    $('#landings-grid tbody').on('click', '.btn-delete', function () {
        currentRow = $(this).parents('tr');
        data = grid.row(currentRow).data();
        let id = data.id;
        deleteLanding(id, currentRow);
    });

    // Bind grid EDIT button event
    $('#landings-grid tbody').on('click', '.btn-edit', function () {
        currentRow = $(this).parents('tr');
        data = grid.row(currentRow).data();
        editLanding(data);
    });

    // Bind grid DRAW button event
    $('#landings-grid tbody').on('click', '.btn-draw', function () {
        currentRow = $(this).parents('tr');
        data = grid.row(currentRow).data();
        // If page already has some content
        if (data.html)
            // Jump to Edit page
            window.location.href = "/admin/editlanding?landingId=" + data.id;
        else
            // Jump to pick templates page
            window.location.href = "/admin/TemplatesView?landingId=" + data.id;
    });
}

/**
 * Init Board selector
 * 
 */
function initBoardSelector(boardId) {
    let boardSelector = $('#modal-landing-board');
    getAjax('/api/Boards', APIHeader, false, false).then((res) => {
        let strHTML = '<option value="0"></option>';
        res.forEach((board) => {
            strHTML += '<option value="' + board.id + '"' + (board.id == boardId ? ' selected' : '') + '>' + board.name + '</option>';
        });
        boardSelector.html(strHTML);
    });
}

/*
 * Validate Url
 */
function validateUrl(value) {
    return /^(?:(?:(?:https?|ftp):)?\/\/)(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))(?::\d{2,5})?(?:[/?#]\S*)?$/i.test(value);
}
