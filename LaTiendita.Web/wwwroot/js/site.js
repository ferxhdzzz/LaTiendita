// Helpers globales de seguridad (CSRF y prevención de XSS en DOM)
function getCsrfToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
}

function escapeHtml(texto) {
    if (texto === null || texto === undefined) return '';
    return String(texto)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}
