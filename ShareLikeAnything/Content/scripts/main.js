$("body").ready(function () {
	$('body').bind('paste', function (e) {
		var items = e.originalEvent.clipboardData.items;
		var blob = items[0].getAsFile();
		uploadFile(blob, e);
	});

	$('body').bind('drop', function (e) {
		e.stopPropagation();
		e.preventDefault();

		var files = e.originalEvent.dataTransfer.files;
		var count = files.length;

		// Only call the handler if 1 or more files was dropped.
		if (count > 0)
			uploadFile(files[0], e);
	});

	$('body').bind('dragEnter', noopHandler);

	$('body').bind('dragexit', noopHandler);

	$('body').bind('dragover', noopHandler);

	function uploadFile(blob, e) {
		showLoading();
		if (blob == null)
			blob = e.originalEvent.clipboardData.getData('text/plain');

		var oMyForm = new FormData();
		// HTML file input user's choice...
		oMyForm.append("data", blob);

		var oReq = new XMLHttpRequest();
		oReq.onreadystatechange = function (data) {
			if (oReq.readyState == 4) {
				if (oReq.status == 200) {
					stopLoading();
					$("#shareLink").html(document.domain + "/view/" + data.currentTarget.responseText);
					$('#myModal').modal('show');
				}
			}
		}
		oReq.open("POST", "/fileUpload");
		oReq.send(oMyForm);
	}

	$("#goToLink").click(function (e) {
		window.location = $("#shareLink").html();
	});

	function noopHandler(evt) {
		evt.stopPropagation();
		evt.preventDefault();
	}

	function showLoading() {
		var docHeight = $(document).height();

		$("body").append("<div id='overlay'></div>");

		$("#overlay")
				.height(docHeight)
				.css({
					'opacity': 0.4,
					'position': 'absolute',
					'top': 0,
					'left': 0,
					'background-color': 'black',
					'width': '100%',
					'z-index': 5000
				});
	}

	function stopLoading() {
		$("#overlay").remove();
	}
});
