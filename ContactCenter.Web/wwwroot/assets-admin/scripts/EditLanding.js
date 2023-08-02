/* 
 * Scripts do Editor de Landing Pages
 * 
 */

var templateId;
var projectId = 15648;
var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};
var landing = {
    id: 0,
    title: '',
    createdDate: ''
};

// Checks for landingId parameter at URL
var landingId = getUrlParameter('landingId');

/*
 * Ao carregar o documento, 
 */
$(document).ready(function () {

    // Bind SAVE button event
    $('#edit-landing-header').on('click', '#btnSave', function () {
        SaveLanding();
    });

    // Bind CLOSE button event
    $('#edit-landing-header').on('click', '#btnClose', function () {
        window.location.href = "/admin/LandingsView";
    });

    // Get landingId from URL
    landingId = getUrlParameter("landingId");

    // Get templateId from URL
    templateId = getUrlParameter("templateId");

    if (!templateId && !landingId) {
        alert("URL inválida!");
        window.location.href = "/admin/LandingsView";

    }
    else {
        // Initialize Editor
        unlayer.init({
            id: 'editor',
            projectId: projectId,
            locale: 'pt-BR',
            displayMode: 'web',
            appearance: {
                theme: 'light',
                panels: {
                    tools: {
                        dock: 'left'
                    }
                }
            },
            tools: {
                form: {
                    properties: {
                        action: {
                            editor: {
                                data: {
                                    actions: [
                                        {
                                            method: 'POST'
                                        }
                                    ]
                                }
                            }
                        }
                    }
                }
            }
        })

        if (landingId) {
            // Load landing
            LoadLanding(landingId);
        }
        if (templateId) {
            // Load template
            LoadTemplate(templateId);
        }

    }
});

/*
 * Save Landing
 */
function SaveLanding() {

    unlayer.exportHtml(function (data) {

        landing.jsonContent = JSON.stringify(data.design);
        landing.html = data.html;

        // Se tem Id de mensagem - estamos editando uma mensagem existente
        if (landingId) {
            // PUT landing to the API
            $.ajax({
                url: '/api/Landings/' + landingId,
                contentType: 'application/json',
                headers: APIHeader,
                data: JSON.stringify(landing),
                method: 'PUT',
                success: function () {
                    alert("Landing salva!");
                },
                error: function (xhr, textStatus, errorThrown) {
                    alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
                }
            })
        }
        // Criando nova mensagem a partir de template
        else {
            // Zera o Id - tinha o Id do template
            landing.id = 0;
            // POST new landing to the API
            $.ajax({
                url: '/api/Landings',
                contentType: 'application/json',
                headers: APIHeader,
                data: JSON.stringify(landing),
                method: 'POST',
                success: function (data) {
                    alert("Nova landing criada!");
                    landingId = data.id;
                    landing.Id = data.id;
                },
                error: function (xhr, textStatus, errorThrown) {
                    alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
                }
            })

        } 

    });

}

/*
 * Load Landing
 */
function LoadLanding(landingId) {

    // Get data from API
    $.ajax({
        url: '/api/Landings/' + landingId,
        headers: APIHeader,
        type: 'GET',
        success: function (data) {
            landing = data;
            if (data.jsonContent) {
                unlayer.loadDesign(JSON.parse(data.jsonContent));
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })

}

/*
 * Load Template
 */
function LoadTemplate(templateId) {

    // Get data from API
    $.ajax({
        url: '/api/Messages/Templates/' + templateId,
        headers: APIHeader,
        type: 'GET',
        success: function (data) {
            if (data.content) {
                unlayer.loadDesign(JSON.parse(data.content));
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })

}
