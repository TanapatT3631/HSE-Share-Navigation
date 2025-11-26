let showImage2 = (src, title = '', isMaxHeight = false) => {
    let id = 'ShowImage2Modal';
    let elementId = $('#' + id);
    if (elementId !== undefined) {
        elementId.remove();
    }
    let modal = document.createElement('div');
    modal.id = id;
    modal.classList.add('ui', 'basic', 'modal');
    let icon = document.createElement('i');
    icon.classList.add('close', 'icon');
    let header = document.createElement('div');
    header.classList.add('center', 'aligned', 'header');
    header.innerHTML = title;
    let content = document.createElement('div');
    content.classList.add('center', 'aligned', 'content');
    let image = document.createElement('img');
    image.classList.add('ui', 'centered', 'image');
    image.src = src;
    if (isMaxHeight) {
        image.style.maxHeight = "450px";
    }
    content.append(image);
    modal.append(icon);
    modal.append(header);
    modal.append(content);
    $('body').append(modal);
    $('#' + id).modal({
        allowMultiple: true
    }).modal('show');
}