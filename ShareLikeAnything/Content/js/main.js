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
		var isValid = true;
		var msgError = "";
		if (blob == null) {
			blob = e.originalEvent.clipboardData.getData('text/plain');
			if (lengthInUtf8Bytes(blob) > 1000000) {
				isValid = false;
				msgError = "The text is bigger than 100,000 bytes";
			}
		} else {
			if (blob.size > 5000000) {
				isValid = false;
				msgError = "The size of the file is bigger than 5,000,000 bytes";
			}
		}

		if (!isValid) {
			stopLoading();
			$("#errorMessage").html(msgError);
			$("#alertDialog").removeClass("hidden");
			return;
		}



		var oMyForm = new FormData();
		// HTML file input user's choice...
		oMyForm.append("data", blob);

		var oReq = new XMLHttpRequest();
		oReq.onreadystatechange = function (data) {
			if (oReq.readyState == 4) {
				if (oReq.status == 200) {
					stopLoading();
					$("#shareLink").html("http://" + document.domain + "/view/" + data.currentTarget.responseText);
					$('#myModal').modal('show');
				}
			}
		}
		oReq.open("POST", "/fileUpload");
		oReq.send(oMyForm);
	}

	function lengthInUtf8Bytes(str) {
		// Matches only the 10.. bytes that are non-initial characters in a multi-byte sequence.
		var m = encodeURIComponent(str).match(/%[89ABab]/g);
		return str.length + (m ? m.length : 0);
	}

	$("#goToLink").click(function (e) {
		window.location = $("#shareLink").html();
	});

	$("#copyToClipboard").click(function (e) {
		window.prompt("Copy to clipboard: Ctrl+C, Enter", $("#shareLink").html());
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
