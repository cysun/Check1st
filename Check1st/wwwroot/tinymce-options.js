var tinymceOptions = {};

tinymceOptions.basic = (selector, height) => ({
    selector: selector,
    plugins: "autolink code image link lists table wordcount fullscreen",
    menubar: false,
    convert_urls: false,
    browser_spellcheck: true,
    toolbar: "bold italic underline bullist numlist link image code fullscreen",
    height: height ? height : 350
});
