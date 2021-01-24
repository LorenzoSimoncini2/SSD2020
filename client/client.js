function init() {
}

function saveCsv() {
	var id = $('#indxID').val();
	console.log(id);
	$.ajax(
		{
			url: "https://localhost:5001/api/index/" + id,
			type: "GET",
			contentType: "application/json",
			data: "",
			success: function (result) {
				readResult(JSON.parse(result));
			},
			error: function (xhr, status, p3, p4) {
				var err = "Error " + " " + status + " " + p3;
				if (xhr.responseText && xhr.responseText[0] == "{")
					err = JSON.parse(xhr.responseText).message;
				alert(err);
			}
		});
}

function forecast() {
	var options = {};
	options.url = "https://localhost:5001/api/index/Predict";
	options.type = "POST";
	options.success = function (msg) {
		alert(msg);
	};
	options.error = function (err) {
		alert(err.responseText);
	};
	$.ajax(options);
}

function portfolio() {
	var options = {};
	options.url = "https://localhost:5001/api/index/OptimizePortfolio";
	options.type = "POST";
	options.success = function (msg) {
		alert("Optimization done");
		document.getElementById('txtarea').value = msg;
	};
	options.error = function (err) {
		alert(err.responseText);
	};
	$.ajax(options);
}


function findAll() {
	$.ajax({
		url: "http://localhost:5000/api/Stagione",
		type: "GET",
		contentType: "application/json",
		success: function (result) {
			console.log(result);
			readResult(JSON.stringify(result));
		},
		error: function (xhr, status, p3, p4) {
			var err = "Error " + " " + status + " " + p3;
			if (xhr.responseText && xhr.responseText[0] == "{")
				err = JSON.parse(xhr.responseText).message;
			alert(err);
		}
	});
}

function readResult(res) {
	document.getElementById('txtarea').value = res.text;
	renderImage(res.img);
}

function renderImage(img) {
	var tmp = img;
	tmp = tmp.substring(0, tmp.length - 1);
	tmp = tmp.substring(2, tmp.length);
	
	var image = new Image();
	image.src = 'data:image/png;base64,'+tmp;
	document.getElementById('imgContainer').innerHTML = "";
	document.getElementById('imgContainer').appendChild(image);
}