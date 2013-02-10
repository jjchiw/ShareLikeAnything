$("body").ready(function () {
	$("#sintaxHighlight").change(function () {
		var str = $(this).find("option:selected").text();
		myCodeMirror.setOption("mode", str);
	})

	editorHub.client.addMessage = function (message, sessionId) {
		var thisSessionId = $("#sessionId").val();
		if (thisSessionId != sessionId) {
			dmp.Match_Distance = 1000;
			dmp.Match_Threshold = 0.5;
			dmp.Patch_DeleteThreshold = 0.5;

			var code = myCodeMirror.getValue();
			var patches = dmp.patch_make(code, message);

			var position = myCodeMirror.getCursor();
			var results = dmp.patch_apply(patches, code);
			myCodeMirror.setValue(results[0]);
			myCodeMirror.setCursor(position);
		}
	};

	$.connection.hub.start();

});