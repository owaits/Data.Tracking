
window.closeModal = (modalId) => {
    $(modalId).modal('hide');
};

window.openModal = (modalId) => {
    $(modalId).modal('show');
};