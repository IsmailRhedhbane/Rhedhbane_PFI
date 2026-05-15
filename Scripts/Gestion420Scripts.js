$(document).ready(function () {
    if ($.fn.mask) {
        $('.phone').mask('(999) 999-9999');
        $('.phoneExt').mask('(999) 999-9999 poste 99999');
        $('.zipcode').mask('a9a 9a9');
    }

    if ($.fn.datepicker) {
        $('.datepicker').datepicker({
            dateFormat: 'yy-mm-dd',
            changeMonth: true,
            changeYear: true,
            dayNamesMin: ['Dim', 'Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam'],
            monthNamesShort: ['Janv.', 'Févr.', 'Mars', 'Avril', 'Mai', 'Juin', 'Juil.', 'Août', 'Sept.', 'Oct.', 'Nov.', 'Déc.']
        });
    }
});

let minKeywordLenth = 1;

function highlight(text, elem) {
    text = text.trim();
    if (text.length >= minKeywordLenth) {
        let originalText = $(elem).text();
        let normalizedText = text.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
        let normalizedOriginal = originalText.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
        let index = normalizedOriginal.indexOf(normalizedText);

        if (index >= 0) {
            let before = originalText.substring(0, index);
            let match = originalText.substring(index, index + text.length);
            let after = originalText.substring(index + text.length);
            $(elem).html(before + "<span class='highlight'>" + match + '</span>' + after);
        }
    }
}
