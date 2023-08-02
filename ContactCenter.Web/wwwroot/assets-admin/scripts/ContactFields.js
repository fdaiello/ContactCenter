/**
 * This is script for Admin / Contact Field View
 * 
 * @author Felipe Daiello
 */

//To get the grid to use application assigned IDs
function getRowNodeId(data) {
    return data.id;
}

// Token previosly saved to localstorage
var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};

// Column definition we are going to use at the grid
var columnDefs = [
    { headerName: "Campo", field: "field.label", checkboxSelection: true, rowDrag: true }
];

// Grid Options
var gridOptions

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
        success: function (contactFields) {
            //é retirado a tela de processamento liberando a tela para novos eventos
            stopSpinner();
            // Define the data for the Grid
            var rowData = contactFields;

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
            var gridDiv = document.querySelector('#ContactFieldGrid');
            new agGrid.Grid(gridDiv, gridOptions);

            // par todos os elementos
            var x = 0;
            contactFields.forEach((contactField) => {
                node = gridOptions.api.getRowNode(x);
                node.setSelected(contactField.enabled);
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

    // Call BackEnd to resort
    $.ajax({
        url: '/api/ContactFields/Sort?movedId=' + movedId + '&overIndex=' + overIndex,
        type: 'PUT',
        headers: APIHeader,
        success: function (res) {

            //Insere a mensagem de aviso de sucesso no alert customizado
            $(".aviso-mensagem").html("Dados alterados com sucesso!");

            //Animação para mostrar a mensagem suavemente
            $(".aviso").fadeIn("slow", function () {
                //adiciona a classe que mostra na tela a mensagem
                $(this).addClass("aviso-sucesso");
            });

            //Retira a mensagem suavemente depois de 3 segundos
            setTimeout(function () {
                $(".aviso").fadeOut("slow", function () {
                    $(this).removeClass("aviso-sucesso");
                });
            }, 3000);

        },
        error: function (xhr, textStatus, errorThrown) {
            //é retirado a tela de processamento liberando a tela para novos eventos
            stopSpinner();
            //alerta de erro
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

//função que salva os checkboxes somente depois que o datagrid é carregado
function changeOfFields() {
    $(".ag-checkbox-input").on('change', function () {
        saveSelectedRows();
    });
}

//executa a função que popula o datagrid depois do carregamento total da página
$(document).ready(function () {
    carregaDados('/api/ContactFields/JoinFields');
});

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
    update('/api/ContactFields/JoinFields', rowData);
}



