// Asegúrate de cargar bootstrap.bundle antes de este archivo
window.modalHelper = {
    getInstance(id, opts) {
        const el = document.getElementById(id);
        if (!el) return null;
        return bootstrap.Modal.getOrCreateInstance(el, opts || {});
    },

    // Mostrar asegurando que no queden restos de aperturas anteriores
    safeShow(id) {
        const m = this.getInstance(id, { backdrop: 'static', keyboard: false });
        if (!m) return;
        this.cleanupBackdrops();
        m.show();
    },

    // Ocultar sin dejar el overlay colgado
    safeHide(id) {
        const el = document.getElementById(id);
        if (!el) return;
        const m = bootstrap.Modal.getInstance(el);
        if (m) m.hide();
        this.ensureHidden(id);
    },

    // Forzar que el DOM quede “limpio”
    ensureHidden(id) {
        const el = document.getElementById(id);
        if (!el) return;
        el.classList.remove('show');
        el.setAttribute('aria-hidden', 'true');
        el.style.display = 'none';
        this.cleanupBackdrops();
    },

    cleanupBackdrops() {
        document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
        document.body.classList.remove('modal-open');
        document.body.style.removeProperty('overflow');
        document.body.style.removeProperty('paddingRight');
        document.body.style.removeProperty('padding-right');
    }
};
