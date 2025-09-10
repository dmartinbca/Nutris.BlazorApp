// orders-component-helper.js
window.OrdersComponentHelper = {
    // Inicializar tooltips de Bootstrap
    initializeTooltips: function () {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    },

    // Guardar estado de contenedores
    toggleContainer: function (containerId) {
        const container = document.getElementById(containerId);
        if (container) {
            const isOpen = container.classList.contains('active');
            if (isOpen) {
                container.classList.remove('active');
                localStorage.setItem(containerId, 'closed');
            } else {
                container.classList.add('active');
                localStorage.setItem(containerId, 'open');
            }
        }
    },

    // Restaurar estados de contenedores
    restoreContainerStates: function () {
        for (let i = 1; i <= 5; i++) {
            const containerId = `container-toggle-${i}`;
            const state = localStorage.getItem(containerId);
            const container = document.getElementById(containerId);

            if (container) {
                if (state === 'open') {
                    container.classList.add('active');
                } else if (state === 'closed') {
                    container.classList.remove('active');
                }
            }
        }
    },

    // Guardar logo del cliente
    setCustomerLogo: function (base64Logo) {
        if (base64Logo) {
            localStorage.setItem('customerLogo', base64Logo);
        }
    },

    // Obtener logo del cliente
    getCustomerLogo: function () {
        return localStorage.getItem('customerLogo');
    },

    // Descargar archivo base64
    downloadBase64File: function (base64Data, fileName) {
        const link = document.createElement('a');
        link.href = 'data:application/octet-stream;base64,' + base64Data;
        link.download = fileName;
        link.click();
    }
};

// Función para mostrar toasts
window.showToast = function (message, type = 'success') {
    // Crear contenedor de toasts si no existe
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }

    // Crear el toast
    const toastId = 'toast-' + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-white bg-${type === 'success' ? 'success' : 'danger'} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);

    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, {
        autohide: true,
        delay: 5000
    });

    toast.show();

    // Limpiar el toast después de que se oculte
    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
};

// Función para manejar modales de Bootstrap
window.showBootstrapModal = function (modalId) {
    const modal = new bootstrap.Modal(document.getElementById(modalId));
    modal.show();
};

window.hideBootstrapModal = function (modalId) {
    const modalElement = document.getElementById(modalId);
    if (modalElement) {
        const modal = bootstrap.Modal.getInstance(modalElement);
        if (modal) {
            modal.hide();
        }
    }
};