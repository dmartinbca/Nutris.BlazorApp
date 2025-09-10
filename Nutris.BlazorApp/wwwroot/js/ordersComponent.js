window.OrdersComponentHelper = {
    initializeTooltips: function () {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        [...tooltipTriggerList].forEach(tooltipTriggerEl => {
            if (tooltipTriggerEl._bootstrapTooltip) {
                tooltipTriggerEl._bootstrapTooltip.dispose();
            }
            const tooltip = new bootstrap.Tooltip(tooltipTriggerEl, {
                trigger: 'hover',
                placement: 'top',
                html: true
            });
            tooltipTriggerEl._bootstrapTooltip = tooltip;
        });
    },

    destroyTooltips: function () {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        [...tooltipTriggerList].forEach(tooltipTriggerEl => {
            if (tooltipTriggerEl._bootstrapTooltip) {
                tooltipTriggerEl._bootstrapTooltip.dispose();
                delete tooltipTriggerEl._bootstrapTooltip;
            }
        });
    },

    toggleContainer: function (containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        container.classList.toggle('active');

        // Guardar estado en localStorage
        const isActive = container.classList.contains('active');
        localStorage.setItem(containerId, isActive ? 'open' : 'closed');

        return isActive;
    },

    restoreContainerStates: function () {
        ['container-toggle-1', 'container-toggle-2', 'container-toggle-3', 'container-toggle-4', 'container-toggle-5'].forEach(id => {
            const state = localStorage.getItem(id);
            const container = document.getElementById(id);
            if (container && state === 'open') {
                container.classList.add('active');
            }
        });
    },

    downloadBase64File: function (base64String, fileName) {
        try {
            const byteCharacters = atob(base64String);
            const byteNumbers = new Array(byteCharacters.length);

            for (let i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
            }

            const byteArray = new Uint8Array(byteNumbers);

            // Detectar tipo de archivo
            let mimeType = 'application/octet-stream';
            let fileExtension = fileName.toLowerCase().split('.').pop();

            if (fileExtension === 'pdf') {
                mimeType = 'application/pdf';
            } else if (fileExtension === 'doc') {
                mimeType = 'application/msword';
            } else if (fileExtension === 'docx') {
                mimeType = 'application/vnd.openxmlformats-officedocument.wordprocessingml.document';
            } else if (fileExtension === 'xls') {
                mimeType = 'application/vnd.ms-excel';
            } else if (fileExtension === 'xlsx') {
                mimeType = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
            }

            const blob = new Blob([byteArray], { type: mimeType });

            if (fileExtension === 'pdf') {
                const url = window.URL.createObjectURL(blob);
                window.open(url, '_blank');
                setTimeout(() => {
                    window.URL.revokeObjectURL(url);
                }, 1000);
            } else {
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = fileName;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                window.URL.revokeObjectURL(url);
            }
        } catch (error) {
            console.error('Error al procesar archivo:', error);
        }
    },

    setCustomerLogo: function (base64Logo) {
        if (base64Logo) {
            localStorage.setItem('Customer_logo', base64Logo);
        }
    },

    getCustomerLogo: function () {
        return localStorage.getItem('Customer_logo') || '';
    }
};