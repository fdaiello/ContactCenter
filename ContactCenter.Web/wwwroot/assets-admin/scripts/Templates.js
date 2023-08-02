/* 
 * Script para produzir a galeria de templates
 * PlugIn - http://unitegallery.net/
 */

var APIHeader = {
    Authorization: 'Bearer ' + localStorage.getItem('token')
};

// Checks for landingId parameter at URL
var landingId = getUrlParameter('landingId');
var messageId = getUrlParameter('messageId');

// Ao carregar o documento
jQuery(document).ready(function () {

    // Spinner
    startSpinner();

    // Get data from API
    $.ajax({
        url: '/api/Messages/Templates',
        headers: APIHeader,
        type: 'GET',
        success: function (data) {
            // Preenche a galeria de templates
            populateGallery(data);
            // Stop spinner
            stopSpinner();
        },
        error: function (xhr, textStatus, errorThrown) {
            // Stop spinner
            stopSpinner();
            // Alerta erro
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    })

}); 

/*
 *  Gera a galeria com as miniaturas dos templates
 */
function populateGallery(data) {

    // Monta o Html com as imagens dos templates
    var html="";
    var i;
    for (i = 0; i < data.length; i++) {
        if (landingId) {
            html += `<a href="/admin/EditLanding?templateId=${data[i].id}&landingId=${landingId}">
                    <img alt="${data[i].title}" src="${data[i].thumbnail}"
				    data-image="${data[i].thumbnail}"
                    data-description="${data[i].title}"></a>`;
        }
        else {
            html += `<a href="/admin/EditMailMessage?templateId=${data[i].id}&messageId=${messageId}">
                    <img alt="${data[i].title}" src="${data[i].thumbnail}"
				    data-image="${data[i].thumbnail}"
                    data-description="${data[i].title}"></a>`;
        }
    }
    
    // Carrega a Div da galeria 
    $("#gallery").html(html);

	// Aciona o script da galeria
    jQuery("#gallery").unitegallery({
        tile_show_link_icon: true,
        tile_width: 256,
        tile_height: 565,
        tile_enable_shadow: true,
        tile_link_newpage: false,
	    tile_enable_textpanel: true,
        tile_textpanel_title_text_align: "center",
    });

};