/**
 * This is script for Admin / ContactsBySource
 * 
 * @author Felipe Daiello
 */

// Token previosly saved to localstorage
var APIHeader1 = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};

// Column definition we are going to use at the grid
var columnDefs1 = [

    { headerName: "Canal", field: "channelName" },
    { headerName: "Número", field: "channelNumber"},
    { headerName: "Origem", field: "sourceDescription" },
    { headerName: "Qtd", field: "qtd", width: 105, type: 'rightAligned' }
];

// Grid Options
var gridOptions1

// Fetch data from Api
function fechData1() {
    $.ajax({
        url: '/api/Dashboard/ContactsBySource?dateStart=' + $('#filter_fdate').val() + '&dateEnd=' + $('#filter_tdate').val(),
        type: 'GET',
        headers: APIHeader1,
        success: function (contactsBySource) {
            // Define the data for the Grid
            var rowData = contactsBySource;

            // let the grid know which columns and what data to use
            gridOptions1 = {
                columnDefs: columnDefs1,
                rowData: rowData
            };

            // setup the grid 
            var gridDiv1 = document.querySelector('#DashboardContactsBySource');
            new agGrid.Grid(gridDiv1, gridOptions1);

        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })
}

function onBtnExport1() {
    gridOptions1.api.exportDataAsCsv();
}

/*
 * Inicializa a grid dos atendentes
 */
fechData1();
