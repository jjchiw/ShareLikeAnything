$("body").ready(function () {
	$("#sintaxHighlight").change(function () {
		var str = $(this).find("option:selected").text();
		myCodeMirror.setOption("mode", str);
	})
});