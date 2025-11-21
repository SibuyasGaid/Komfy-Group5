// KOZY Library Management System - UI Interactions

(function() {
    'use strict';

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', function() {
        initSidebar();
        initAnimations();
        initTooltips();
        initModals();
        initNotifications();
    });

    // Sidebar Toggle
    function initSidebar() {
        const sidebar = document.querySelector('.kozy-sidebar');
        const sidebarToggle = document.querySelector('.kozy-sidebar-toggle');
        const mainContent = document.querySelector('.kozy-main-content');
        const header = document.querySelector('.kozy-header');
        const mobileOverlay = document.querySelector('.kozy-mobile-overlay');

        if (sidebarToggle) {
            sidebarToggle.addEventListener('click', function() {
                if (window.innerWidth <= 768) {
                    // Mobile: toggle sidebar with overlay
                    sidebar.classList.toggle('mobile-open');
                    if (mobileOverlay) {
                        mobileOverlay.classList.toggle('active');
                    }
                } else {
                    // Desktop: collapse/expand sidebar
                    sidebar.classList.toggle('collapsed');
                    if (mainContent) {
                        mainContent.classList.toggle('sidebar-collapsed');
                    }
                    if (header) {
                        header.classList.toggle('sidebar-collapsed');
                    }
                }
            });
        }

        // Close sidebar when clicking overlay on mobile
        if (mobileOverlay) {
            mobileOverlay.addEventListener('click', function() {
                sidebar.classList.remove('mobile-open');
                mobileOverlay.classList.remove('active');
            });
        }

        // Close sidebar on mobile when clicking outside
        document.addEventListener('click', function(e) {
            if (window.innerWidth <= 768 && sidebar && sidebar.classList.contains('mobile-open')) {
                if (!sidebar.contains(e.target) && !sidebarToggle.contains(e.target)) {
                    sidebar.classList.remove('mobile-open');
                    if (mobileOverlay) {
                        mobileOverlay.classList.remove('active');
                    }
                }
            }
        });

        // Handle window resize
        window.addEventListener('resize', function() {
            if (window.innerWidth > 768) {
                sidebar.classList.remove('mobile-open');
                if (mobileOverlay) {
                    mobileOverlay.classList.remove('active');
                }
            }
        });
    }

    // Page Load Animations
    function initAnimations() {
        // Animate elements on scroll
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver(function(entries) {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-fade-in-up');
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        // Observe all cards and sections
        document.querySelectorAll('.kozy-card, .kozy-book-card, .kozy-genre-card').forEach(el => {
            observer.observe(el);
        });

        // Stagger animation for grid items
        const gridItems = document.querySelectorAll('.kozy-book-grid .kozy-book-card');
        gridItems.forEach((item, index) => {
            item.style.animationDelay = `${index * 0.1}s`;
        });
    }

    // Tooltips
    function initTooltips() {
        const tooltipElements = document.querySelectorAll('[data-tooltip]');
        tooltipElements.forEach(element => {
            element.addEventListener('mouseenter', showTooltip);
            element.addEventListener('mouseleave', hideTooltip);
        });
    }

    function showTooltip(e) {
        const text = e.target.getAttribute('data-tooltip');
        const tooltip = document.createElement('div');
        tooltip.className = 'kozy-tooltip';
        tooltip.textContent = text;
        document.body.appendChild(tooltip);

        const rect = e.target.getBoundingClientRect();
        tooltip.style.left = rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2) + 'px';
        tooltip.style.top = rect.top - tooltip.offsetHeight - 8 + 'px';
    }

    function hideTooltip() {
        const tooltip = document.querySelector('.kozy-tooltip');
        if (tooltip) {
            tooltip.remove();
        }
    }

    // Enhanced Modal Handling
    function initModals() {
        const modals = document.querySelectorAll('.kozy-modal');
        modals.forEach(modal => {
            const closeBtn = modal.querySelector('.kozy-modal-close');
            const backdrop = modal.querySelector('.kozy-modal-backdrop');

            if (closeBtn) {
                closeBtn.addEventListener('click', () => closeModal(modal));
            }

            if (backdrop) {
                backdrop.addEventListener('click', () => closeModal(modal));
            }
        });
    }

    function closeModal(modal) {
        modal.classList.remove('active');
        document.body.style.overflow = '';
    }

    // Notification System
    function initNotifications() {
        // Enhanced toast notifications
        window.showKozyToast = function(message, type = 'info', duration = 3000) {
            const toast = document.createElement('div');
            toast.className = `kozy-toast kozy-toast-${type} toast-enter`;
            toast.innerHTML = `
                <div class="kozy-toast-content">
                    <i class="fa-solid ${getToastIcon(type)}"></i>
                    <span>${message}</span>
                </div>
            `;

            const container = document.querySelector('.kozy-toast-container') || createToastContainer();
            container.appendChild(toast);

            setTimeout(() => {
                toast.classList.add('toast-exit');
                setTimeout(() => toast.remove(), 300);
            }, duration);
        };

        function getToastIcon(type) {
            const icons = {
                success: 'fa-check-circle',
                error: 'fa-exclamation-circle',
                warning: 'fa-exclamation-triangle',
                info: 'fa-info-circle'
            };
            return icons[type] || icons.info;
        }

        function createToastContainer() {
            const container = document.createElement('div');
            container.className = 'kozy-toast-container';
            document.body.appendChild(container);
            return container;
        }
    }

    // Smooth Scroll to Top
    window.scrollToTop = function() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    };

    // Form Validation Enhancement
    function enhanceFormValidation() {
        const forms = document.querySelectorAll('form');
        forms.forEach(form => {
            form.addEventListener('submit', function(e) {
                const inputs = form.querySelectorAll('input[required], textarea[required], select[required]');
                let isValid = true;

                inputs.forEach(input => {
                    if (!input.value.trim()) {
                        isValid = false;
                        input.classList.add('error');
                    } else {
                        input.classList.remove('error');
                    }
                });

                if (!isValid) {
                    e.preventDefault();
                    showKozyToast('Please fill in all required fields', 'warning');
                }
            });
        });
    }

    // Initialize form validation
    enhanceFormValidation();

    // Search Enhancement
    function initSearch() {
        const searchInputs = document.querySelectorAll('.kozy-search-input');
        searchInputs.forEach(input => {
            input.addEventListener('focus', function() {
                this.parentElement.classList.add('focused');
            });

            input.addEventListener('blur', function() {
                this.parentElement.classList.remove('focused');
            });
        });
    }

    initSearch();

    // Lazy Loading for Images
    function initLazyLoading() {
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        if (img.dataset.src) {
                            img.src = img.dataset.src;
                            img.removeAttribute('data-src');
                            observer.unobserve(img);
                        }
                    }
                });
            });

            document.querySelectorAll('img[data-src]').forEach(img => {
                imageObserver.observe(img);
            });
        }
    }

    initLazyLoading();

    // Export functions for global use
    window.KozyUI = {
        showToast: window.showKozyToast,
        scrollToTop: window.scrollToTop
    };
})();

