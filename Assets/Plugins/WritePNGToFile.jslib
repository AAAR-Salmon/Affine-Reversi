mergeInto(LibraryManager.library, {
	DownloadPNG: function(bytes, size) {
		var buffer = new Uint8Array(size).map(function (_, i) {
			return HEAPU8[bytes + i];
		});
		var blob;
		try {
			blob = new Blob([buffer.buffer], {
				type: 'image/png'
			})
		} catch (e) {
			return;
		}
		var url = URL.createObjectURL(blob);
		var a = document.createElement('a');
		a.href = url;
		a.download = 'screenshot.png';
		a.style.display = 'none';
		document.body.appendChild(a);
		a.click();
		document.body.removeChild(a);
	}
});