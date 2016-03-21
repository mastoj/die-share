(function() {
    console.log("Hello app")
    var readFile = function(file) {
        var formData = new FormData()
        var fileMeta = {
            'fileName': file.name,
            'fileType': file.type,
            'fileSize': file.size
        }
        var fileMetaJson = JSON.stringify(fileMeta)
        formData.append("fileMeta", fileMetaJson)
        formData.append("file", file)
        var xhttp = new XMLHttpRequest();
        xhttp.open('POST', '/expenses', true)
        xhttp.onload = function(e2) {
            var result = JSON.parse(e2.target.response)
            console.log(result)
        }
        xhttp.send(formData)
    }
    DS = {}
    DS.uploadFile = function(e) {
        console.log(e)
        console.log(e.target.files[0])
        var file = e.target.files[0]
        if(file) {
            readFile(file)
        }
    }
})();

(function() {
    HTMLCollection.prototype["map"] = function(fn) {
        for(var i = 0; i < this.length; i++) {
            fn(this[i])
        }
    }
})()

document.addEventListener("DOMContentLoaded", function(event) {
    var fileUploaders = document.getElementsByClassName("file-uploader");
    fileUploaders.map(function(e) {
        e.addEventListener("change", DS.uploadFile)
    })
    console.log(fileUploaders)
})
//<input type="file" id="input" onchange="handleFiles(this.files)">
