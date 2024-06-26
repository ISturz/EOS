/*Word Counter*/
	function letterCount(object){
        document.getElementById("letterCountValue").innerHTML = object.value.length + '/500';
}

/*Back to top btn*/
let mybutton = document.getElementById("topBTN");
function topFunction() {
    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;
}

/*Remove email btn*/
function removeEmail(button) {
    // Get the form associated with the clicked "Remove" button
    var form = button.closest(".deleteGenForm");

    // Confirm deletion
    if (confirm("Are you sure you want to remove this email?")) {
        form.submit();
    }
}

/*Copy email address*/
function copyEmailToClipboard(clickedspan) {
    const el = document.createElement('textarea');
    el.value = clickedspan.innerText;
    document.body.appendChild(el);
    el.select();
    document.execCommand('copy');
    document.body.removeChild(el);
    alert("Text copied to clipboard: " + clickedspan.innerText);
}