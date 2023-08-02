/*
 * Script para Admin - CRM - imports
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
$(document).on('click', '#modal-import .btn-save-field', function () {

    // Volta as molduras dos campos ao padrão normal - podem ter sido enfatizadas se os campos obrigatórios não foram preenchidos
    $("#modal-import-board").attr("style", "1px solid #ced4da");
    $("#modal-import-newlistname").attr("style", "1px solid #ced4da");

    var boardId = $("#modal-import-board").val();
    var newlistname = $("#modal-import-newlistname").val();
    var filename = $("#modal-import-filename").val();
    var uniquefilename = $("#modal-import-uniquefilename").val();
    var countTotal = $('#modal-import-counter').val();
     
    // Validação dos campos obrigatorios
    if (boardId == '-1') {
        $("#modal-import-board").attr("style", "border-color:red;");
        alert('Escolha uma lista!');
        $("#modal-import-board").focus();
        return false;
    }
    else if (boardId == '0' && !newlistname) {
        $("#modal-import-newlistname").attr("style", "border-color:red;");
        alert('Informe o nome da lista!');
        $("#modal-import-newlistname").focus();
        return false;
    }
    else if (!filename) {
        alert("Selecione o arquivo a ser importado!");
        return false;
    }

    // Cria um objeto IMPORT para ser salvo
    let newImport = {
        id: 0,
        status: 0,
        boardid: boardId,
        newlistname: newlistname,
        fileName: filename,
        uniqueFileName: uniquefilename,
        msgerro: '',
        countTotal: countTotal ?? 0,
        countErrors: 0,
        countImported: 0
    };

    SaveImport(newImport)
}); 

/*
 * Salva a importação
 */
function SaveImport(newImport) {

    var method;
    var url = '/api/Imports';
    if (newImport.id != 0) {
        method = 'PUT';
        url += '/' + newImport.id;
    }
    else
        method = 'POST'

    startSpinner();

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        headers: APIHeader,
        data: JSON.stringify(newImport),
        success: function (newImportSaved) {
            if (method == 'POST') {
                // Append new row to table
                grid.row.add(newImportSaved); 
                grid.draw();
            }
            else {
                // Update import descriptors data
                newImportSaved.board = { name: $("#modal-import-board option:selected").text() };

                // Update grid row                
                grid.row(currentRow).data(newImportSaved).draw();
                grid.draw();
            }
            // Close modal
            $('#modal-import').fadeOut(1000);
            $('.modal-close-button').trigger('click');

            stopSpinner();

            // Recarrega a Grid depois de 3 segundos
            setTimeout(() => {
                reloadGridData();
            }, 3000);

        },
        error: function (xhr, textStatus, errorThrown) {
            stopSpinner;
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

/*
 * Clear Import Modal
 * @param type 1 - create, 2 - edit
 */
function clearImportModal() {

    // Limpa os campos
    $("#modal-import-board").val("");
    $("#modal-import-newlistname").val("");
    $('#modal-import-uniquefilename').val("");
    $('#modal-import-filename').val("");
    $('#modal-import-counter').val('');
    $('#modal-import-counter-div').addClass('hide');
    $('#attached-file').val('');

    // Configura o select dos Boards
    initBoardSelector(0);

    // Habilita o botão salvar
    $(".btn-save-field").prop('disabled', false)

}

function editimport(data) {

    // Fill modal fields with data from the row
    $("#modal-import-newlistname").val(data.newlistname);

    // Init selectors with selected option
    initBoardSelector(data.boardId);

    // Se já foi processado ou estiver cancelado, impede editar
    $(".btn-save-field").prop('disabled', data.status != 0)

    // Bind função do evento de tratamento do arquivo enviado
    bindAttachedFileChange()
}


/**
 * Init Board selector
 * 
 */
function initBoardSelector(boardId) {
    let boardSelector = $('#modal-import-board');
    getAjax('/api/Boards', APIHeader, false, false).then((res) => {
        let strHTML = '<option value="-1"></option><option value="0">---criar nova lista---</option>';
        res.forEach((board) => {
            strHTML += '<option value="' + board.id + '"' + (board.id == boardId ? ' selected' : '') + '>' + board.name + '</option>';
        });
        boardSelector.html(strHTML);
    });
}

/*
 * Ao carregar o documento, inicializa a Grid
 */
$(document).ready(function () {

    startSpinner();

    // Get data from API
    $.ajax({
        url: '/api/Imports',
        headers: APIHeader,
        type: 'GET',
        success: function (data) {
            populateGrid(data);
            stopSpinner();
            // Se tem alguma linha com status importando
            if (hasImportingRow(data))
                // Recarrega a Grid depois de 3 segundos
                setTimeout(() => {
                    reloadGridData();
                }, 3000);
        },
        error: function (xhr, textStatus, errorThrown) {
            stopSpinner();
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })

});

/*
 * Chama a API para ler os dados da Grid
 * Chama a rotina que preenche a Grid com os dados
 */
function reloadGridData() {
    // Get data from API
    $.ajax({
        url: '/api/Imports',
        headers: APIHeader,
        type: 'GET',
        success: function (data) {
            let datatable = $('#imports-grid').DataTable();
            datatable.clear();
            datatable.rows.add(data);
            datatable.draw();
            // Se tem alguma linha com status importando
            if (hasImportingRow(data))
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
 * Confere se tem alguma linha com status importando
 */
function hasImportingRow(data) {
    let has = false;
    data.forEach(function(row) {
        if (row.status == 0 || row.status==1)
            has=true;
    })
    return has;
}

/*
 * Preenche a grid com os dados
 */
function populateGrid(data) {

    renderButtons = '';

    var cols = [
        {
            "mData": 'status', "render": function (data, type, row) {
                let status = '';
                if ( data != 1 )
                    status = `<span style='color:${data == 0 ? 'yellow' : data == 1 ? 'green' : data == 2 ? 'blue' : data == 3 ? 'orange' : data == 4 ? 'red' : 'black'}'><i class="fas fa-circle"></i></span> `;
                status = status + (data == 0 ? 'na fila' : data == 1 ? '<progress value="' + row.progressPercent +'" max="100"></progress>' : data == 2 ? 'importado' : data == 3 ? 'cancelado' : data == 4 ? 'erro' : 'undefined')
                return status;
            }
        },
        {
            "mData": 'importDate', "render": function (data, type, row) {
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

        { "mData": 'board.name' },
        { "mData": 'fileName' },
        { "mData": 'countTotal' },
        { "mData": 'countImported' },
        {
            data: null,
            defaultContent: renderButtons,
            orderable: false
        }];

    var tblDef = {
        data: data,
        searching: false,
        bJQueryUI: true,
        sPaginationType: 'full_numbers',
        aaSorting: [[2, 'asc']],
        aoColumns: cols,
        dom: 'Bfrtip',
        order: [[1, "desc"]],
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

    grid = $('#imports-grid').DataTable(tblDef);

    // Bind EDIT button event
    $('#imports-grid tbody').on('click', '.edit-icon-custom', function () {
        currentRow = $(this).parents('tr');
        var data = grid.row(currentRow).data();
        editimport(data);
    });

    // Bind board select change event
    $(document).on('change', '#modal-import-board', function () {
        let boardId = $(this).val();
        // Se escolheu a opção "Nova Lista"
        if (boardId == "0") {
            // mostra input para digitar o nome da lista
            $('#modal-import-newlistname-div').removeClass('hide');
        }
        else {
            $('#modal-import-newlistname-div').addClass('hide');
        }
    });

    // Bind função do evento de tratamento do arquivo enviado
    bindAttachedFileChange()
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

        // Salva o nome do arquivo original, no campo visivel
        $("#modal-import-filename").val($(this).prop('files')[0].name);

        // Form data para salvar o arquivo via POST
        var formData = new FormData();
        formData.append('file', $(this).prop('files')[0]);

        // Le o conteudo do arquivo enviado
        var reader = new FileReader();
        reader.onload = event => {
            content = event.target.result;
            // allTextLines array: separa o conteudo por quebra de linha
            var allTextLines = content.split(/\r\n|\n/); 
            // Se tem linha em branco no final
            if (!allTextLines[allTextLines.length - 1])
                // Remove a ulima linha ( para contar corretamente )
                allTextLines.length = allTextLines.length - 1;
            // Header array: pega a primeira linha, e separa por vírgula ou ponto e vírgula
            var header = allTextLines[0].toLowerCase().split(/[,;]+/);
            if (header.includes('celular') || header.includes('"celular"')) {
                //Salva o arquivo
                postAjax('/api/Files', APIHeader, formData, true, false, false, false, false)
                    .then(
                        (res) => {
                            // Salva o nome do arquivo recebido, no campo escondido do formulário
                            $("#modal-import-uniquefilename").val(res.fileName);

                            // Exibe o contador com a quantidade de linhas do arquivo
                            $('#modal-import-counter').val(allTextLines.length-1);
                            $('#modal-import-counter-div').removeClass('hide');
                        }
                    );
            }
            else {
                // Limpa os campos com o nome dos arquivos
                $("#modal-import-filename").val('');
                $("#modal-import-uniquefilename").val('');

                // Alerta o usuário
                alert('O arquivo precisa conter um cabeçalho na primeira linha com o nome dos campos, e ter pelo menos um campo com nome CELULAR.')
            }
        }
        reader.onerror = error => {
            alert(error);
        }
        reader.readAsText($(this).prop('files')[0]);

    });

}