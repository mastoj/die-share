DS = window.DS || {}

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

(function() {
     HTMLCollection.prototype["map"] = function(fn) {
         for(var i = 0; i < this.length; i++) {
             fn(this[i])
         }
     }
     NodeList.prototype["map"] = function(fn) {
         for(var i = 0; i < this.length; i++) {
             fn(this[i])
         }
     }
})();

(function() {
    var model = {
        data: {},
        container: null
    }

    const save = data => {
        fetch('/api/expense/' + data.Id, {
    	       method: 'put',
               credentials: 'same-origin',
               body: JSON.stringify(data)
        })
    }

    const reload = rerender => e => {
        model.data = {
            Description: model.container.querySelector("input[name='Description']").value,
            Expenses: [],
            Id: model.data.Id,
            Project: model.container.querySelector("select[name='Project']").value,
            Status: model.data.Status
        }
        console.log("rendering stuff", model)
        save(model.data)
        rerender()
    }

    var attachListeners = (container, rerender) => {
        container.querySelectorAll("input[type=text]").map(function(e) {
            e.addEventListener("change", reload(rerender))
        })
        container.getElementsByTagName("select").map(function(e) {
            e.addEventListener("change", reload(rerender))
        })
    }

    var render = () => {
        var source = document.getElementById("expense-form-template").innerHTML;
        var template = Handlebars.compile(source)
        var html = template(model.data)
        model.container.innerHTML = html
        attachListeners(model.container, render)
    }

    Expense = {
        load: (data, expenseContainer) => {
            model = {data: data, container: expenseContainer}
            render()
            console.log(data, expenseContainer)
        }
    }
})()

document.addEventListener("DOMContentLoaded", function(event) {
    var uploadPath = '/api/expense/' + 4 + '/file'
    var fileUploaders = document.getElementsByClassName("file-uploader")

    const expenseContainer = document.getElementById("expense-form-container")
//    const expenseContainer = DS.expense(document.getElementById("expense-form-container"))
    var expenseId = window.location.pathname.split( '/' )[2]
    fetch('/api/expense/' + expenseId, {
	       method: 'get',
           credentials: 'same-origin'
    }).then(function(response) {
    	console.log(response)
        response.json().then(function(d) {
            Expense.load(d, expenseContainer)
        })
    }).catch(function(err) {
        console.log(err)
    });
    console.log(expenseId)
//    DS.createFileUploader(fileUploaders[0], uploadPath, function(d){expense.addFile(d)})
})
