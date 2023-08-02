/**
 * This is script for Admin / Board Field View
 * 
 * @author Daniel
 */
var currentBoardId;

//To get the grid to use application assigned IDs
function getRowNodeId(data) {
    return data.id;
}

var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};

/**
 * Init Fields Table 
 * */
function initView() {
    $.ajax({
        url: '/api/Boards',
        type: 'GET',
        headers: APIHeader,
        success: function (boards) {
            allBoards = boards;

            let boardOptions = '';
            boards.forEach((board) => {
                boardOptions += '<option value="' + board.id + '">' + board.name + '</option>';
            });
            $('#board_selector').html(boardOptions);

            currentBoardId = boards[0].id
            carregaDados('/api/BoardFields?boardId=' + currentBoardId);
        }
    })
}

$(document).ready(function () {
    initView();
});

// Column definition we are going to use at the grid
var columnDefs = [
    { headerName: "Campo", field: "field.label", checkboxSelection: true, rowDrag: true }
];

// Grid Options
var gridOptions;

// Fetch data from Api
function carregaDados(url) {
    $.ajax({
        url: url,
        type: 'GET',
        headers: APIHeader,
        //Função jquery executada enquanto função Ajax está processando
        beforeSend: function () {
            //Adiciona tela de processamento que bloqueia qualquer outro evento
            startSpinner();
        },
        success: function (boardFields) {
            //console.log("boardFields", boardFields);
            //é retirado a tela de processamento liberando a tela para novos eventos
            stopSpinner();
            // Define the data for the Grid
            var rowData = boardFields;

            // let the grid know which columns and what data to use
            gridOptions = {
                columnDefs: columnDefs,
                rowSelection: 'multiple',
                rowData: rowData,
                defaultColDef: {
                    width: 487,
                    sortable: true,
                    filter: true,
                },
                rowDragManaged: true,
                animateRows: true,
                onRowDragEnd: onRowDragEnd,
                onGridReady: function (event) { changeOfFields(); },
            };

            // setup the grid 
            var gridDiv = document.querySelector('#BoardFieldGrid');
            //Remove datagrid anterior
            $(".ag-layout-normal").remove();
            new agGrid.Grid(gridDiv, gridOptions);

            // par todos os elementos
            var x = 0;
            boardFields.forEach((boardFields) => {
                node = gridOptions.api.getRowNode(x);
                node.setSelected(boardFields.enabled);
                x++;
            });

        },
        error: function (xhr, textStatus, errorThrown) {
            //é retirado a tela de processamento liberando a tela para novos eventos
            stopSpinner();
            //alerta de erro
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

// Evento dispardo quando é feito Drag em Drop na Grid, para reordenar.
// Chama API que faz a ordenção "Server Side"
// by Felipe Daiello - 10-12-2020
function onRowDragEnd(e) {

    // Id do objeto que foi movido
    var movedId = e.overNode.data.id;

    // Posição na grid para onde ele foi movido
    var overIndex = e.overIndex;

    //Obtem o boardId do select
    var boardSelected = $("#board_selector").val();

    // Call BackEnd to resort
    $.ajax({
        url: '/api/BoardFields/Sort?boardId=' + boardSelected + '&movedId=' + movedId + '&overIndex=' + overIndex,
        type: 'PUT',
        headers: APIHeader,
        //Função jquery executada enquanto função Ajax está processando
        beforeSend: function () {
            //Adiciona tela de processamento que bloqueia qualquer outro evento
            startSpinner();
        },
        success: function (res) {
            //é retirado a tela de processamento liberando a tela para novos eventos
            stopSpinner();
            //alerta de sucesso
            statusAlert("Dados alterados com sucesso!", "alerta-sucesso", "slow", 3000);

        },
        error: function (xhr, textStatus, errorThrown) {
            //é retirado a tela de processamento liberando a tela para novos eventos
            stopSpinner();
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

function update(url, data) {
    $.ajax({
        url: url,
        type: 'PUT',
        headers: APIHeader,
        data: JSON.stringify(data),
        contentType: 'application/json',
        //Função jquery executada enquanto função Ajax está processando
        beforeSend: function () {
            //Adiciona tela de processamento que bloqueia qualquer outro evento
            startSpinner();
        },
        success: function (data) {
            //é retirado a tela de processamento liberando a tela para novos eventos
            stopSpinner();
            //alerta de sucesso
            statusAlert("Dados alterados com sucesso!", "alerta-sucesso", "slow", 3000);
        },
        error: function (xhr, textStatus, errorThrown) {
            // Carrega novamente os dados - para voltar ao estado anterior
            carregaDados('/api/BoardFields?boardId=' + currentBoardId);
            //é retirado a tela de processamento liberando a tela para novos eventos
            stopSpinner();
            //alerta de erro
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

// Save selected rows
function saveSelectedRows() {
    var selectedNodes = gridOptions.api.getSelectedNodes();
    var rowData = gridOptions.rowData;
    rowData.map(function (node) {
        node.enabled = false;
    });
    var selectedData = selectedNodes.map(function (node) { return node.data });
    selectedData.map(function (node) {
        node.enabled = true;
    });
    var boardSelected = $("#board_selector").val();
    update('/api/BoardFields?boardId=' + boardSelected, rowData);
}

//função que salva os checkboxes somente depois que o datagrid é carregado
function changeOfFields() {
    $(".ag-checkbox-input").on('change', function () {
        saveSelectedRows();
    });
}

//executa a função que popula o datagrid na seleção do board
$("#board_selector").on("change", function () {
    currentBoardId = $(this).val();
    carregaDados('/api/BoardFields?boardId=' + currentBoardId);
});