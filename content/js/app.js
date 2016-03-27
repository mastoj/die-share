window.Handlebars.registerHelper('select', function( value, options ){
        var $el = $('<select />').html( options.fn(this) );
        $el.find('[value="' + value + '"]').attr({'selected':'selected'});
        return $el.html();
});

(function() {
    var readFile = function(path, file, continuation) {
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
        xhttp.open('POST', path, true)
        xhttp.onload = function(e2) {
            var result = JSON.parse(e2.target.response)
            continuation(result)
        }
        xhttp.send(formData)
    }
    DS = {}
    DS.createFileUploader = function(elem, uploadPath, continuation) {
        const uploadFn = resolve => {
            return e => {
                var file = e.target.files[0]
                return readFile(uploadPath, file, resolve)
            }
        }

        const continuation2 = function(data) {
            continuation(data)
            elem.value = ""
        }
        elem.addEventListener("change", uploadFn(continuation2))
    }
})();

(function() {
    DS = DS || {}
    DS.expense = container => {
        const data = {
            files: [],
            project: "",
            userName: "john",
            description: ""
        }
        const updateForm = () => {
            console.log("Updating the form")
        }
        const addFile = file => {
            file.amount = 0
            data.files.push(file)
            console.log(container, data)
        }
        return {
            addFile: addFile
        }
    }
})();

document.addEventListener("DOMContentLoaded", function(event) {
    var uploadPath = '/api/expense/' + 4 + '/file'
    var fileUploaders = document.getElementsByClassName("file-uploader")
    const expenseContainer = DS.expense(document.getElementById("expense-form"))
    var expenseId = window.location.pathname.split( '/' )[2]
    fetch('/api/expense/' + expenseId, {
	       method: 'get',
           headers: {
               'Authorization': 'Basic ' + btoa("tomas:tomas")
           }
    }).then(function(response) {
    	console.log(response)
        response.json().then(function(d) {console.log(d)})
    }).catch(function(err) {
        console.log(err)
    });
    console.log(expenseId)
//    DS.createFileUploader(fileUploaders[0], uploadPath, function(d){expense.addFile(d)})
})
