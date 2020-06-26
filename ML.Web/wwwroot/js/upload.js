//const serviceUrl = 'https://localhost:44380/api/ImageClassification/classifyImage';

//const serviceUrl = 'http://localhost:5000/api/ImageClassification/classifyImage';

const serviceUrl = 'api/ImageClassification';
const form = document.querySelector('form');

function collapse(id) {
    var labelDIVid = "labelHeadingAndEdit_" + id;
    var element = document.getElementById(labelDIVid);
    element.classList.toggle("active");
    var content = element.nextElementSibling;
    if (content.style.display === "block") {
        content.style.display = "none";
    } else {
        content.style.display = "block";
    }
}

function openTab(evt, tabName) {
    console.log(evt);
    console.log(tabName);

    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByClassName("tabcontent");
    for (i = 0; i < tabcontent.length; i++) {
        tabcontent[i].style.display = "none";
    }
    tablinks = document.getElementsByClassName("tablinks");
    for (i = 0; i < tablinks.length; i++) {
        tablinks[i].className = tablinks[i].className.replace(" active", "");
    }
    document.getElementById(tabName).style.display = "block";
    evt.currentTarget.className += " active";
}

function labelEdit(elementid) {

    var el = document.getElementById(elementid);
    el.contentEditable = true;

    var range = document.createRange();
    var sel = window.getSelection();
    range.setStart(el, 1);
    range.collapse(true);
    sel.removeAllRanges();
    sel.addRange(range);

    var buttonEdit = document.getElementById("buttonEditForLabel_" + elementid);
    var buttonCancel = document.getElementById("buttonCancelForLabel_" + elementid);
    var buttonSave = document.getElementById("buttonSaveForLabel_" + elementid);

    buttonEdit.hidden = true;
    buttonCancel.hidden = false;
    buttonSave.hidden = false;
}

function labelSaveEdit(elementid, objectid) {
    var el = document.getElementById(elementid);
    var newName = el.textContent;
    el.setAttribute("data-init", newName);

    var guidpartofLabelName = el.getAttribute('data-guid');

    const labelItem = {
        id: objectid,
        name: newName + guidpartofLabelName
    };

    console.log(labelItem);

    fetch(serviceUrl + "/EditLabelClassName",
        {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(labelItem),
        })
        .then((resp) => resp.json())
        .then(function (response) {
            console.log(response);
        });
}

function labelCancelEdit(elementid) {
    var el = document.getElementById(elementid);
    el.contentEditable = false;
    el.textContent = el.getAttribute("data-init");

    var buttonEdit = document.getElementById("buttonEditForLabel_" + elementid);
    var buttonCancel = document.getElementById("buttonCancelForLabel_" + elementid);
    var buttonSave = document.getElementById("buttonSaveForLabel_" + elementid);

    buttonEdit.hidden = false;
    buttonCancel.hidden = true;
    buttonSave.hidden = true;
}

function labelCancelEditEsc(e) {
    if (e.key == "Escape") {
        labelCancelEdit(e.target.id);
    }
}

function loadImages() {
    fetch(serviceUrl + "/GetAllImages") // Call the fetch function passing the url of the API as a parameter
        .then((resp) => resp.json())
        .then(function (response) {

            console.log(response);

            for (var i = 0; i < response.length; i++) {
                var divAdvertisements = document.getElementById("divAdvertisements");
                var x = document.createElement("div");
                var id = "advertisement" + i;
                x.setAttribute("id", id);

                var lastIndex = response[i].predictedLabel.lastIndexOf('_');
                var firstPartOfLabelName = response[i].predictedLabel.substr(0, lastIndex);
                var guidpartofLabelName = response[i].predictedLabel.substr(lastIndex, response[i].predictedLabel.Length);

                var labelHeading = document.createElement("h4");
                var labelHeadingElementID = "firstPartOfLabelName" + i;
                labelHeading.setAttribute("id", labelHeadingElementID);
                labelHeading.textContent = firstPartOfLabelName;
                labelHeading.setAttribute('data-init', firstPartOfLabelName);
                labelHeading.setAttribute('data-guid', guidpartofLabelName);
                labelHeading.style.classList = "noselect";
                labelHeading.style.display = "inline-block";
                labelHeading.setAttribute("onblur", "labelCancelEdit('" + labelHeading.id + "')");
                labelHeading.addEventListener('keydown', labelCancelEditEsc);

                var span = document.createElement("span");
                span.textContent = guidpartofLabelName;
                span.style.classList = "noselect";
                span.style.display = "inline-block";
                span.style.fontSize = "1.5rem";
                span.style.marginTop = "0.32rem";
                //span.style.marginBottom = "1rem";
                span.style.fontFamily = "inherit";
                span.style.fontWeight = "500";
                span.style.lineHeight = "1.2";
                span.style.color = "inherit";

                var editbutton = document.createElement("button");
                editbutton.setAttribute("id", "buttonEditForLabel_" + labelHeadingElementID);
                editbutton.textContent = "Edit";
                editbutton.classList = "btn";
                editbutton.style.position = "absolute";
                editbutton.style.marginLeft = "2%";
                editbutton.setAttribute("onClick", "labelEdit('" + labelHeadingElementID + "')");

                var saveButton = document.createElement("button");
                saveButton.setAttribute("id", "buttonSaveForLabel_" + labelHeadingElementID);
                saveButton.textContent = "Save";
                saveButton.classList = "btn btn-success";
                saveButton.hidden = true;
                saveButton.style.position = "absolute";
                saveButton.style.marginLeft = "2%";
                saveButton.setAttribute("onMouseDown", "labelSaveEdit('" + labelHeadingElementID + "','" + response[i].id + "')");

                var cancelButton = document.createElement("button");
                cancelButton.setAttribute("id", "buttonCancelForLabel_" + labelHeadingElementID);
                cancelButton.textContent = "Cancel";
                cancelButton.classList = "btn btn-danger";
                cancelButton.hidden = true;
                cancelButton.style.position = "absolute";
                cancelButton.style.marginLeft = "8%";
                cancelButton.setAttribute("onMouseDown", "labelCancelEdit('" + labelHeadingElementID + "')");

                var labelHeadingAndEdit = document.createElement("div");
                labelHeadingAndEdit.setAttribute("id", "labelHeadingAndEdit_" + labelHeadingElementID);
                labelHeadingAndEdit.classList = "collapsible";
                labelHeadingAndEdit.setAttribute("onClick", "collapse('" + labelHeadingElementID + "')");
                labelHeadingAndEdit.style.cursor = "pointer";
                labelHeadingAndEdit.style.marginBottom = "0.6%";

                labelHeadingAndEdit.appendChild(labelHeading);
                labelHeadingAndEdit.appendChild(span);
                labelHeadingAndEdit.appendChild(editbutton);
                labelHeadingAndEdit.appendChild(saveButton);
                labelHeadingAndEdit.appendChild(cancelButton);
                x.appendChild(labelHeadingAndEdit);
                divAdvertisements.appendChild(x);

                //var coll = document.getElementsByClassName("collapsible");
                //var i;

                //for (i = 0; i < coll.length; i++) {
                //    coll[i].addEventListener("click", function () {
                //        this.classList.toggle("active");
                //        var content = this.nextElementSibling;
                //        if (content.style.display === "block") {
                //            content.style.display = "none";
                //        } else {
                //            content.style.display = "block";
                //        }
                //    });
                //}
                var advertisementImagesDiv = document.createElement("div");
                advertisementImagesDiv.setAttribute("id", "advertisementImagesDiv_" + id);

                for (var j = 0; j < response[i].advertisements.length; j++) {
                    //var y = document.createElement("div");
                    //var idy = "advertisementImage" + j;
                    //y.setAttribute("id", idy);
                    var imageId = response[i].advertisements[j].imageId;
                    var figure = document.createElement("figure");
                    figure.style.height = "180px";
                    figure.style.width = "180px";

                    var img = document.createElement("img");
                    var idimg = "advertisementImageIMG" + j;
                    img.setAttribute("id", idimg);
                    img.setAttribute("src", "images_to_train/" + response[i].predictedLabel + "/" + imageId);
                    img.setAttribute("height", "180px");
                    img.setAttribute("width", "180px");
                    //y.appendChild(img);
                    //divAdvertisements.appendChild(y);
                    figure.style.display = "inline-block";
                    figure.style.marginRight = "2%";
                    figure.style.marginLeft = "2%";

                    figure.appendChild(img);

                    var figCaption = document.createElement("figcaption");
                    figCaption.textContent = (response[i].advertisements[j].maxProbability * 100).toFixed(3) + "%";
                    figCaption.style.textAlign = "center";
                    figure.appendChild(figCaption);

                    advertisementImagesDiv.appendChild(figure);
                }
                x.appendChild(advertisementImagesDiv);
            }
        })
}

function preview_image_reader(id, src) {
    var reader = new FileReader();
    reader.onload = function () {
        var output = document.getElementById(id);
        output.src = reader.result;
    }
    reader.readAsDataURL(src);
}

loadImages();

function preview_image(event) {
    var reader = new FileReader();
    reader.onload = function () {
        var output = document.getElementById('theImage');
        output.src = reader.result;
    }
    reader.readAsDataURL(event.target.files[0]);
}

form.addEventListener('submit', e => {
    e.preventDefault();

    const files = document.querySelector('[type=file]').files;
    const formData = new FormData();

    formData.append('imageFile', files[0]);

    // If multiple files uploaded at the same time:
    //for (let i = 0; i < files.length; i++) {
    //    let file = files[i];
    //
    //    formData.append('imageFile[]', file);
    //}

    fetch(serviceUrl + "/classifyimagecustom", {
        method: 'POST',
        body: formData
    }).then((resp) => resp.json())
        .then(function (response) {
            console.log('Response', response);

            console.log('Prediction is: ' + 'Label: ' + response.predictedLabel + ' Probability/Score: ' + response.maxScore);

            document.getElementById('divPrediction').innerHTML = "Predicted label is: " + response.predictedLabel;
            document.getElementById('divProbability').innerHTML = "Probability is: " + (response.maxScore * 100).toFixed(3) + "%";

            loadImages();

            return response;
        });


    //fetch(serviceUrl + "/classifyImage", {
    //    method: 'POST',
    //    body: formData
    //}).then((resp) => resp.json())
    //    .then(function (response) {
    //        console.info('Response', response);
    //        console.log('Response', response);

    //        console.log('Prediction is: ' + 'Label: ' + response.predictedLabel + ' Probability: ' + response.probability);

    //        document.getElementById('divPrediction').innerHTML = "Predicted label is: " + response.predictedLabel;
    //        document.getElementById('divProbability').innerHTML = "Probability is: " + (response.maxProbability * 100).toFixed(3) + "%";

    //        //document.getElementById('listProbabilities').innerHTML = response.d;
    //        //console.log(typeof(response.allProbabilities));

    //        //if (response.allProbabilities !== undefined) {
    //        //    var allProbabilityArray = Object.keys(response.allProbabilities).map(function (key) {
    //        //        return [String(key), response.allProbabilities[key]];
    //        //    });

    //        //    for (var i = 0; i < allProbabilityArray.length; i++) {
    //        //        var table = document.getElementById("tableProbabilities");
    //        //        var row = table.insertRow(0);
    //        //        var cell1 = row.insertCell(0);
    //        //        var cell2 = row.insertCell(1);
    //        //        cell1.innerHTML = allProbabilityArray[i][0];
    //        //        cell2.innerHTML = ((allProbabilityArray[i][1] * 100).toFixed(3) + "%");
    //        //    }
    //        //}

    //        return response;
    //    });


});