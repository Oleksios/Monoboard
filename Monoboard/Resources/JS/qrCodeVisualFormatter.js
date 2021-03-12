var elem = document.querySelectorAll("a[href='/docs/']")[0];
elem.parentNode.removeChild(elem);

document.querySelectorAll("span[class='i18n']")[0].innerHTML = "Відскануйте QR код для входу";