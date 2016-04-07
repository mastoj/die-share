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

    var submitExpense = data => {
        fetch('/api/expense/' + data.Id + '/submit', {
    	       method: 'post',
               credentials: 'same-origin'
        })
        model.data.notSubmitted = false
    }

    const addFile = (data, rerender) => {
        model.data.Expenses.push({File: data, Amount: 0})
        console.log("File save", model.data)
        save(model.data)
        rerender()
    }

    const reload = rerender => e => {
        var amounts = model.container.querySelectorAll(".file-amount")
        amounts.map(function(amountField){
            var id = amountField.id.split("_")[1]
            var expense = model.data.Expenses.filter(function(e){return e.File.FileId == id})[0].Amount = amountField.value
        })
        console.log(amounts)
        console.log(model.data.Expenses)
        model.data = {
            User: model.data.User,
            Description: model.container.querySelector("input[name='Description']").value,
            Expenses: model.data.Expenses,
            Id: model.data.Id,
            notSubmitted: model.data.notSubmitted,
            Project: model.container.querySelector("select[name='Project']").value,
            Status: model.data.Status
        }
        console.log("rendering stuff", model)
        save(model.data)
        rerender()
    }

    var attachListeners = (container, data, rerender) => {
        container.querySelectorAll("input[type=text]").map(function(e) {
            e.addEventListener("change", reload(rerender))
        })
        container.getElementsByTagName("select").map(function(e) {
            e.addEventListener("change", reload(rerender))
        })
        container.getElementsByTagName("form").map(function(elem) {
            elem.addEventListener("submit", function(e) {
                e.preventDefault()
                submitExpense(model.data)
                rerender()
            })
        })
        var uploadPath = '/api/expense/' + data.Id + '/file'
        var fileUploaders = container.getElementsByClassName("file-uploader")
        DS.createFileUploader(fileUploaders[0], uploadPath, function(d){addFile(d, rerender)})
    }

    var render = () => {
        model.data.Total = model.data.Expenses.reduce(function(prevVal, currentVal) {
            return parseInt(currentVal.Amount) + prevVal
        }, 0)
        var source = document.getElementById("expense-form-template").innerHTML;
        var template = Handlebars.compile(source)
        var html = template(model.data)
        model.container.innerHTML = html
        attachListeners(model.container, model.data, render)
    }

    Expense = {
        load: (data, expenseContainer) => {
            model = {data: data, container: expenseContainer}
            model.data.notSubmitted = data.Status == 0;
            render()
        }
    }
})()

document.addEventListener("DOMContentLoaded", function(event) {
    const expenseContainer = document.getElementById("expense-form-container")
    var expenseId = window.location.pathname.split( '/' )[2]
    fetch('/api/expense/' + expenseId, {
	       method: 'get',
           credentials: 'same-origin'
    }).then(function(response) {
        response.json().then(function(d) {
            Expense.load(d, expenseContainer)
        })
    }).catch(function(err) {
        console.log(err)
    });
})
