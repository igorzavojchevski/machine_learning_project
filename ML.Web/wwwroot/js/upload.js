//const serviceUrl = 'https://localhost:44380/api/ImageClassification/classifyImage';

//const serviceUrl = 'http://localhost:5000/api/ImageClassification/classifyImage';

const serviceUrl = 'api/ImageClassification';
const form = document.querySelector('#formCheckLabel');
var allLabels = new Array();
var paginationInitialized = false;

$(document).ready(function () {
    $('#btnCommercials').trigger('click');
});

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

    $("#divCommercials").html("");
    $("#timeFramesDiv").html("");
    $("#evaluationStreamsDiv").html("");
    $("#systemSettingsDiv").html("");

    if (tabName === "Labels") {
        paginationInitialized = false;
        LoadLabelsAndImages(4, 1);
    }
    else if (tabName === "LabelTimeFrames") {
        GetLabelTimeFrames();
    }
    else if (tabName === "ImageCheck") {
        LoadImageCheck();
    }
    else if (tabName === "EvaluationStreams") {
        GetEvaluationStreams();
    }
    else if (tabName === "SystemSettings") {
        GetSystemSettings();
    }

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
    evt.target.className += " active";
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
        .then(function (response) {
            console.log(response);
            $('#btnCommercials').trigger('click');
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

function saveMoveImages(commercialid, parentDIV) {
    var arrayOfSelected = $("#" + parentDIV + " > ul.thumbnails").find(".selected").toArray();
    //console.log(arrayOfSelected);
    //var arrayImagesOfCommercial = arrayOfSelected.filter((x) => { return x.parentElement.getAttribute("id") === "commercialImagesDivSelect_" + commercialid; });
    var result = arrayOfSelected.map(a => a.getAttribute("value"));
    console.log(result);

    var selectElement = document.getElementById("commercialImagesDiv_SELECTForMove_" + commercialid);
    var selectedValue = selectElement.options[selectElement.selectedIndex].value;

    const moveImagesModel = {
        newClassNameId: selectedValue,
        imagesIds: result
    };

    console.log(moveImagesModel);

    fetch(serviceUrl + "/MoveImages",
        {
            method: 'POST',
            headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' },
            body: JSON.stringify(moveImagesModel),
        })
        .then(function (response) {
            console.log(response);
            $('#btnCommercials').trigger('click');
        });
}

function LoadLabelsAndImages(size, page, showTrained) {
    fetch(serviceUrl + "/GetAllImages?size=" + size + "&page=" + page + (showTrained == null ? "" : "&showTrained=" + showTrained),
        {
            headers: { 'Accept': 'application/json', 'Content-Type': 'application/x-www-form-urlencoded' }
        }) // Call the fetch function passing the url of the API as a parameter
        .then((resp) => resp.json())
        .then(function (response) {

            GetAllLabels();

            console.log(response);

            CreateLabelAndImageSection(response);

            //InitializeImagePicker();
            InitializePaginationPlaceholder(response);
        })
}

function GetAllLabels() {
    jQuery.ajax({
        url: serviceUrl + "/GetAllAvailableLabels",
        success: function (allLabelResponse) {
            console.log(allLabelResponse);
            allLabels = new Array();
            allLabels = allLabelResponse;
        },
        async: false
    });
}

function CreateLabelAndImageSection(groupList) {
    var divCommercials = document.getElementById("divCommercials");
    divCommercials.innerHTML = "";

    for (var i = 0; i < groupList.group.length; i++) {
        var x = document.createElement("div");
        var id = "commercial" + i;
        x.setAttribute("id", id);

        var lastIndex = groupList.group[i].predictedLabel.lastIndexOf('_');
        var firstPartOfLabelName = groupList.group[i].predictedLabel;
        var guidpartofLabelName = "";
        if (lastIndex !== -1) {
            firstPartOfLabelName = groupList.group[i].predictedLabel.substr(0, lastIndex);
            guidpartofLabelName = groupList.group[i].predictedLabel.substr(lastIndex, groupList.group[i].predictedLabel.Length);
        }

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

        var spanForCount = document.createElement("span");
        spanForCount.textContent = "(" + groupList.group[i].commercials.length + " items)";
        spanForCount.style.classList = "noselect";
        spanForCount.style.display = "block";
        spanForCount.style.fontSize = "1rem";
        spanForCount.style.fontFamily = "inherit";
        spanForCount.style.fontWeight = "500";
        spanForCount.style.lineHeight = "1.2";
        spanForCount.style.color = "inherit";


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
        saveButton.setAttribute("onMouseDown", "labelSaveEdit('" + labelHeadingElementID + "','" + groupList.group[i].id + "')");

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
        labelHeadingAndEdit.appendChild(spanForCount);
        x.appendChild(labelHeadingAndEdit);
        divCommercials.appendChild(x);

        var commercialImagesDiv = document.createElement("div");
        commercialImagesDiv.setAttribute("id", "commercialImagesDiv_" + id);
        commercialImagesDiv.style.display = "none";

        var commercialImagesDiv_DIVForMove = document.createElement("div");
        commercialImagesDiv_DIVForMove.setAttribute("id", "commercialImagesDiv_DIVForMove_" + id);
        commercialImagesDiv_DIVForMove.style.marginBottom = "1%";
        var span_commercialImagesDiv_SELECTForMove = document.createElement("span");
        span_commercialImagesDiv_SELECTForMove.textContent = "Move to: ";
        var commercialImagesDiv_SELECTForMove = document.createElement("select");
        commercialImagesDiv_SELECTForMove.setAttribute("id", "commercialImagesDiv_SELECTForMove_" + id);
        for (var li = 0; li < allLabels.length; li++) {
            if (allLabels[li].className === groupList.group[i].predictedLabel) continue;

            var opt = document.createElement("option");
            opt.value = allLabels[li].id;
            opt.textContent = allLabels[li].className;

            commercialImagesDiv_SELECTForMove.appendChild(opt);
        }
        commercialImagesDiv_DIVForMove.hidden = true;

        var commercialImagesSaveButton = document.createElement("button");
        commercialImagesSaveButton.setAttribute("id", "buttonSaveForMoveImages_" + id);
        commercialImagesSaveButton.classList = "btn btn-success";
        commercialImagesSaveButton.textContent = "Save";
        commercialImagesSaveButton.style.fontSize = "12px";
        commercialImagesSaveButton.style.position = "absolute";
        commercialImagesSaveButton.style.marginLeft = "2%";
        commercialImagesSaveButton.style.marginTop = "-0.25%";
        commercialImagesSaveButton.setAttribute("onMouseDown", "saveMoveImages('" + id + "','" + commercialImagesDiv.id + "')");

        commercialImagesDiv_DIVForMove.appendChild(span_commercialImagesDiv_SELECTForMove)
        commercialImagesDiv_DIVForMove.appendChild(commercialImagesDiv_SELECTForMove);
        commercialImagesDiv_DIVForMove.appendChild(commercialImagesSaveButton);
        commercialImagesDiv.appendChild(commercialImagesDiv_DIVForMove);


        var commercialImagesDivSelect = document.createElement("select");
        var commercialImagesDivSelectID = "commercialImagesDivSelect_" + id;
        commercialImagesDivSelect.setAttribute("id", commercialImagesDivSelectID);
        commercialImagesDivSelect.classList = "image-picker";
        commercialImagesDivSelect.setAttribute("multiple", "multiple");

        var initializedImagePicker = false;

        for (var j = 0; j < groupList.group[i].commercials.length; j++) {

            var imageId = groupList.group[i].commercials[j].imageId;

            var commGroupGuid = "commGroup_" + i + "_" + groupList.group[i].commercials[j].groupGuid;
            var optGroup = document.getElementById(commGroupGuid);
            console.log(!optGroup);
            if (!optGroup) {
                optGroup = document.createElement("optgroup");
                optGroup.setAttribute("id", commGroupGuid);
                optGroup.setAttribute("label", groupList.group[i].commercials[j].groupGuid);
            }

            var selectOption = document.createElement("option");
            selectOption.setAttribute("data-img-src", "images_to_train/" + groupList.group[i].predictedLabel + "/" + imageId);
            selectOption.setAttribute("data-img-label", (groupList.group[i].commercials[j].maxProbability * 100).toFixed(3) + "%");
            selectOption.setAttribute("data-img-label2", "(" + FormatDateAndTime(groupList.group[i].commercials[j].imageDateTime) + ")");
            selectOption.setAttribute("data-img-label3", groupList.group[i].commercials[j].trainedForClassName === groupList.group[i].predictedLabel ? (groupList.group[i].commercials[j].isTrained ? "<span style='color:darkgreen;font-weight:600'>Trained</span>" : "<span style='color:red;font-weight:600'>Not trained</span>") : "<span style='color:red;font-weight:600'>Not trained for this label</span>");
            selectOption.setAttribute("value", groupList.group[i].commercials[j].id);
            optGroup.appendChild(selectOption);
            commercialImagesDivSelect.append(optGroup);

            if (!initializedImagePicker) { commercialImagesDiv.appendChild(commercialImagesDivSelect); x.appendChild(commercialImagesDiv); InitializeImagePicker(commercialImagesDivSelectID); initializedImagePicker = true; }
            else $("#" + commercialImagesDivSelectID).data("picker").sync_picker_with_select();
        }
        InitializeImagePicker(commercialImagesDivSelectID);
    }
}

function InitializePaginationPlaceholder(responseImages) {
    if (paginationInitialized) return;

    var size = 4; //make this systemsetting
    $("#labelPagination").pagination({
        items: responseImages.count,
        itemsOnPage: size,
        cssStyle: 'light-theme',
        onPageClick: function (e) {
            LoadLabelsAndImages(size, e);
        }
    });
    paginationInitialized = true;
}

function InitializeImagePicker(id) {
    $("#" + id).imagepicker({
        show_label: true,
        clicked: function () {
            //console.log(this.find("option:selected"));
            //console.log(this.find("option:selected").length);
            //console.log(this.find("option:selected").prevObject[0].getAttribute("id"));
            var selectID = this.find("option:selected").prevObject[0].getAttribute("id");
            var lastIndex = selectID.lastIndexOf('_');
            var commercialID = selectID.substr(lastIndex + 1, selectID.Length);

            var element = document.getElementById("commercialImagesDiv_DIVForMove_" + commercialID);
            if (this.find("option:selected").length > 0) {
                //console.log('testtt');
                element.hidden = false;
            }
            else {
                //console.log('testtt123123');
                element.hidden = true;
            }
        }
    });
}

function GetLabelTimeFrames() {

    fetch(serviceUrl + "/GetLabelTimeFrames",
        {
            method: 'GET',
            headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' },
        })
        .then((resp) => resp.json())
        .then(function (response) {
            console.log(response);
            var timeFramesDiv = document.getElementById("timeFramesDiv");
            var table = document.createElement("table");
            table.style.width = '100%';
            table.setAttribute('border', '1');
            var tbody = document.createElement('tbody');
            var th1 = document.createElement('th');
            var th2 = document.createElement('th');
            var th3 = document.createElement('th');
            var th4 = document.createElement('th');
            var th5 = document.createElement('th');
            th1.textContent = "Commercial Name";
            th1.style.textAlign = "center";
            th2.textContent = "Start Time";
            th2.style.textAlign = "center";
            th3.textContent = "End Time";
            th3.style.textAlign = "center";
            th4.textContent = "Stream Name";
            th4.style.textAlign = "center";
            th5.textContent = "Evaluated by";
            th5.style.textAlign = "center";
            tbody.appendChild(th1);
            tbody.appendChild(th2);
            tbody.appendChild(th3);
            tbody.appendChild(th4);
            tbody.appendChild(th5);

            for (var i = 0; i < response.length; i++) {

                var tr = document.createElement('tr');
                var td = document.createElement('td');
                td.setAttribute("colspan", "5");
                var date = new Date(response[i].dateTimeKey);
                td.textContent = FormatDate(date);
                td.style.textAlign = "center";
                tr.appendChild(td);
                tbody.appendChild(tr);

                for (var j = 0; j < response[i].labelTimeFrameGroups.length; j++) {
                    var tr1 = document.createElement('tr');
                    console.log('test');
                    var td1 = document.createElement('td');
                    td1.textContent = response[i].labelTimeFrameGroups[j].className;
                    var td2 = document.createElement('td');
                    td2.textContent = FormatDateAndTime(response[i].labelTimeFrameGroups[j].startDate);
                    td2.style.textAlign = "center";
                    var td3 = document.createElement('td');
                    td3.textContent = FormatDateAndTime(response[i].labelTimeFrameGroups[j].endDate);
                    td3.style.textAlign = "center";
                    var td4 = document.createElement('td');
                    td4.textContent = response[i].labelTimeFrameGroups[j].evaluationStreamName;
                    td4.style.textAlign = "center";
                    var td5 = document.createElement('td');
                    td5.textContent = response[i].labelTimeFrameGroups[j].classifiedBy;
                    td5.style.textAlign = "center";
                    tr1.appendChild(td1);
                    tr1.appendChild(td2);
                    tr1.appendChild(td3);
                    tr1.appendChild(td4);
                    tr1.appendChild(td5);
                    tbody.appendChild(tr1);
                }
            }
            table.appendChild(tbody);
            timeFramesDiv.appendChild(table);
        });
}

function FormatDate(date) {

    const d = new Date(date)
    const ye = new Intl.DateTimeFormat('en', { year: 'numeric' }).format(d);
    const mo = new Intl.DateTimeFormat('en', { month: 'short' }).format(d);
    const da = new Intl.DateTimeFormat('en', { day: '2-digit' }).format(d);

    return `${da}-${mo}-${ye}`;
}

function FormatDateAndTime(date) {

    const d = new Date(date)

    return `${d.getDate()}/${d.getMonth() + 1}/${d.getFullYear()} ${(d.getUTCHours() < 10 ? '0' : '') + d.getUTCHours()}:${(d.getMinutes() < 10 ? '0' : '') + d.getMinutes()}:${(d.getSeconds() < 10 ? '0' : '') + d.getSeconds()}`;
}

function preview_image(event) {
    var reader = new FileReader();
    reader.onload = function () {
        var output = document.getElementById('theImage');
        output.src = reader.result;
    }
    reader.readAsDataURL(event.target.files[0]);
}

function SaveCustomImage() {

    const files = document.querySelector('[type=file]').files;
    var element = document.getElementById("select_divSaveCustomImage");
    var selectedValue = element.options[element.selectedIndex].value;

    var formDataObj = new FormData();
    formDataObj.append('labelID', selectedValue);
    formDataObj.append('image', files[0]);

    fetch(serviceUrl + "/SaveCustomImage",
        {
            method: 'POST',
            body: formDataObj
        })
        .then(function (response) {
            console.log(response);
            if (response.status === 200) {
                $('#btnImageCheck').trigger('click');
            }
        });
}

function LoadImageCheck() {
    $('#imgFile').val("");
    $('#theImage').attr('src', "");
    $('#divPrediction').html("");
    $('#divProbability').html("");
    $('#divSaveCustomImage').html("");
    $("#buttonSaveCustomImage").attr("hidden", true);
}

function OpenCreateCommercialModal() {

    var modal = document.getElementById("createCommercialModal");

    modal.style.display = "block";

    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    }
}

function CloseCreateCommercialModal() {
    var modal = document.getElementById("createCommercialModal");

    modal.style.display = "none";
}

function CreateNewCommercial() {
    var inputValue = document.getElementById("newCommercialInput").value;

    var newCommercialClass = {
        name: inputValue
    };

    console.log(newCommercialClass);

    fetch(serviceUrl + "/CreateLabelClassName",
        {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newCommercialClass),
        })
        .then(function (response) {
            console.log(response);
            CloseCreateCommercialModal();
            $('#btnCommercials').trigger('click');
        });
}

function GetEvaluationStreams() {

    fetch(serviceUrl + "/GetEvaluationStreams",
        {
            method: 'GET',
            headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' },
        })
        .then((resp) => resp.json())
        .then(function (response) {
            console.log(response);
            var evaluationStreamsDiv = document.getElementById("evaluationStreamsDiv");
            var table = document.createElement("table");
            table.style.width = '100%';
            table.setAttribute('border', '1');
            var tbody = document.createElement('tbody');
            var th1 = document.createElement('th');
            var th2 = document.createElement('th');
            var th3 = document.createElement('th');
            var th4 = document.createElement('th');
            th1.textContent = "Name";
            th1.style.textAlign = "center";
            th2.textContent = "Stream";
            th2.style.textAlign = "center";
            th3.textContent = "Code";
            th3.style.textAlign = "center";
            th4.textContent = "Active";
            th4.style.textAlign = "center";
            tbody.appendChild(th1);
            tbody.appendChild(th2);
            tbody.appendChild(th3);
            tbody.appendChild(th4);

            for (var i = 0; i < response.length; i++) {
                var tr = document.createElement('tr');
                tbody.appendChild(tr);

                var td1 = document.createElement('td');
                td1.textContent = response[i].name;
                td1.style.textAlign = "center";
                var td2 = document.createElement('td');
                td2.textContent = response[i].stream;
                var td3 = document.createElement('td');
                td3.textContent = response[i].code;
                td3.style.textAlign = "center";
                var td4 = document.createElement('td');
                td4.textContent = response[i].isActive;
                td4.style.textAlign = "center";
                var td5 = document.createElement('td');
                td5.style.textAlign = "center";
                var td5buttonEdit = document.createElement("button");
                td5buttonEdit.textContent = "Edit";
                td5buttonEdit.classList = "btn";
                td5buttonEdit.style.fontSize = "12px";
                td5buttonEdit.setAttribute("onClick", "editEvaluationStream('" + response[i].id + "', '" + response[i].name + "', '" + response[i].stream + "', '" + response[i].code + "', " + response[i].isActive + ")");

                td5.appendChild(td5buttonEdit);
                tr.appendChild(td1);
                tr.appendChild(td2);
                tr.appendChild(td3);
                tr.appendChild(td4);
                tr.appendChild(td5);
                tbody.appendChild(tr);
            }
            table.appendChild(tbody);
            evaluationStreamsDiv.appendChild(table);
        });
}


function GetSystemSettings() {

    fetch(serviceUrl + "/GetSystemSettings",
        {
            method: 'GET',
            headers: { 'Accept': 'application/json', 'Content-Type': 'application/json' },
        })
        .then((resp) => resp.json())
        .then(function (response) {
            console.log(response);
            var systemSettingsDiv = document.getElementById("systemSettingsDiv");
            var table = document.createElement("table");
            table.style.width = '100%';
            table.setAttribute('border', '1');
            var tbody = document.createElement('tbody');
            var th1 = document.createElement('th');
            var th2 = document.createElement('th');
            var th3 = document.createElement('th');
            var th4 = document.createElement('th');
            th1.textContent = "Setting Key";
            th1.style.textAlign = "center";
            th2.textContent = "Setting Value";
            th2.style.textAlign = "center";
            th3.textContent = "Date Modified";
            th3.style.textAlign = "center";
            th4.textContent = "Modified By";
            th4.style.textAlign = "center";
            tbody.appendChild(th1);
            tbody.appendChild(th2);
            tbody.appendChild(th3);
            tbody.appendChild(th4);

            for (var i = 0; i < response.length; i++) {
                var tr = document.createElement('tr');
                tbody.appendChild(tr);

                var td1 = document.createElement('td');
                td1.textContent = response[i].settingKey;
                var td2 = document.createElement('td');
                td2.textContent = response[i].settingValue;
                var td3 = document.createElement('td');
                td3.textContent =  FormatDateAndTime(response[i].modifiedOn);
                td3.style.textAlign = "center";
                var td4 = document.createElement('td');
                td4.textContent = response[i].modifiedBy;
                td4.style.textAlign = "center";
                var td5 = document.createElement('td');
                td5.style.textAlign = "center";
                var td5buttonEdit = document.createElement("button");
                td5buttonEdit.textContent = "Edit";
                td5buttonEdit.classList = "btn";
                td5buttonEdit.style.fontSize = "12px";
                td5buttonEdit.setAttribute("onClick", "editSystemSetting('" + response[i].id + "', '" + response[i].settingKey + "', '" + response[i].settingValue + "')");

                td5.appendChild(td5buttonEdit);
                tr.appendChild(td1);
                tr.appendChild(td2);
                tr.appendChild(td3);
                tr.appendChild(td4);
                tr.appendChild(td5);
                tbody.appendChild(tr);
            }
            table.appendChild(tbody);
            systemSettingsDiv.appendChild(table);
        });
}

function editEvaluationStream(id, name, stream, code, isActive) {
    console.log(id, name, stream, code, isActive);
    var idInputValueById = document.getElementById("idEvaluationStreamInput");
    idInputValueById.value = id;
    var nameInputValueById = document.getElementById("nameEvaluationStreamInput");
    nameInputValueById.value = name;
    var streamInputValueById = document.getElementById("streamEvaluationStreamInput");
    streamInputValueById.value = stream;
    var codeInputValueById = document.getElementById("codeEvaluationStreamInput");
    codeInputValueById.value = code;
    var isActiveInputValueById = document.getElementById("isActiveEvaluationStreamInput");
    isActiveInputValueById.checked = isActive;
    console.log(isActiveInputValueById);

    OpenCreateEvaluationStreamModal(false);
}

function OpenCreateEvaluationStreamModal(isCreate) {

    var modal = document.getElementById("createEvaluationStreamModal");
    modal.style.display = "block";

    if (!isCreate) document.getElementById("buttonCreateNewEvaluationStream").textContent = "Edit";
    else document.getElementById("buttonCreateNewEvaluationStream").textContent = "Create";

    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    }
}
function CreateEvaluationStream() {
    var idInputValue = document.getElementById("idEvaluationStreamInput").value;
    var nameInputValue = document.getElementById("nameEvaluationStreamInput").value;
    var streamInputValue = document.getElementById("streamEvaluationStreamInput").value;
    var codeInputValue = document.getElementById("codeEvaluationStreamInput").value;
    var isActiveInputValue = document.getElementById("isActiveEvaluationStreamInput").checked;

    var newEvaluationStream = {
        id: idInputValue,
        name: nameInputValue,
        stream: streamInputValue,
        code: codeInputValue,
        isActive: isActiveInputValue
    };

    console.log(newEvaluationStream);

    fetch(serviceUrl + "/CreateEvaluationStream",
        {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newEvaluationStream),
        })
        .then(function (response) {
            console.log(response);
            CloseCreateEvaluationStreamModal();
            $('#btnEvaluationStreams').trigger('click');
        });
}

function CloseCreateEvaluationStreamModal() {
    var modal = document.getElementById("createEvaluationStreamModal");

    modal.style.display = "none";

    var idInputValue = document.getElementById("idEvaluationStreamInput");
    idInputValue.value = "";
    var nameInputValue = document.getElementById("nameEvaluationStreamInput");
    nameInputValue.value = "";
    var streamInputValue = document.getElementById("streamEvaluationStreamInput");
    streamInputValue.value = "";
    var codeInputValue = document.getElementById("codeEvaluationStreamInput");
    codeInputValue.value = "";
    var isActiveInputValue = document.getElementById("isActiveEvaluationStreamInput");
    isActiveInputValue.checked = false;
}



function OpenSystemSettingModal(isCreate) {
    var modal = document.getElementById("systemSettingsModal");
    modal.style.display = "block";

    if (!isCreate) document.getElementById("buttonCreateNewSystemSetting").textContent = "Edit";
    else document.getElementById("buttonCreateNewSystemSetting").textContent = "Create";

    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    }
}

function CloseSystemSettingsModal() {
    var modal = document.getElementById("systemSettingsModal");

    modal.style.display = "none";

    var idInputValue = document.getElementById("idSystemSettingsInput");
    idInputValue.value = "";
    var settingKeyInput = document.getElementById("settingKeyInput");
    settingKeyInput.value = "";
    var settingValueInput = document.getElementById("settingValueInput");
    settingValueInput.value = "";
}


function CreateSystemSetting() {
    var idInputValue = document.getElementById("idSystemSettingsInput").value;
    var settingKeyInputValue = document.getElementById("settingKeyInput").value;
    var settingValueInputValue = document.getElementById("settingValueInput").value;

    var newSettingKey = {
        id: idInputValue,
        settingKey: settingKeyInputValue,
        settingValue: settingValueInputValue
    };

    console.log(newSettingKey);

    fetch(serviceUrl + "/CreateSystemSetting",
        {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newSettingKey),
        })
        .then(function (response) {
            console.log(response);
            CloseSystemSettingsModal();
            $('#btnSystemSettings').trigger('click');
        });
}

function editSystemSetting(id, settingKey, settingValue) {
    console.log(id, settingKey, settingValue);
    var idInputValue = document.getElementById("idSystemSettingsInput");
    idInputValue.value = id;
    var settingKeyInput = document.getElementById("settingKeyInput");
    settingKeyInput.value = settingKey;
    var settingValueInput = document.getElementById("settingValueInput");
    settingValueInput.value = settingValue;

    OpenSystemSettingModal(false);
}



form.addEventListener('submit', e => {
    e.preventDefault();

    console.log('TestSubmit');

    const files = document.querySelector('[type=file]').files;
    const formData = new FormData();

    formData.append('imageFile', files[0]);

    // If multiple files uploaded at the same time:
    //for (let i = 0; i < files.length; i++) {
    //    let file = files[i];
    //
    //    formData.append('imageFile[]', file);
    //}

    fetch(serviceUrl + "/PredictCustomImage", {
        method: 'POST',
        body: formData
    }).then((resp) => resp.json())
        .then(function (response) {
            console.log('Response', response);

            console.log('Prediction is: ' + 'Label: ' + response.predictedLabel + ' Probability/Score: ' + response.maxScore);

            document.getElementById('divPrediction').innerHTML = "Predicted label is: " + response.predictedLabel;
            document.getElementById('divProbability').innerHTML = "Probability is: " + (response.maxScore * 100).toFixed(3) + "%";

            document.getElementById('buttonSaveCustomImage').hidden = false;

            jQuery.ajax({
                url: serviceUrl + "/GetAllAvailableLabels",
                success: function (allLabelResponse) {
                    console.log(allLabelResponse);
                    allLabels = new Array();
                    allLabels = allLabelResponse;
                },
                async: false
            });

            var span_divSaveCustomImage = document.createElement("span");
            span_divSaveCustomImage.textContent = "Save to: ";
            var select_divSaveCustomImage = document.createElement("select");
            select_divSaveCustomImage.setAttribute("id", "select_divSaveCustomImage");
            for (var li = 0; li < allLabels.length; li++) {
                var opt = document.createElement("option");
                opt.value = allLabels[li].id;
                opt.textContent = allLabels[li].className;

                if (allLabels[li].className === response.predictedLabel) opt.selected = true;

                select_divSaveCustomImage.appendChild(opt);

            }

            var divSaveCustomImage = document.getElementById("divSaveCustomImage");
            divSaveCustomImage.appendChild(span_divSaveCustomImage)
            divSaveCustomImage.appendChild(select_divSaveCustomImage);

            return response;
        });
});