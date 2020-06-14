//const serviceUrl = 'https://localhost:44380/api/ImageClassification/classifyImage';

//const serviceUrl = 'http://localhost:5000/api/ImageClassification/classifyImage';

const serviceUrl = 'api/ImageClassification/classifyImage';
const form = document.querySelector('form');

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


    fetch(serviceUrl, {
        method: 'POST',
        body: formData
    }).then((resp) => resp.json())
        .then(function (response) {
            console.info('Response', response);
            console.log('Response', response);

            console.log('Prediction is: ' + 'Label: ' + response.predictedLabel + ' Probability: ' + response.probability);

            document.getElementById('divPrediction').innerHTML = "Predicted label is: " + response.predictedLabel;
            document.getElementById('divProbability').innerHTML = "Probability is: " + (response.maxProbability * 100).toFixed(3) + "%";

            //document.getElementById('listProbabilities').innerHTML = response.d;
            console.log(typeof(response.allProbabilities));

            var allProbabilityArray = Object.keys(response.allProbabilities).map(function (key) {
                return [String(key), response.allProbabilities[key]];
            });

            for (var i = 0; i < allProbabilityArray.length; i++) {
                var table = document.getElementById("tableProbabilities");
                var row = table.insertRow(0);
                var cell1 = row.insertCell(0);
                var cell2 = row.insertCell(1);
                cell1.innerHTML = allProbabilityArray[i][0];
                cell2.innerHTML = ((allProbabilityArray[i][1] * 100).toFixed(3) + "%");
            }


            return response;
        });


});