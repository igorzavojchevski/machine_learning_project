//const serviceUrl = 'https://localhost:44380/api/ImageClassification/classifyImage';

//const serviceUrl = 'http://localhost:5000/api/ImageClassification/classifyImage';

const serviceUrl = 'api/ImageClassification';
const form = document.querySelector('form');

function loadImages()
{
    fetch(serviceUrl + "/GetAllImages") // Call the fetch function passing the url of the API as a parameter
        .then((resp) => resp.json())
        .then(function (response) {

            console.log(response);

            for (var i = 0; i < response.length; i++) {
                var divAdvertisements = document.getElementById("divAdvertisements");
                var x = document.createElement("div");
                var id = "advertisement" + i;
                x.setAttribute("id", id);
                var h3 = document.createElement("h3");
                h3.textContent = response[i].predictedLabel;

                x.appendChild(h3);
                divAdvertisements.appendChild(x);

                for (var j = 0; j < response[i].advertisements.length; j++) {
                    //var y = document.createElement("div");
                    //var idy = "advertisementImage" + j;
                    //y.setAttribute("id", idy);
                    var imageId = response[i].advertisements[j].imageId;
                    var figure = document.createElement("figure");
                    figure.style.height = "200px";
                    figure.style.width = "200px";

                    var img = document.createElement("img");
                    var idimg = "advertisementImageIMG" + j;
                    img.setAttribute("id", idimg);
                    img.setAttribute("src", "images_to_train/" + response[i].predictedLabel + "/" + imageId);
                    img.setAttribute("height", "200px");
                    img.setAttribute("width", "200px");
                    //y.appendChild(img);
                    //divAdvertisements.appendChild(y);
                    figure.style.display = "inline-block";
                    figure.style.marginRight = "5px";

                    figure.appendChild(img);

                    var figCaption = document.createElement("figcaption");
                    figCaption.textContent = (response[i].advertisements[j].maxProbability * 100).toFixed(3) + "%";
                    figCaption.style.textAlign = "center";
                    figure.appendChild(figCaption);

                    x.appendChild(figure);
                }
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