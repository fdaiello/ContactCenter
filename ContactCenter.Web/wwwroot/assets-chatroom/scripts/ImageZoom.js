// Get the modal
var imgModal = document.getElementById("zoomImageModal");

// Get the image and insert it inside the modal - use its "alt" text as a caption
function ImageZoom (e) {
    var modalImg = document.getElementById("img01");
    var captionText = document.getElementById("caption");
    imgModal.style.display = "block";
    modalImg.src = e.src;
    captionText.innerHTML = e.alt;
}

// Get the <span> element that closes the modal
var span = document.getElementsByClassName("closeZoomImage")[0];

// When the user clicks on <span> (x), close the modal
span.onclick = function () {
    imgModal.style.display = "none";
}