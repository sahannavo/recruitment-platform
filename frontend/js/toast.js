// ============================================
// js/toast.js - Toast Notification System
// ============================================

(function () {
    'use strict';

    let toastTimer = null;
    let toastElement = null;

    // Create toast element if it doesn't exist
    function getToastElement() {
        if (toastElement) return toastElement;

        toastElement = document.createElement('div');
        toastElement.id = 'toast';
        toastElement.className = 'toast';
        toastElement.style.cssText = `
            position: fixed;
            bottom: 24px;
            left: 50%;
            transform: translateX(-50%) translateY(100px);
            background: var(--text-primary, #111827);
            color: #fff;
            padding: 12px 24px;
            border-radius: 8px;
            font-size: 14px;
            font-weight: 500;
            display: flex;
            align-items: center;
            gap: 10px;
            opacity: 0;
            pointer-events: none;
            transition: opacity 0.3s ease, transform 0.3s ease;
            z-index: 9999;
            box-shadow: 0 10px 25px rgba(0,0,0,0.15);
            max-width: 90%;
        `;
        document.body.appendChild(toastElement);
        return toastElement;
    }

    // Main toast function
    window.showToast = function (message, type = 'info') {
        const toast = getToastElement();
        const iconMap = {
            success: '✅',
            error: '❌',
            warning: '⚠️',
            info: 'ℹ️'
        };

        const icon = iconMap[type] || 'ℹ️';
        toast.innerHTML = `<span>${icon}</span><span>${message}</span>`;

        // Set color based on type
        const colors = {
            success: 'var(--success, #10b981)',
            error: 'var(--danger, #ef4444)',
            warning: 'var(--warning, #f59e0b)',
            info: 'var(--primary, #4f46e5)'
        };
        toast.style.borderLeft = `4px solid ${colors[type] || colors.info}`;

        // Show toast
        toast.style.opacity = '1';
        toast.style.transform = 'translateX(-50%) translateY(0)';
        toast.style.pointerEvents = 'auto';

        // Auto-hide after 3 seconds
        clearTimeout(toastTimer);
        toastTimer = setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(-50%) translateY(100px)';
            toast.style.pointerEvents = 'none';
        }, 3000);
    };

    // Close toast on click
    document.addEventListener('click', function (e) {
        const toast = getToastElement();
        if (toast.style.opacity === '1' && toast.contains(e.target)) {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(-50%) translateY(100px)';
            toast.style.pointerEvents = 'none';
            clearTimeout(toastTimer);
        }
    });

    console.log('✅ Toast system initialized');
})();