/*
 * Salva o WebpushID ( id de assinante das notificacoes de webpush ) do agente na base
 */
saveWebPushId();
function saveWebPushId() {
	webpushr('fetch_id', function (sid) {
		if (sid) {
			//save id to database
			var form_data = new FormData();
			form_data.append('webpushId', sid);
			$.ajax({
				url: '/Chat/SaveWebPushId',
				cache: false,
				contentType: false,
				processData: false,
				data: form_data,
				type: 'POST',
				dataType: "text",
				success: function (response) {
					console.log("sid " + sid + " salvo no banco de dados :-)");
				},
				error: function (xhr, textStatus, errorThrown) {
					alert(textStatus + "\n" + errorThrown + "\n" + xhr.responseText);
				}
			});
		}
		else {
			console.log("sid false");
		}
	});
};