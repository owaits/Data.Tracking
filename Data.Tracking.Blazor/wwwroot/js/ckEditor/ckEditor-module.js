//import './ckeditor.js';

export async function initialiseCKEditor(elementId, dotNetReference) {
    var editor = CKEDITOR.replace(elementId);

    editor.on('change', function () {
        let data = editor.getData();
        dotNetReference.invokeMethodAsync('EditorDataChanged', data);
    });

    return editor;
}

export async function disposeCKEditor(editor) {
    if (typeof (editor) != 'undefined' && editor != null)
        editor.destroy();
}