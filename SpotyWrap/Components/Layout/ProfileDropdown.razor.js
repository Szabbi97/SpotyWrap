let dotNetRef = null;
let containerElement = null;
let clickHandler = null;

export function initialize(element, dotNetReference) {
    dotNetRef = dotNetReference;
    containerElement = element;

    clickHandler = (event) => {
        if (containerElement && !containerElement.contains(event.target)) {
            dotNetRef.invokeMethodAsync('CloseDropdown');
        }
    };

    document.addEventListener('click', clickHandler, true);
}

export function dispose() {
    if (clickHandler) {
        document.removeEventListener('click', clickHandler, true);
        clickHandler = null;
    }
    dotNetRef = null;
    containerElement = null;
}
