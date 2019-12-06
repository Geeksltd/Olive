/**
 * @license Copyright (c) 2003-2014, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.html or http://ckeditor.com/license
 */
CKEDITOR.editorConfig = function (config) {
    /*
    To see list of all available toolbar items, go to
    http://docs.cksource.com/CKEditor_3.x/Developers_Guide/Toolbar#Toolbar_Definition
    */
    config.toolbar_Compact =
        [
            { name: 'basicstyles', items: ['Bold', 'Italic'] },
            { name: 'paragraph', items: ['NumberedList', 'BulletedList'] }
        ];
    config.toolbar_Medium =
        [
            { name: 'styles', items: ['Styles', 'Format'] },
            { name: 'basicstyles', items: ['Bold', 'Italic', 'Strike', '-', 'RemoveFormat'] },
            { name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Blockquote'] },
            { name: 'links', items: ['Link', 'Unlink', 'Anchor'] }
        ];
    config.toolbar_Advance =
        [
            { name: 'source', items: ['Source'] },
            { name: 'clipboard', items: ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'] },
            { name: 'editing', items: ['Find', 'Replace', '-', 'SelectAll', '-', 'Scayt'] },
            {
                name: 'insert', items: ['Image', 'Flash', 'Table', 'HorizontalRule', 'Smiley', 'SpecialChar', 'PageBreak',
                    'Iframe']
            },
            '/',
            { name: 'styles', items: ['Styles', 'Format'] },
            { name: 'basicstyles', items: ['Bold', 'Italic', 'Strike', '-', 'RemoveFormat'] },
            { name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'BidiLtr', 'BidiRtl'] },
            { name: 'links', items: ['Link', 'Unlink', 'Anchor'] }
        ];
    config.toolbar_ContractTemplate =
        [
            { name: 'styles', items: ['Format'] },
            { name: 'clipboard', items: ['PasteText'] },
            { name: 'basicstyles', items: ['Bold', 'Italic', 'Strike', '-', 'RemoveFormat'] },
            { name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'HorizontalRule', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', '-', 'TextColor', 'BGColor'] },
            { name: 'links', items: ['Link', 'Unlink', 'Anchor'] }
        ];
};
//# sourceMappingURL=ckeditor_config.js.map