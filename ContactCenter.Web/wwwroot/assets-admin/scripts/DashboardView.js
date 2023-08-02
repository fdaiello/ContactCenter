/**
 * This is script for Admin / DashboardView
 * 
 * @author Felipe Daiello
 */

// Token previosly saved to localstorage
var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};

//To get the grid to use application assigned IDs
function getRowNodeId(data) {
    return data.id;
}

// Custom cell renderer for exposing the Agent file picture
function imgCellRenderer(params) {
    if (params.data.pictureFile != "") {
        return '<img src="' + params.data.pictureFile + '" class="rounded-circle">';
    }
    else {
        return getIconByName(params.data.fullName);
    }
}

// Column definition we are going to use at the grid
var columnDefs = [
    { headerName: "", field: "pictureFile", cellRenderer: 'imgCellRenderer', width: 40, cellStyle: { padding: '0' } },
    { headerName: "", field: "fullName", width: 230 },
    { headerName: "Enviadas", field: "msg_env", width: 105, type: 'rightAligned' },
    { headerName: "Recebidas", field: "msg_rec", width: 105, type: 'rightAligned' },
    { headerName: "Contatos", field: "contacts", width: 105, type: 'rightAligned' },
    { headerName: "Cartoes", field: "cards", width: 105, type: 'rightAligned' }
];

// Grid Options
var gridOptions

// Fetch data from Api
function fechData() {
    $.ajax({
        url: '/api/Dashboard/Agents?dateStart=' + $('#filter_fdate').val() + '&dateEnd=' + $('#filter_tdate').val(),
        type: 'GET',
        headers: APIHeader,
        success: function (dashAgents) {
            // Define the data for the Grid
            var rowData = dashAgents;

            // let the grid know which columns and what data to use
            gridOptions = {
                columnDefs: columnDefs,
                rowData: rowData,
                components: {
                    imgCellRenderer: imgCellRenderer
                },
            };

            // setup the grid 
            var gridDiv = document.querySelector('#DashboardAgents');
            new agGrid.Grid(gridDiv, gridOptions);

        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })
}

/**
 * generate icon bt user name
 * */
function getIconByName(name, status, unAnsweredCount) {
    if (name == "" | name == "?") {
        icon_name = "?";
        icon_color = "dark";
    }
    else {
        var arr = name.split(" ");//ALT
        var color_arr = (new String("PRIMARY SECONDARY SUCCESS INFO WARNING DANGER FOCUS DARK")).toLowerCase().split(" ");
        var first_letter = arr[0].charAt(0).toUpperCase();
        var icon_name = first_letter + (arr.length > 1 ? arr[1].charAt(0) : "").toUpperCase();
        var icon_color = color_arr[(first_letter.charCodeAt(0) - 65) % color_arr.length];
    }

    var html = "<div class=\"badge badge-pill badge-" + icon_color + " left-panel-generated-icon\" style=\"width: 34px; height: 34px; padding-top: 12px;\">" + icon_name;

    // Se o status == 2 ( Aguardando ) insere div com css para circulo piscante
    if (status == "2")
        html += "<div role=\"presentation\" tabindex=\"-1\" title=\"" + name + "\" aria-hidden=\"true\" class=\"waiting-flashing-icon\"></div>";
    // Se tem mensagens nao respondidas, insere div com numero
    else if (unAnsweredCount > 0)
        html += "<div role=\"presentation\" tabindex=\"-1\" title=\"" + name + "\" aria-hidden=\"true\" class=\"unAnsweredCount-icon\">" + unAnsweredCount + "</div>";

    html += "</div>";
    return html;
}

function onBtnExport() {
    gridOptions.api.exportDataAsCsv();
}

/*
 * Inicializa a grid dos atendentes
 */
fechData();

