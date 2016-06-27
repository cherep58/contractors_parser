var ContragentsController = {
	apiUrl: '/api/contractors/',
	
	/**
	* Функция загружает список контрагентов и отображает их на странице
	*/
	loadAll: function() {
		$.getJSON(this.apiUrl).done(function (contractors) {
			ContragentsController.show(contractors);
		}).fail(function(xhr){
			alert(xhr.responseText);
		});
	},
	
	/**
	* Функция отображает список контрагентов на странице
	*/
	show: function(contractors) {
		var place = $('#contractors_list')[0];
		place.innerHTML = '';
		place.style.display = '';
			
		contractors.forEach(function(contractor) {
			var div = document.createElement('div');
			div.className = 'contractor_short_info';
			div.setAttribute('data-id', contractor.Id);
			$(div).click(function(){
				ContragentsController.loadContragent(this.getAttribute('data-id'));
			});
			
			var p = document.createElement('p');
			p.textContent = contractor.Name;
			div.appendChild(p);
			
			place.appendChild(div);
		});
	},
	
	/**
	* Функция загружает данные контрагента и отображает их в popup-окне
	*/
	loadContragent: function(id) {
		$.getJSON(this.apiUrl + id).done(function (data) {
			ContragentCard.show(data.Name, data.Inn, data.PaymentAccount);
		});
	},
	
	/**
	* Функция загружает файл на сервер, в ответ получает список
	* спарсенных контрагентов и отображает их
	*/
	importFromFile: function(form) {
		var formData = new FormData(form);
		$.ajax({
			url: ContragentsController.apiUrl,
			data: formData,
			cache: false,
			contentType: false,
			processData: false,
			type: 'POST',
			success: function(contractors) {
				$('#imported_contractors_title')[0].style.display = '';
				ContragentsController.show(contractors);
			},
			error: function(xhr) {
			   alert(xhr.responseText);
			}
		});

		return false;
	}
};

var ContragentCard = {
	popup: {
		background: $('#popup_background')[0],
		div: $('#contragent_popup')[0],
		
		KEY_ESCAPE: 27,
		
		show: function() {
			this.background.style.display = '';
			this.div.style.display = '';
			
			$(this.background).click(function(){
				ContragentCard.popup.hide();
			});
			
			$(document).keydown(function(e) {
				if (e.keyCode == ContragentCard.popup.KEY_ESCAPE) {
					ContragentCard.popup.hide();
				}
			});
		},
		
		hide: function() {
			this.background.style.display = 'none';
			this.div.style.display = 'none';
		}
	},
	
	show: function(name, inn, paymentAccount) {
		$('#contragent_name').val(name);
		$('#contragent_inn').val(inn);
		$('#contragent_payment_account').val(paymentAccount);
		
		this.popup.show();
	}
};