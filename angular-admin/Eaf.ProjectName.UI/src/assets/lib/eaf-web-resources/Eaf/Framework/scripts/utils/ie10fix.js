//Windows Phone 8 and Internet Explorer 10 FIX
if (navigator.userAgent.match(/IEMobile\/10\.0/)) { // NOSONAR
    var msViewportStyle = document.createElement("style"); // NOSONAR
    msViewportStyle.appendChild(
        document.createTextNode(
            "@-ms-viewport{width:auto!important}"
        )
    );

    document.getElementsByTagName("head")[0].appendChild(msViewportStyle);
}
