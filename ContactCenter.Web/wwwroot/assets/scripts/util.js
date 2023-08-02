//função usada para os alertas de sucesso ou erro em requisições ajax
/*
    * mensagem - mensagem que será exibida;
    * tipo - se a mensagem é de 'sucesso'(.alerta-sucesso), 'erro'(.alerta-erro) e etc;
    * velocidadeFadeIn - a velocidade da animação de fadeIn para mostrar ou sumir a mensagem;
    * tempoExibição - tempo em que a animação ficará na tela
*/
function statusAlert(mensagem, tipo, velocidadeFadeIn, tempoExibicao) {
    //Template para alertas de sucesso ou erro para as funcionalidade.
    var template = `<div class="alerta">
        <h6 class="alerta-mensagem">${mensagem}</h6>
    </div>`;
    //insere o template no corpo da página
    $("main").append(template);
    //Adiciona o alerta com efeito de fadeIn
    $(".alerta").fadeIn(velocidadeFadeIn, () => $(this).addClass(tipo)); //adiciona a classe que mostra na tela a mensagem
    //Retira a mensagem no tempo determinado em tempoExibicao
    setTimeout(() => {
        //Animação para mostrar a mensagem suavemente
        $(".alerta").fadeOut(velocidadeFadeIn, () => $(this).removeClass(".alerta")); //adiciona a classe que mostra na tela a mensagem
    }, tempoExibicao);
}

//adiciona um spinner de carregamento
function startSpinner() {
    //Spinner que indica que determinada função está sendo processada!
    var template = `<div class="spinner-processamento spinner-start">
        <div class="spinner-border text-info">
            <span class="visually-hidden"></span>
        </div>
    </div>`;
    //insere o spinner no corpo da página
    $("main").append(template);
}

//é retirado a tela de processamento liberando a tela para novos eventos
function stopSpinner(){ $(".spinner-processamento").removeClass("spinner-start") }

//Funções utilitárias ajax
/*
* Função que implementa a função get de http request, dispara startSpinner até a conclusão da requisição,
* stopSpinner e uma mensagem no callback de sucesso ou de erro.
* url - Rota que disponibiliza o recurso solicitado
* headers - Cabeçalho da requisição
* customMsg - Mensage exibida na conclusão da requisição
* msg - Se o valor for false nenhuma mensagem será exibida
*/
function getAjax(url, headers, spinner = true, msg = true, customMsg = "Dado(s) obtido(s) com sucesso!"){
    return $.ajax({
        url: url, headers: headers, type: 'GET',
        beforeSend: () => {
            if(spinner == true) {
                startSpinner();
            }
        },
        success: () => {
            if(msg == true) statusAlert(customMsg, "alerta-sucesso", "slow", 4000);
            if(spinner == true) {
                stopSpinner();
            }
        },
        error: (xhr, textStatus, errorThrown) => {
            if(spinner == true) {
                stopSpinner();
            }
            alert(textStatus + "\n" + errorThrown + "\n" + (xhr.responseText ? xhr.responseText : "Erro de conexão com a internet."));
        }
    });
}

/*
* Função que implementa a função put de http request, dispara startSpinner até a conclusão da requisição,
* stopSpinner e uma mensagem no callback de sucesso ou de erro.
* url - Rota que disponibiliza o recurso solicitado
* headers - Cabeçalho da requisição
* data - Dados enviados para o servidor
* customMsg - Mensage exibida na conclusão da requisição
* msg - Se o valor for false nenhuma mensagem será exibida
*/
function putAjax(url, headers, data, cache = true, processData = true, contentType = "application/json", msg = true, customMsg = "Dado(s) atualizado(s) com sucesso!"){
    return $.ajax({
        url: url, headers: headers, type: 'PUT', data: data, cache: cache, processData: processData, contentType: contentType,
        beforeSend: () => startSpinner(),
        success: () => {
            if(msg == true) statusAlert(customMsg, "alerta-sucesso", "slow", 4000);
            stopSpinner();
        },
        error: (xhr, textStatus, errorThrown) => {
            stopSpinner();
            console.log('Put Ajax: error');
            console.log(xhr);
            console.log(textStatus);
            console.log(errorThrown);
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

/*
* Função que implementa a função delete de http request, dispara startSpinner até a conclusão da requisição,
* stopSpinner e uma mensagem no callback de sucesso ou de erro.
* url - Rota que disponibiliza o recurso solicitado
* headers - Cabeçalho da requisição
* customMsg - Mensage exibida na conclusão da requisição
* msg - Se o valor for false nenhuma mensagem será exibida
*/
function deleteAjax(url, headers, msg = true, customMsg = "Dado(s) excluido(s) com sucesso!"){
    return $.ajax({
        url: url, headers: headers, type: 'DELETE',
        beforeSend: () => startSpinner(),
        success: () => {
            if(msg == true) statusAlert(customMsg, "alerta-sucesso", "slow", 4000);
            stopSpinner();
        },
        error: (xhr, textStatus, errorThrown) => {
            stopSpinner();
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

/*
* Função que implementa a função post de http request, dispara startSpinner até a conclusão da requisição,
* stopSpinner e uma mensagem no callback de sucesso ou de erro.
* url - Rota que disponibiliza o recurso solicitado
* headers - Cabeçalho da requisição
* data - Dados enviados para o servidor
* customMsg - Mensage exibida na conclusão da requisição
* msg - Se o valor for false nenhuma mensagem será exibida
*/
function postAjax(url, headers, data, spinner = true, cache = true, processData = true, contentType = "application/json", msg = true, customMsg = "Dado(s) salvo(s) com sucesso!"){
    return $.ajax({
        url: url, headers: headers, type: 'POST', data: data, cache: cache, processData: processData, contentType: contentType,
        beforeSend: () => {
            if(spinner == true) startSpinner();
        },
        success: () => {
            if(msg == true) statusAlert(customMsg, "alerta-sucesso", "slow", 4000);
            if(spinner == true) stopSpinner();
        },
        error: (xhr, textStatus, errorThrown) => {
            if(spinner == true) stopSpinner();
            alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
        }
    });
}

//Converte o formato da data
function dateConverter(d) {
    var d = d.split('T');
    d = d[0].split('-');
    var day = d[2];
    var month = d[1];
    date = day + "/" + month;
    return date;
}

$(document).ready(function(){
    /* Loop through all dropdown buttons to toggle between hiding and showing its dropdown content - This allows the user to have multiple dropdowns without any conflict */
    var dropdown = $(".dropdown-btn");
    var i;
    for(i = 0; i < dropdown.length; i++){
        dropdown[i].addEventListener("click", function(){
            this.classList.toggle("active");
            var dropdownContent = this.nextElementSibling;
            if(dropdownContent.style.display === "block"){
                dropdownContent.style.display = "none";
            }else{
                dropdownContent.style.display = "block";
            }
        });
    }

    // Menu Mobile
    $(".mobile-toggle-header-nav").click(function () {
        $(".app-header__content").toggleClass("header-mobile-open");
    });
});

//Função que busca o valor de váriaveis de url (get)
function getUrlParameter(sParam) {
    if (window.location.search) {
        var sPageURL = decodeURI(unescape(window.location)).split("?")[1];
        var sURLVariables = sPageURL.split('&'),
            sParameterName,
            i;
        for (i = 0; i < sURLVariables.length; i++) {
            sParameterName = sURLVariables[i].split('=');
            if (sParameterName[0] === sParam) {
                return sParameterName[1] === undefined ? true : sParameterName[1];
            }
        }
    }
};